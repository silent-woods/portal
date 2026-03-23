using App.Data.Extensions;
using App.Services;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Media;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExpenseCategories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Expenses;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.RecurringExpenses;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalaryComponentConfigs;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;

public partial class ExpenseModelFactory : IExpenseModelFactory
{
    #region Fields

    protected readonly IExpenseCategoryService _expenseCategoryService;
    protected readonly IExpenseService _expenseService;
    protected readonly IEmployeeSalaryService _employeeSalaryService;
    protected readonly IEmployeeService _employeeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IDownloadService _downloadService;
    protected readonly ICurrencyService _currencyService;
    protected readonly ISalaryComponentConfigService _salaryComponentConfigService;
    protected readonly IEmployeeSalaryCustomComponentService _salaryCustomComponentService;
    protected readonly IAccountManagementService _accountManagementService;
    protected readonly ExpenseManagementSettings _expenseManagementSettings;

    #endregion

    #region Ctor

    public ExpenseModelFactory(IExpenseCategoryService expenseCategoryService,
        IExpenseService expenseService,
        IEmployeeSalaryService employeeSalaryService,
        IEmployeeService employeeService,
        ILocalizationService localizationService,
        IDownloadService downloadService,
        ICurrencyService currencyService,
        ISalaryComponentConfigService salaryComponentConfigService,
        IEmployeeSalaryCustomComponentService salaryCustomComponentService,
        IAccountManagementService accountManagementService,
        ExpenseManagementSettings expenseManagementSettings)
    {
        _expenseCategoryService = expenseCategoryService;
        _expenseService = expenseService;
        _employeeSalaryService = employeeSalaryService;
        _employeeService = employeeService;
        _salaryComponentConfigService = salaryComponentConfigService;
        _salaryCustomComponentService = salaryCustomComponentService;
        _localizationService = localizationService;
        _downloadService = downloadService;
        _currencyService = currencyService;
        _accountManagementService = accountManagementService;
        _expenseManagementSettings = expenseManagementSettings;
    }

    #endregion

    #region Utilities

    protected virtual IList<SelectListItem> PrepareAvailableCategoryTypes()
    {
        return Enum.GetValues<ExpenseCategoryTypeEnum>()
            .Select(e => new SelectListItem { Text = e.ToString(), Value = ((int)e).ToString() })
            .ToList();
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableCategoriesAsync(bool includeAll = true)
    {
        var categories = await _expenseCategoryService.GetActiveExpenseCategoriesAsync();
        var items = categories.Select(c => new SelectListItem
        {
            Text = c.Name,
            Value = c.Id.ToString()
        }).ToList();

        if (includeAll)
            items.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

        return items;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableFrequenciesAsync()
    {
        return new List<SelectListItem>
        {
            new SelectListItem { Text = "Weekly", Value = "1" },
            new SelectListItem { Text = "Monthly", Value = "2" },
            new SelectListItem { Text = "Quarterly", Value = "3" },
            new SelectListItem { Text = "Yearly", Value = "4" }
        };
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableCurrenciesAsync(string selectedCode = "INR")
    {
        var currencies = await _currencyService.GetAllCurrenciesAsync(showHidden: false);
        var items = currencies.Select(c => new SelectListItem
        {
            Text = $"{c.Name} ({c.CurrencyCode})",
            Value = c.CurrencyCode,
            Selected = c.CurrencyCode.Equals(selectedCode, StringComparison.OrdinalIgnoreCase)
        }).ToList();

        if (!items.Any())
            items.Add(new SelectListItem { Text = "Indian Rupee (INR)", Value = "INR", Selected = true });

        return items;
    }

    private async Task<int> GetTransactionAccountGroupIdAsync(int accountTransactionId)
    {
        if (accountTransactionId <= 0) return 0;
        var tx = await _accountManagementService.GetAccountTransactionByIdAsync(accountTransactionId);
        return tx?.AccountGroupId ?? 0;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableExpenseAccountGroupsAsync(int selectedId = 0)
    {
        var selectText = await _localizationService.GetResourceAsync("Admin.Common.Select");
        var groups = await _accountManagementService.GetAllAccountGroupsAsync(accountCategoryId: (int)AccountCategoryEnum.Expense, showHidden: false);
        var items = groups.Select(g => new SelectListItem
        {
            Text = g.Name,
            Value = g.Id.ToString(),
            Selected = g.Id == selectedId
        }).ToList();
        items.Insert(0, new SelectListItem { Text = selectText, Value = "0" });
        return items;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailablePaymentMethodsAsync(int selectedId = 0)
    {
        var selectText = await _localizationService.GetResourceAsync("Admin.Common.Select");
        var items = (await PaymentTypeEnum.Bank.ToSelectListAsync())
            .Select(x => new SelectListItem { Text = x.Text, Value = x.Value, Selected = x.Value == selectedId.ToString() })
            .ToList();
        items.Insert(0, new SelectListItem { Text = selectText, Value = "0" });
        return items;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableMonthsAsync()
    {
        var allText = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.All");
        var months = new List<SelectListItem>();
        months.Add(new SelectListItem { Text = allText, Value = "0" });
        for (var i = 1; i <= 12; i++)
            months.Add(new SelectListItem { Text = new DateTime(2000, i, 1).ToString("MMMM"), Value = i.ToString() });
        return months;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableYearsAsync()
    {
        var allText = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.All");
        var years = new List<SelectListItem>();
        years.Add(new SelectListItem { Text = allText, Value = "0" });
        for (var i = -1; i <= 2; i++)
            years.Add(new SelectListItem { Text = (DateTime.Now.Year + i).ToString(), Value = (DateTime.Now.Year + i).ToString() });
        return years;
    }

    #endregion

    #region Expense Category Methods

    public virtual async Task<ExpenseCategorySearchModel> PrepareExpenseCategorySearchModelAsync(ExpenseCategorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public virtual async Task<ExpenseCategoryListModel> PrepareExpenseCategoryListModelAsync(ExpenseCategorySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var categories = await _expenseCategoryService.GetAllExpenseCategoriesAsync(
            name: searchModel.SearchName,
            showHidden: true,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        return await new ExpenseCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
        {
            return categories.SelectAwait(async category =>
            {
                var m = new ExpenseCategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    CategoryTypeId = category.CategoryTypeId,
                    CategoryTypeName = category.CategoryType.ToString(),
                    IsActive = category.IsActive,
                    DisplayOrder = category.DisplayOrder
                };
                return m;
            });
        });
    }

    public virtual async Task<ExpenseCategoryModel> PrepareExpenseCategoryModelAsync(ExpenseCategoryModel model, ExpenseCategory expenseCategory)
    {
        if (expenseCategory != null)
        {
            model ??= new ExpenseCategoryModel();
            model.Id = expenseCategory.Id;
            model.Name = expenseCategory.Name;
            model.Description = expenseCategory.Description;
            model.CategoryTypeId = expenseCategory.CategoryTypeId;
            model.CategoryTypeName = expenseCategory.CategoryType.ToString();
            model.IsActive = expenseCategory.IsActive;
            model.DisplayOrder = expenseCategory.DisplayOrder;
        }
        else
        {
            model ??= new ExpenseCategoryModel();
            model.IsActive = true;
            model.DisplayOrder = 0;
        }

        model.AvailableCategoryTypes = PrepareAvailableCategoryTypes();

        return model;
    }

    #endregion

    #region Expense Methods

    public virtual async Task<ExpenseSearchModel> PrepareExpenseSearchModelAsync(ExpenseSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        searchModel.AvailableCategories = await PrepareAvailableCategoriesAsync(includeAll: true);
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public virtual async Task<ExpenseListModel> PrepareExpenseListModelAsync(ExpenseSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var expenses = await _expenseService.GetAllExpensesAsync(
            categoryId: searchModel.SearchCategoryId,
            title: searchModel.SearchTitle,
            fromDate: searchModel.SearchFromDate,
            toDate: searchModel.SearchToDate,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var categories = await _expenseCategoryService.GetAllExpenseCategoriesAsync(showHidden: true);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return await new ExpenseListModel().PrepareToGridAsync(searchModel, expenses, () =>
        {
            return expenses.SelectAwait(async expense =>
            {
                var m = new ExpenseModel
                {
                    Id = expense.Id,
                    ExpenseCategoryId = expense.ExpenseCategoryId,
                    ExpenseCategoryName = categoryDict.TryGetValue(expense.ExpenseCategoryId, out var cn) ? cn : string.Empty,
                    Title = expense.Title,
                    Amount = expense.Amount,
                    CurrencyCode = expense.CurrencyCode,
                    ExpenseDate = expense.ExpenseDate,
                    ExpenseDateStr = expense.ExpenseDate != default ? expense.ExpenseDate.ToString("dd-MMM-yyyy") : string.Empty,
                    EmployeeSalaryRecordId = expense.EmployeeSalaryRecordId,
                    RecurringExpenseId = expense.RecurringExpenseId
                };

                if (expense.SubmittedByEmployeeId > 0)
                {
                    var emp = await _employeeService.GetEmployeeByIdAsync(expense.SubmittedByEmployeeId);
                    m.SubmittedByEmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : string.Empty;
                }

                return m;
            });
        });
    }

    public virtual async Task<ExpenseModel> PrepareExpenseModelAsync(ExpenseModel model, Expense expense)
    {
        if (expense != null)
        {
            model ??= new ExpenseModel();
            model.Id = expense.Id;
            model.ExpenseCategoryId = expense.ExpenseCategoryId;
            model.Title = expense.Title;
            model.Description = expense.Description;
            model.Amount = expense.Amount;
            model.CurrencyCode = expense.CurrencyCode ?? "INR";
            model.ExpenseDate = expense.ExpenseDate;
            model.SubmittedByEmployeeId = expense.SubmittedByEmployeeId;
            model.ReceiptDownloadId = expense.ReceiptDownloadId;
            if (expense.ReceiptDownloadId > 0)
            {
                var download = await _downloadService.GetDownloadByIdAsync(expense.ReceiptDownloadId);
                if (download != null)
                {
                    model.ReceiptFileName = download.Filename + download.Extension;
                    model.ReceiptDownloadGuid = download.DownloadGuid;
                }
            }
            model.EmployeeSalaryRecordId = expense.EmployeeSalaryRecordId;
            model.RecurringExpenseId = expense.RecurringExpenseId;
            model.AccountGroupId = expense.AccountTransactionId > 0 ? await GetTransactionAccountGroupIdAsync(expense.AccountTransactionId) : 0;
            model.PaymentMethodId = expense.PaymentMethodId;

            if (expense.EmployeeSalaryRecordId > 0)
            {
                if (model.AccountGroupId == 0)
                    model.AccountGroupId = _expenseManagementSettings.SalaryAccountGroupId;
                if (model.PaymentMethodId == 0)
                    model.PaymentMethodId = (int)PaymentTypeEnum.Bank;
            }

            if (expense.SubmittedByEmployeeId > 0)
            {
                var emp = await _employeeService.GetEmployeeByIdAsync(expense.SubmittedByEmployeeId);
                model.SubmittedByEmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : string.Empty;
            }

            var categories = await _expenseCategoryService.GetAllExpenseCategoriesAsync(showHidden: true);
            model.ExpenseCategoryName = categories.FirstOrDefault(c => c.Id == expense.ExpenseCategoryId)?.Name ?? string.Empty;
        }
        else
        {
            model ??= new ExpenseModel();
            model.CurrencyCode = "INR";
            model.ExpenseDate = DateTime.Now;
        }

        model.AvailableCategories = await PrepareAvailableCategoriesAsync(includeAll: false);
        var allCatItems = model.AvailableCategories.ToList();
        allCatItems.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = "0" });
        model.AvailableCategories = allCatItems;

        model.AvailableCurrencies = await PrepareAvailableCurrenciesAsync(model.CurrencyCode ?? "INR");
        model.AvailableAccountGroups = await PrepareAvailableExpenseAccountGroupsAsync(model.AccountGroupId);
        model.AvailablePaymentMethods = await PrepareAvailablePaymentMethodsAsync(model.PaymentMethodId);

        return model;
    }

    #endregion

    #region Recurring Expense Methods

    public virtual async Task<RecurringExpenseSearchModel> PrepareRecurringExpenseSearchModelAsync(RecurringExpenseSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        searchModel.AvailableCategories = await PrepareAvailableCategoriesAsync(includeAll: true);
        var allText = await _localizationService.GetResourceAsync("Admin.Common.All");
        searchModel.AvailableFrequencies = (await RecurringFrequencyEnum.Weekly.ToSelectListAsync())
            .Select(x => new SelectListItem { Text = x.Text, Value = x.Value })
            .Prepend(new SelectListItem { Text = allText, Value = "0" })
            .ToList();
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public virtual async Task<RecurringExpenseListModel> PrepareRecurringExpenseListModelAsync(RecurringExpenseSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var records = await _expenseService.GetAllRecurringExpensesAsync(
            categoryId: searchModel.SearchCategoryId,
            title: searchModel.SearchTitle,
            frequencyId: searchModel.SearchFrequencyId,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var categories = await _expenseCategoryService.GetAllExpenseCategoriesAsync(showHidden: true);
        var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

        return await new RecurringExpenseListModel().PrepareToGridAsync(searchModel, records, () =>
        {
            return records.SelectAwait(async r =>
            {
                var m = new RecurringExpenseModel
                {
                    Id = r.Id,
                    ExpenseCategoryId = r.ExpenseCategoryId,
                    ExpenseCategoryName = categoryDict.TryGetValue(r.ExpenseCategoryId, out var cn) ? cn : string.Empty,
                    Title = r.Title,
                    Amount = r.Amount,
                    CurrencyCode = r.CurrencyCode,
                    FrequencyId = r.FrequencyId,
                    FrequencyName = r.Frequency.ToString(),
                    RecurrenceDay = r.RecurrenceDay,
                    RecurrenceMonth = r.RecurrenceMonth,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    LastGeneratedOnUtc = r.LastGeneratedOnUtc,
                    NextGenerateOnUtc = r.NextGenerateOnUtc,
                    NextGenerateStr = r.NextGenerateOnUtc.HasValue ? r.NextGenerateOnUtc.Value.ToString("dd-MMM-yyyy") : "-",
                    IsActive = r.IsActive
                };
                return m;
            });
        });
    }

    public virtual async Task<RecurringExpenseModel> PrepareRecurringExpenseModelAsync(RecurringExpenseModel model, RecurringExpense recurringExpense)
    {
        if (recurringExpense != null)
        {
            model ??= new RecurringExpenseModel();
            model.Id = recurringExpense.Id;
            model.ExpenseCategoryId = recurringExpense.ExpenseCategoryId;
            model.Title = recurringExpense.Title;
            model.Description = recurringExpense.Description;
            model.Amount = recurringExpense.Amount;
            model.CurrencyCode = recurringExpense.CurrencyCode ?? "INR";
            model.FrequencyId = recurringExpense.FrequencyId;
            model.FrequencyName = recurringExpense.Frequency.ToString();
            model.RecurrenceDay = recurringExpense.RecurrenceDay;
            model.RecurrenceMonth = recurringExpense.RecurrenceMonth;
            model.StartDate = recurringExpense.StartDate;
            model.EndDate = recurringExpense.EndDate;
            model.LastGeneratedOnUtc = recurringExpense.LastGeneratedOnUtc;
            model.NextGenerateOnUtc = recurringExpense.NextGenerateOnUtc;
            model.IsActive = recurringExpense.IsActive;
            model.AccountGroupId = recurringExpense.AccountGroupId;
            model.PaymentMethodId = recurringExpense.PaymentMethodId;
        }
        else
        {
            var isNew = model == null;
            model ??= new RecurringExpenseModel();
            if (isNew)
            {
                model.CurrencyCode = "INR";
                model.IsActive = true;
                model.StartDate = DateTime.Now;
                model.FrequencyId = (int)RecurringFrequencyEnum.Monthly;
                model.RecurrenceDay = 1;
            }
        }

        model.AvailableCategories = await PrepareAvailableCategoriesAsync(includeAll: false);
        var catItems = model.AvailableCategories.ToList();
        catItems.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = "0" });
        model.AvailableCategories = catItems;

        model.AvailableFrequencies = await PrepareAvailableFrequenciesAsync();
        model.AvailableCurrencies = await PrepareAvailableCurrenciesAsync(model.CurrencyCode ?? "INR");
        model.AvailableAccountGroups = await PrepareAvailableExpenseAccountGroupsAsync(model.AccountGroupId);
        model.AvailablePaymentMethods = await PrepareAvailablePaymentMethodsAsync(model.PaymentMethodId);

        return model;
    }

    #endregion

    #region Employee Salary Methods

    public virtual async Task<EmployeeSalarySearchModel> PrepareEmployeeSalarySearchModelAsync(EmployeeSalarySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        searchModel.AvailableMonths = await PrepareAvailableMonthsAsync();
        searchModel.AvailableYears = await PrepareAvailableYearsAsync();
        searchModel.AvailableStatuses = new List<SelectListItem>
        {
            new SelectListItem { Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.All"), Value = "0" },
            new SelectListItem { Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.Status.Draft"), Value = "1" },
            new SelectListItem { Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.Status.Finalized"), Value = "2" },
            new SelectListItem { Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.Status.Paid"), Value = "3" }
        };

        var employees = await _employeeService.GetAllEmployeesAsync(showInActive: false, showVendors: true);
        searchModel.AvailableEmployees = employees
            .Select(e => new SelectListItem
            {
                Text = $"{e.FirstName} {e.LastName}".Trim(),
                Value = e.Id.ToString()
            })
            .ToList();

        searchModel.SearchMonthId = 0;
        searchModel.SearchYearId = DateTime.Now.Year;

        searchModel.SetGridPageSize();
        return searchModel;
    }

    public virtual async Task<EmployeeSalaryListModel> PrepareEmployeeSalaryListModelAsync(EmployeeSalarySearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var records = await _employeeSalaryService.GetAllSalaryRecordsAsync(
            monthId: searchModel.SearchMonthId,
            yearId: searchModel.SearchYearId,
            statusId: searchModel.SearchStatusId,
            searchEmployeeIds: searchModel.SearchEmployeeIds,
            pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        return await new EmployeeSalaryListModel().PrepareToGridAsync(searchModel, records, () =>
        {
            return records.SelectAwait(async record =>
            {
                var emp = await _employeeService.GetEmployeeByIdAsync(record.EmployeeId);
                var m = new EmployeeSalaryModel
                {
                    Id = record.Id,
                    EmployeeId = record.EmployeeId,
                    EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : $"Employee #{record.EmployeeId}",
                    MonthId = record.MonthId,
                    MonthName = new DateTime(2000, record.MonthId, 1).ToString("MMMM"),
                    YearId = record.YearId,
                    GrossSalary = record.GrossSalary,
                    WorkingDaysInMonth = record.WorkingDaysInMonth,
                    DailySalary = record.DailySalary,
                    LeaveDeductionDays = record.LeaveDeductionDays,
                    LeaveDeductionAmount = record.LeaveDeductionAmount,
                    OtherDeductions = record.OtherDeductions,
                    OtherAdditions = record.OtherAdditions,
                    NetSalary = record.NetSalary,
                    Remarks = record.Remarks,
                    StatusId = record.StatusId,
                    StatusName = ((SalaryStatusEnum)record.StatusId).ToString(),
                    IsManuallyModified = record.IsManuallyModified
                };
                return m;
            });
        });
    }

    public virtual async Task<EmployeeSalaryModel> PrepareEmployeeSalaryModelAsync(EmployeeSalaryModel model, EmployeeMonthlySalary salaryRecord)
    {
        if (salaryRecord != null)
        {
            model ??= new EmployeeSalaryModel();
            model.Id = salaryRecord.Id;
            model.EmployeeId = salaryRecord.EmployeeId;
            model.MonthId = salaryRecord.MonthId;
            model.MonthName = new DateTime(2000, salaryRecord.MonthId, 1).ToString("MMMM");
            model.YearId = salaryRecord.YearId;
            model.GrossSalary = salaryRecord.GrossSalary;
            model.WorkingDaysInMonth = salaryRecord.WorkingDaysInMonth;
            model.DailySalary = salaryRecord.DailySalary;
            model.LeaveDeductionDays = salaryRecord.LeaveDeductionDays;
            model.LeaveDeductionAmount = salaryRecord.LeaveDeductionAmount;
            model.OtherDeductions = salaryRecord.OtherDeductions;
            model.OtherAdditions = salaryRecord.OtherAdditions;
            model.LeaveEncashmentDays = salaryRecord.LeaveEncashmentDays;
            model.LeaveEncashmentAmount = salaryRecord.LeaveEncashmentAmount;
            model.NetSalary = salaryRecord.NetSalary;
            model.Remarks = salaryRecord.Remarks;
            model.StatusId = salaryRecord.StatusId;
            model.StatusName = ((SalaryStatusEnum)salaryRecord.StatusId).ToString();
            model.IsManuallyModified = salaryRecord.IsManuallyModified;

            var emp = await _employeeService.GetEmployeeByIdAsync(salaryRecord.EmployeeId);
            model.EmployeeName = emp != null ? $"{emp.FirstName} {emp.LastName}" : $"Employee #{salaryRecord.EmployeeId}";

            var auditLogs = await _employeeSalaryService.GetSalaryAuditLogsAsync(salaryRecord.Id);
            var auditLogModels = new System.Collections.Generic.List<EmployeeSalaryAuditLogModel>();
            foreach (var l in auditLogs)
            {
                var changedByEmp = await _employeeService.GetEmployeeByIdAsync(l.ChangedByEmployeeId);
                auditLogModels.Add(new EmployeeSalaryAuditLogModel
                {
                    Id = l.Id,
                    FieldName = l.FieldName,
                    OldValue = l.OldValue,
                    NewValue = l.NewValue,
                    ChangedByName = changedByEmp != null
                        ? $"{changedByEmp.FirstName} {changedByEmp.LastName}"
                        : $"Employee #{l.ChangedByEmployeeId}",
                    ChangedOnUtc = l.ChangedOnUtc
                });
            }
            model.AuditLogs = auditLogModels;

            var components = await _salaryComponentConfigService.GetAllActiveComponentsAsync();
            var earningLines = new System.Collections.Generic.List<SalaryBreakdownLineModel>();
            var deductionLines = new System.Collections.Generic.List<SalaryBreakdownLineModel>();

            var totalConfigDeductions = 0m;
            var totalNonRemainderEarnings = 0m;
            foreach (var c in components)
            {
                if (c.IsRemainder) continue;
                var amount = c.IsPercentage
                    ? System.Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 2)
                    : c.Value;
                if (c.ComponentTypeId == (int)Domain.Enums.SalaryComponentTypeEnum.Earning)
                    totalNonRemainderEarnings += amount;
                else
                    totalConfigDeductions += amount;
            }

            foreach (var c in components)
            {
                decimal amount;
                if (c.IsRemainder)
                    amount = System.Math.Round(salaryRecord.GrossSalary - totalNonRemainderEarnings, 2);
                else
                    amount = c.IsPercentage
                        ? System.Math.Round(salaryRecord.GrossSalary * c.Value / 100m, 2)
                        : c.Value;

                var line = new SalaryBreakdownLineModel { Name = c.Name, Amount = amount };
                if (c.ComponentTypeId == (int)Domain.Enums.SalaryComponentTypeEnum.Earning)
                    earningLines.Add(line);
                else
                    deductionLines.Add(line);
            }

            if (salaryRecord.LeaveDeductionAmount > 0)
                deductionLines.Add(new SalaryBreakdownLineModel { Name = $"Loss of Pay ({salaryRecord.LeaveDeductionDays:0.##} days)", Amount = salaryRecord.LeaveDeductionAmount });

            var customComponents = await _salaryCustomComponentService.GetComponentsBySalaryRecordIdAsync(salaryRecord.Id);
            var customComponentModels = new System.Collections.Generic.List<EmployeeSalaryCustomComponentModel>();
            foreach (var cc in customComponents)
            {
                customComponentModels.Add(new EmployeeSalaryCustomComponentModel
                {
                    Id = cc.Id,
                    SalaryRecordId = cc.SalaryRecordId,
                    TypeId = cc.TypeId,
                    TypeName = cc.ComponentType == Domain.SalaryCustomComponentType.Addition
                        ? await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.CustomComponent.Addition")
                        : await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.CustomComponent.Deduction"),
                    Name = cc.Name,
                    Amount = cc.Amount
                });
            }

            model.CustomComponents = customComponentModels;
            model.EarningLines = earningLines;
            model.DeductionLines = deductionLines;
            model.ConfigDeductionsTotal = totalConfigDeductions;
        }
        else
        {
            model ??= new EmployeeSalaryModel();
        }

        return model;
    }

    public virtual async Task<SalaryComponentConfigSearchModel> PrepareSalaryComponentConfigSearchModelAsync(SalaryComponentConfigSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);
        searchModel.SetGridPageSize();
        return searchModel;
    }

    public virtual async Task<SalaryComponentConfigListModel> PrepareSalaryComponentConfigListModelAsync(SalaryComponentConfigSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var components = await _salaryComponentConfigService.GetAllActiveComponentsAsync(showHidden: true);
        var pagedComponents = components
            .Skip((searchModel.Page - 1) * searchModel.PageSize)
            .Take(searchModel.PageSize)
            .ToList();

        var pagedList = new App.Core.PagedList<SalaryComponentConfig>(pagedComponents, searchModel.Page - 1, searchModel.PageSize, components.Count);

        return await new SalaryComponentConfigListModel().PrepareToGridAsync(searchModel, pagedList, () =>
        {
            return pagedList.SelectAwait(async c => new SalaryComponentConfigModel
            {
                Id = c.Id,
                Name = c.Name,
                ComponentTypeId = c.ComponentTypeId,
                ComponentTypeName = c.ComponentType.ToString(),
                IsPercentage = c.IsPercentage,
                Value = c.Value,
                IsRemainder = c.IsRemainder,
                IsActive = c.IsActive,
                DisplayOrder = c.DisplayOrder
            });
        });
    }

    public virtual async Task<SalaryComponentConfigModel> PrepareSalaryComponentConfigModelAsync(SalaryComponentConfigModel model, SalaryComponentConfig component)
    {
        if (component != null)
        {
            model ??= new SalaryComponentConfigModel();
            model.Id = component.Id;
            model.Name = component.Name;
            model.ComponentTypeId = component.ComponentTypeId;
            model.ComponentTypeName = component.ComponentType.ToString();
            model.IsPercentage = component.IsPercentage;
            model.Value = component.Value;
            model.IsRemainder = component.IsRemainder;
            model.IsActive = component.IsActive;
            model.DisplayOrder = component.DisplayOrder;
        }
        else
        {
            model ??= new SalaryComponentConfigModel();
            model.IsPercentage = true;
            model.IsActive = true;
        }

        model.AvailableComponentTypes = new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.ComponentType.Earning") },
            new SelectListItem { Value = "2", Text = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Common.ComponentType.Deduction") }
        };

        return model;
    }

    #endregion
}
