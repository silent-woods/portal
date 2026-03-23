using App.Core;
using App.Data;
using App.Services.Helpers;
using App.Services.Logging;
using App.Services.ScheduleTasks;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services.ScheduleTasks;

public partial class GenerateRecurringExpensesTask : IScheduleTask
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IExpenseService _expenseService;
    private readonly IRepository<AccountTransaction> _accountTransactionRepository;
    private readonly ExpenseManagementSettings _expenseManagementSettings;
    private readonly ILogger _logger;

    #endregion

    #region Ctor

    public GenerateRecurringExpensesTask(IDateTimeHelper dateTimeHelper,
        IExpenseService expenseService,
        IRepository<AccountTransaction> accountTransactionRepository,
        ExpenseManagementSettings expenseManagementSettings,
        ILogger logger)
    {
        _dateTimeHelper = dateTimeHelper;
        _expenseService = expenseService;
        _accountTransactionRepository = accountTransactionRepository;
        _expenseManagementSettings = expenseManagementSettings;
        _logger = logger;
    }

    #endregion

    #region Utilities

    protected virtual DateTime CalculateNextDate(DateTime lastDate, RecurringFrequencyEnum frequency, int recurrenceDay, int? recurrenceMonth)
    {
        switch (frequency)
        {
            case RecurringFrequencyEnum.Weekly:
            {
                var targetDow = recurrenceDay == 7 ? DayOfWeek.Sunday : (DayOfWeek)recurrenceDay;
                var next = lastDate.AddDays(1);
                while (next.DayOfWeek != targetDow)
                    next = next.AddDays(1);
                return next.Date;
            }

            case RecurringFrequencyEnum.Monthly:
            {
                var next = lastDate.AddMonths(1);
                var daysInMonth = DateTime.DaysInMonth(next.Year, next.Month);
                var day = Math.Min(recurrenceDay, daysInMonth);
                return new DateTime(next.Year, next.Month, day);
            }

            case RecurringFrequencyEnum.Quarterly:
            {
                var next = lastDate.AddMonths(3);
                var daysInMonth = DateTime.DaysInMonth(next.Year, next.Month);
                var day = Math.Min(recurrenceDay, daysInMonth);
                return new DateTime(next.Year, next.Month, day);
            }

            case RecurringFrequencyEnum.Yearly:
            {
                var next = lastDate.AddYears(1);
                var month = recurrenceMonth ?? next.Month;
                var daysInMonth = DateTime.DaysInMonth(next.Year, month);
                var day = Math.Min(recurrenceDay, daysInMonth);
                return new DateTime(next.Year, month, day);
            }

            default:
                return lastDate.AddMonths(1);
        }
    }

    #endregion

    #region Methods

    public virtual async Task ExecuteAsync()
    {
        var today = await _dateTimeHelper.GetIndianTimeAsync();

        try
        {
            var dueTemplates = await _expenseService.GetDueRecurringExpensesAsync(today);

            foreach (var template in dueTemplates)
            {
                try
                {
                    var expense = new Expense
                    {
                        ExpenseCategoryId = template.ExpenseCategoryId,
                        Title = template.Title,
                        Description = template.Description,
                        Amount = template.Amount,
                        CurrencyCode = template.CurrencyCode ?? "INR",
                        ExpenseDate = today.Date,
                        SubmittedByEmployeeId = template.CreatedByEmployeeId,
                        RecurringExpenseId = template.Id,
                        EmployeeSalaryRecordId = 0,
                        ReceiptDownloadId = 0,
                        Deleted = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _expenseService.InsertExpenseAsync(expense);

                    var txAccountGroupId = template.AccountGroupId > 0
                        ? template.AccountGroupId
                        : _expenseManagementSettings.RecurringExpenseAccountGroupId;
                    var txPaymentMethodId = template.PaymentMethodId > 0
                        ? template.PaymentMethodId
                        : (int)PaymentTypeEnum.Bank;

                    if (txAccountGroupId > 0)
                    {
                        var accountTransaction = new AccountTransaction
                        {
                            TransactionTypeId = (int)TransactionTypeEnum.Expense,
                            AccountGroupId = txAccountGroupId,
                            PaymentMethodId = txPaymentMethodId,
                            Amount = expense.Amount,
                            Currency = expense.CurrencyCode ?? "INR",
                            MonthId = today.Month,
                            YearId = today.Year,
                            ReferenceNo = $"EXP-{expense.Id}",
                            Notes = expense.Title,
                            CreatedBy = template.CreatedByEmployeeId,
                            Deleted = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };
                        await _accountTransactionRepository.InsertAsync(accountTransaction);

                        expense.AccountTransactionId = accountTransaction.Id;
                        expense.PaymentMethodId = txPaymentMethodId;
                        await _expenseService.UpdateExpenseAsync(expense);
                    }

                    template.LastGeneratedOnUtc = today;
                    template.NextGenerateOnUtc = CalculateNextDate(today, template.Frequency, template.RecurrenceDay, template.RecurrenceMonth);
                    await _expenseService.UpdateRecurringExpenseAsync(template);

                    await _logger.InformationAsync($"[GenerateRecurringExpenses] Generated expense '{template.Title}' from template #{template.Id}. Next: {template.NextGenerateOnUtc:d}");
                }
                catch (Exception ex)
                {
                    await _logger.ErrorAsync($"[GenerateRecurringExpenses] Error generating expense for template #{template.Id}: {ex.Message}", ex);
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("[GenerateRecurringExpensesTask] Unexpected error.", ex);
        }
    }

    #endregion
}
