using App.Core;
using App.Core.Domain.Extension.Leaves;
using App.Data;
using App.Services.Employees;
using App.Services.Leaves;
using App.Services.Logging;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class EmployeeSalaryService : IEmployeeSalaryService
{
    #region Fields

    protected readonly IRepository<EmployeeMonthlySalary> _salaryRepository;
    protected readonly IRepository<Expense> _expenseRepository;
    protected readonly IRepository<AccountTransaction> _accountTransactionRepository;
    protected readonly IRepository<EmployeeSalaryAuditLog> _auditLogRepository;
    protected readonly IEmployeeService _employeeService;
    protected readonly ILeaveManagementService _leaveManagementService;
    protected readonly ILeaveTransactionLogService _leaveTransactionLogService;
    protected readonly ILeaveTypeService _leaveTypeService;
    protected readonly ISalaryComponentConfigService _salaryComponentConfigService;
    protected readonly ILogger _logger;
    protected readonly ExpenseManagementSettings _expenseManagementSettings;
    protected readonly LeaveSettings _leaveSettings;

    #endregion

    #region Ctor

    public EmployeeSalaryService(IRepository<EmployeeMonthlySalary> salaryRepository,
        IRepository<Expense> expenseRepository,
        IRepository<AccountTransaction> accountTransactionRepository,
        IRepository<EmployeeSalaryAuditLog> auditLogRepository,
        IEmployeeService employeeService,
        ILeaveManagementService leaveManagementService,
        ILeaveTransactionLogService leaveTransactionLogService,
        ILeaveTypeService leaveTypeService,
        ISalaryComponentConfigService salaryComponentConfigService,
        ILogger logger,
        ExpenseManagementSettings expenseManagementSettings,
        LeaveSettings leaveSettings)
    {
        _salaryRepository = salaryRepository;
        _expenseRepository = expenseRepository;
        _accountTransactionRepository = accountTransactionRepository;
        _auditLogRepository = auditLogRepository;
        _employeeService = employeeService;
        _leaveManagementService = leaveManagementService;
        _leaveTransactionLogService = leaveTransactionLogService;
        _leaveTypeService = leaveTypeService;
        _salaryComponentConfigService = salaryComponentConfigService;
        _logger = logger;
        _expenseManagementSettings = expenseManagementSettings;
        _leaveSettings = leaveSettings;
    }

    #endregion

    #region Utilities

    protected virtual decimal ParseCTC(string ctc)
    {
        if (string.IsNullOrWhiteSpace(ctc))
            return 0;

        ctc = ctc.Trim().Replace("₹", "").Replace(",", "").Replace(" ", "").ToUpper();

        if (ctc.EndsWith("LPA"))
        {
            ctc = ctc.Replace("LPA", "");
            if (decimal.TryParse(ctc, out var v))
                return v * 100000m / 12m;
            return 0;
        }

        if (ctc.EndsWith("L"))
        {
            ctc = ctc.Replace("L", "");
            if (decimal.TryParse(ctc, out var v))
                return v * 100000m / 12m;
            return 0;
        }

        return decimal.TryParse(ctc, out var result) ? result / 12m : 0;
    }

    protected virtual DateTime CalculateLastDayOfMonth(int month, int year)
    {
        return new DateTime(year, month, DateTime.DaysInMonth(year, month));
    }

    protected virtual string GetMonthName(int month)
    {
        return new DateTime(2000, month, 1).ToString("MMMM");
    }

    #endregion

    #region Methods

    public virtual async Task InsertSalaryRecordAsync(EmployeeMonthlySalary salaryRecord)
    {
        ArgumentNullException.ThrowIfNull(salaryRecord);
        salaryRecord.CreatedOnUtc = DateTime.UtcNow;
        salaryRecord.UpdatedOnUtc = DateTime.UtcNow;
        await _salaryRepository.InsertAsync(salaryRecord);
    }

    public virtual async Task UpdateSalaryRecordAsync(EmployeeMonthlySalary salaryRecord)
    {
        ArgumentNullException.ThrowIfNull(salaryRecord);
        salaryRecord.UpdatedOnUtc = DateTime.UtcNow;
        await _salaryRepository.UpdateAsync(salaryRecord);

        if (salaryRecord.StatusId == (int)SalaryStatusEnum.Paid)
        {
            var linkedExpenses = await _expenseRepository.GetAllAsync(query =>
                query.Where(e => !e.Deleted && e.EmployeeSalaryRecordId == salaryRecord.Id));
            var expense = linkedExpenses.FirstOrDefault();
            if (expense != null)
            {
                expense.Amount = salaryRecord.NetSalary;
                expense.UpdatedOnUtc = DateTime.UtcNow;
                await _expenseRepository.UpdateAsync(expense);
            }

            if (salaryRecord.AccountTransactionId > 0)
            {
                var transaction = await _accountTransactionRepository.GetByIdAsync(salaryRecord.AccountTransactionId);
                if (transaction != null && !transaction.Deleted)
                {
                    transaction.Amount = salaryRecord.NetSalary;
                    transaction.UpdatedOnUtc = DateTime.UtcNow;
                    await _accountTransactionRepository.UpdateAsync(transaction);
                }
            }
        }
    }

    public virtual async Task DeleteSalaryRecordAsync(EmployeeMonthlySalary salaryRecord)
    {
        ArgumentNullException.ThrowIfNull(salaryRecord);

        var linkedExpenses = await _expenseRepository.GetAllAsync(query =>
            query.Where(e => !e.Deleted && e.EmployeeSalaryRecordId == salaryRecord.Id));
        var linkedExpense = linkedExpenses.FirstOrDefault();
        if (linkedExpense != null)
        {
            linkedExpense.Deleted = true;
            linkedExpense.UpdatedOnUtc = DateTime.UtcNow;
            await _expenseRepository.UpdateAsync(linkedExpense);
        }

        if (salaryRecord.AccountTransactionId > 0)
        {
            var linkedTransaction = await _accountTransactionRepository.GetByIdAsync(salaryRecord.AccountTransactionId);
            if (linkedTransaction != null && !linkedTransaction.Deleted)
            {
                linkedTransaction.Deleted = true;
                linkedTransaction.UpdatedOnUtc = DateTime.UtcNow;
                await _accountTransactionRepository.UpdateAsync(linkedTransaction);
            }
        }

        await _salaryRepository.DeleteAsync(salaryRecord);
    }

    public virtual async Task<EmployeeMonthlySalary> GetSalaryRecordByIdAsync(int id)
    {
        return await _salaryRepository.GetByIdAsync(id);
    }

    public virtual async Task<EmployeeMonthlySalary> GetSalaryRecordAsync(int employeeId, int monthId, int yearId)
    {
        var records = await _salaryRepository.GetAllAsync(query =>
            query.Where(s => s.EmployeeId == employeeId && s.MonthId == monthId && s.YearId == yearId));
        return records.FirstOrDefault();
    }

    public virtual async Task<EmployeeMonthlySalary> GetSalaryRecordByAccountTransactionIdAsync(int accountTransactionId)
    {
        if (accountTransactionId <= 0)
            return null;

        var records = await _salaryRepository.GetAllAsync(query =>
            query.Where(s => s.AccountTransactionId == accountTransactionId));
        return records.FirstOrDefault();
    }

    public virtual async Task<IPagedList<EmployeeMonthlySalary>> GetAllSalaryRecordsAsync(int employeeId = 0, int monthId = 0, int yearId = 0, int statusId = 0, string searchEmployeeIds = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var employeeIdList = string.IsNullOrEmpty(searchEmployeeIds)
            ? new List<int>()
            : searchEmployeeIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

        return await _salaryRepository.GetAllPagedAsync(query =>
        {
            if (employeeId > 0)
                query = query.Where(s => s.EmployeeId == employeeId);
            if (employeeIdList.Count > 0)
                query = query.Where(s => employeeIdList.Contains(s.EmployeeId));
            if (monthId > 0)
                query = query.Where(s => s.MonthId == monthId);
            if (yearId > 0)
                query = query.Where(s => s.YearId == yearId);
            if (statusId > 0)
                query = query.Where(s => s.StatusId == statusId);
            query = query.OrderByDescending(s => s.YearId).ThenByDescending(s => s.MonthId).ThenBy(s => s.EmployeeId);
            return query;
        }, pageIndex, pageSize);
    }

    public virtual async Task ProcessMonthlySalariesAsync(int monthId, int yearId, int submittedByEmployeeId = 0)
    {
        var employees = await _employeeService.GetAllEmployeesAsync(showInActive: false, showVendors: true);

        var firstDay = new DateTime(yearId, monthId, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);

        var leaveTypeId = _leaveSettings.SeletedLeaveTypeId > 0 ? _leaveSettings.SeletedLeaveTypeId : 1;

        var components = await _salaryComponentConfigService.GetAllActiveComponentsAsync();

        foreach (var employee in employees)
        {
            try
            {
                var payrollInfo = await _salaryComponentConfigService.GetEmployeePayrollInfoAsync(employee.Id);
                var grossSalary = ParseCTC(payrollInfo?.CTC);
                if (grossSalary <= 0)
                {
                    await _logger.WarningAsync($"[SalaryProcessing] Employee {employee.Id} ({employee.FirstName} {employee.LastName}) has invalid or empty CTC: '{payrollInfo?.CTC}'. Skipped.");
                    continue;
                }

                var workingDays = await _leaveManagementService.GetDifferenceByFromToAsync(firstDay, lastDay);
                if (workingDays <= 0)
                    workingDays = 1;

                var dailySalary = Math.Round(grossSalary / workingDays, 4);

                var deductionDays = 0m;
                var leaveEncashmentDays = 0m;
                var leaveEncashmentAmount = 0m;

                if (!employee.IsVendor)
                {
                    var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLogForCurrentMonth(employee.Id, leaveTypeId, monthId, yearId);
                    var leaveBalance = leaveLog?.LeaveBalance ?? 0m;
                    deductionDays = leaveBalance < 0 ? Math.Abs(leaveBalance) : 0m;

                    var allLeaveTypes = await _leaveTypeService.GetAllLeaveTypeAsync("");
                    foreach (var lt in allLeaveTypes)
                    {
                        if (lt.Id == leaveTypeId)
                            continue; // already handled above

                        var otherLog = await _leaveTransactionLogService.GetLeaveBalanceByLogForCurrentMonth(employee.Id, lt.Id, monthId, yearId);
                        var otherBalance = otherLog?.LeaveBalance ?? 0m;
                        if (otherBalance < 0)
                            deductionDays += Math.Abs(otherBalance);
                    }

                    // December leave encashment: use the ACTUAL current balance (most recent log, any month).
                    // GetLeaveBalanceByLogForCurrentMonth(12) only finds entries with BalanceMonthYear=December
                    // (just the monthly increment). Leave deductions are stored under earlier months' BalanceMonthYear,
                    // so GetLeaveBalanceByLog (no month filter) gives the true remaining balance.
                    if (monthId == 12)
                    {
                        var latestLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employee.Id, leaveTypeId);
                        var actualBalance = latestLog?.LeaveBalance ?? 0m;
                        if (actualBalance > 0)
                        {
                            leaveEncashmentDays = actualBalance;
                            leaveEncashmentAmount = Math.Round(actualBalance * dailySalary, 4);
                        }
                    }
                }

                var deductionAmount = Math.Round(deductionDays * dailySalary, 4);

                var configDeductionsTotal = components?
                    .Where(c => !c.IsRemainder && c.ComponentTypeId == (int)SalaryComponentTypeEnum.Deduction)
                    .Sum(c => c.IsPercentage ? Math.Round(grossSalary * c.Value / 100m, 2) : c.Value) ?? 0m;

                var netSalary = Math.Round(grossSalary - configDeductionsTotal - deductionAmount + leaveEncashmentAmount, 0, MidpointRounding.AwayFromZero);

                var existing = await GetSalaryRecordAsync(employee.Id, monthId, yearId);
                if (existing != null)
                    continue;

                var record = new EmployeeMonthlySalary
                {
                    EmployeeId = employee.Id,
                    MonthId = monthId,
                    YearId = yearId,
                    GrossSalary = grossSalary,
                    WorkingDaysInMonth = workingDays,
                    DailySalary = dailySalary,
                    LeaveDeductionDays = deductionDays,
                    LeaveDeductionAmount = deductionAmount,
                    OtherDeductions = 0,
                    OtherAdditions = 0,
                    LeaveEncashmentDays = leaveEncashmentDays,
                    LeaveEncashmentAmount = leaveEncashmentAmount,
                    NetSalary = netSalary,
                    StatusId = (int)SalaryStatusEnum.Draft,
                    IsManuallyModified = false,
                    ProcessedOnUtc = DateTime.UtcNow,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _salaryRepository.InsertAsync(record);

                var employeeName = $"{employee.FirstName} {employee.LastName}";
                await _logger.InformationAsync($"[SalaryProcessing] Created Draft salary for {employeeName} — Month: {monthId}/{yearId} — Gross: {grossSalary:F2}, Deduction: {deductionAmount:F2}, Net: {netSalary:F2}");
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync($"[SalaryProcessing] Error processing employee {employee.Id}: {ex.Message}", ex);
            }
        }
    }

    public virtual async Task FinalizeSalaryRecordAsync(EmployeeMonthlySalary salaryRecord, int submittedByEmployeeId)
    {
        ArgumentNullException.ThrowIfNull(salaryRecord);

        if (salaryRecord.StatusId != (int)SalaryStatusEnum.Draft)
            throw new InvalidOperationException("Only Draft salary records can be finalized.");

        salaryRecord.StatusId = (int)SalaryStatusEnum.Finalized;
        salaryRecord.UpdatedOnUtc = DateTime.UtcNow;
        await _salaryRepository.UpdateAsync(salaryRecord);
    }

    public virtual async Task MarkSalaryAsPaidAsync(EmployeeMonthlySalary salaryRecord, int submittedByEmployeeId)
    {
        ArgumentNullException.ThrowIfNull(salaryRecord);

        if (salaryRecord.StatusId != (int)SalaryStatusEnum.Finalized)
            throw new InvalidOperationException("Only Finalized salary records can be marked as Paid.");

        salaryRecord.StatusId = (int)SalaryStatusEnum.Paid;
        salaryRecord.UpdatedOnUtc = DateTime.UtcNow;
        await _salaryRepository.UpdateAsync(salaryRecord);

        var employee = await _employeeService.GetEmployeeByIdAsync(salaryRecord.EmployeeId);
        var employeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : $"Employee #{salaryRecord.EmployeeId}";
        var monthName = GetMonthName(salaryRecord.MonthId);
        var lastDay = CalculateLastDayOfMonth(salaryRecord.MonthId, salaryRecord.YearId);

        var categoryId = _expenseManagementSettings.SalaryExpenseCategoryId > 0
            ? _expenseManagementSettings.SalaryExpenseCategoryId
            : 1;

        var expense = new Expense
        {
            ExpenseCategoryId = categoryId,
            Title = $"Salary - {employeeName} - {monthName} {salaryRecord.YearId}",
            Description = $"Salary payment. Month: {monthName} {salaryRecord.YearId}. Gross: {salaryRecord.GrossSalary:F2}, Leave Deduction: {salaryRecord.LeaveDeductionAmount:F2}, Other Deductions: {salaryRecord.OtherDeductions:F2}, Additions: {salaryRecord.OtherAdditions:F2}",
            Amount = salaryRecord.NetSalary,
            CurrencyCode = "INR",
            ExpenseDate = lastDay,
            SubmittedByEmployeeId = submittedByEmployeeId,
            EmployeeSalaryRecordId = salaryRecord.Id,
            PaymentMethodId = (int)PaymentTypeEnum.Bank,
            RecurringExpenseId = 0,
            ReceiptDownloadId = 0,
            Deleted = false,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        await _expenseRepository.InsertAsync(expense);

        var accountTransaction = new AccountTransaction
        {
            EmployeeId = salaryRecord.EmployeeId,
            TransactionTypeId = (int)TransactionTypeEnum.Expense,
            AccountGroupId = _expenseManagementSettings.SalaryAccountGroupId,
            PaymentMethodId = (int)PaymentTypeEnum.Bank,
            Amount = salaryRecord.NetSalary,
            Currency = "INR",
            MonthId = salaryRecord.MonthId,
            YearId = salaryRecord.YearId,
            ReferenceNo = $"SAL-{salaryRecord.Id}",
            Notes = $"Salary - {employeeName} - {monthName} {salaryRecord.YearId}",
            CreatedBy = submittedByEmployeeId,
            Deleted = false,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        await _accountTransactionRepository.InsertAsync(accountTransaction);

        salaryRecord.AccountTransactionId = accountTransaction.Id;
        await _salaryRepository.UpdateAsync(salaryRecord);
    }

    public virtual async Task InsertSalaryAuditLogAsync(EmployeeSalaryAuditLog auditLog)
    {
        ArgumentNullException.ThrowIfNull(auditLog);
        auditLog.ChangedOnUtc = auditLog.ChangedOnUtc == default ? DateTime.UtcNow : auditLog.ChangedOnUtc;
        await _auditLogRepository.InsertAsync(auditLog);
    }

    public virtual async Task<IList<EmployeeSalaryAuditLog>> GetSalaryAuditLogsAsync(int salaryRecordId)
    {
        return await _auditLogRepository.GetAllAsync(query =>
            query.Where(l => l.EmployeeSalaryRecordId == salaryRecordId)
                 .OrderByDescending(l => l.ChangedOnUtc));
    }

    #endregion
}
