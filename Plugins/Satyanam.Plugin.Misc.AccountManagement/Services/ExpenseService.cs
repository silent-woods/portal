using App.Core;
using App.Data;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Enums;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class ExpenseService : IExpenseService
{ 

    protected readonly IRepository<Expense> _expenseRepository;
    protected readonly IRepository<RecurringExpense> _recurringExpenseRepository;
    protected readonly IRepository<AccountTransaction> _accountTransactionRepository;

    #region Ctor

    public ExpenseService(IRepository<Expense> expenseRepository,
        IRepository<RecurringExpense> recurringExpenseRepository,
        IRepository<AccountTransaction> accountTransactionRepository)
    {
        _expenseRepository = expenseRepository;
        _recurringExpenseRepository = recurringExpenseRepository;
        _accountTransactionRepository = accountTransactionRepository;
    }

    #endregion

    #region Expense Methods

    public virtual async Task InsertExpenseAsync(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);
        expense.CreatedOnUtc = DateTime.UtcNow;
        expense.UpdatedOnUtc = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(expense.CurrencyCode))
            expense.CurrencyCode = "INR";
        await _expenseRepository.InsertAsync(expense);
    }

    public virtual async Task UpdateExpenseAsync(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);
        expense.UpdatedOnUtc = DateTime.UtcNow;
        await _expenseRepository.UpdateAsync(expense);

        if (expense.AccountTransactionId > 0)
        {
            var transaction = await _accountTransactionRepository.GetByIdAsync(expense.AccountTransactionId);
            if (transaction != null && !transaction.Deleted)
            {
                transaction.Amount = expense.Amount;
                transaction.Notes = expense.Title;
                if (expense.PaymentMethodId > 0)
                    transaction.PaymentMethodId = expense.PaymentMethodId;
                transaction.UpdatedOnUtc = DateTime.UtcNow;
                await _accountTransactionRepository.UpdateAsync(transaction);
            }
        }
    }

    public virtual async Task DeleteExpenseAsync(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);
        expense.Deleted = true;
        expense.UpdatedOnUtc = DateTime.UtcNow;
        await _expenseRepository.UpdateAsync(expense);

        if (expense.AccountTransactionId > 0)
        {
            var transaction = await _accountTransactionRepository.GetByIdAsync(expense.AccountTransactionId);
            if (transaction != null && !transaction.Deleted)
            {
                transaction.Deleted = true;
                transaction.UpdatedOnUtc = DateTime.UtcNow;
                await _accountTransactionRepository.UpdateAsync(transaction);
            }
        }
    }

    public virtual async Task<Expense> GetExpenseByIdAsync(int id)
    {
        return await _expenseRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<Expense>> GetAllExpensesAsync(int categoryId = 0, string title = null, int submittedByEmployeeId = 0, DateTime? fromDate = null, DateTime? toDate = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _expenseRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(e => !e.Deleted);
            if (categoryId > 0)
                query = query.Where(e => e.ExpenseCategoryId == categoryId);
            if (!string.IsNullOrEmpty(title))
                query = query.Where(e => e.Title.Contains(title.Trim()));
            if (submittedByEmployeeId > 0)
                query = query.Where(e => e.SubmittedByEmployeeId == submittedByEmployeeId);
            if (fromDate.HasValue)
                query = query.Where(e => e.ExpenseDate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(e => e.ExpenseDate <= toDate.Value);
            query = query.OrderByDescending(e => e.ExpenseDate);
            return query;
        }, pageIndex, pageSize);
    }

    public virtual async Task CreateLinkedTransactionAsync(Expense expense, int accountGroupId, int paymentMethodId, int createdByEmployeeId)
    {
        ArgumentNullException.ThrowIfNull(expense);
        if (accountGroupId <= 0)
            return;

        var transaction = new AccountTransaction
        {
            TransactionTypeId = (int)TransactionTypeEnum.Expense,
            AccountGroupId = accountGroupId,
            PaymentMethodId = paymentMethodId > 0 ? paymentMethodId : (int)PaymentTypeEnum.Bank,
            Amount = expense.Amount,
            Currency = expense.CurrencyCode ?? "INR",
            MonthId = expense.ExpenseDate.Month,
            YearId = expense.ExpenseDate.Year,
            ReferenceNo = $"EXP-{expense.Id}",
            Notes = expense.Title,
            CreatedBy = createdByEmployeeId,
            Deleted = false,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };
        await _accountTransactionRepository.InsertAsync(transaction);

        expense.AccountTransactionId = transaction.Id;
        expense.PaymentMethodId = transaction.PaymentMethodId;
        await _expenseRepository.UpdateAsync(expense);
    }

    public virtual async Task RemoveLinkedTransactionAsync(Expense expense)
    {
        ArgumentNullException.ThrowIfNull(expense);
        if (expense.AccountTransactionId <= 0)
            return;

        var transaction = await _accountTransactionRepository.GetByIdAsync(expense.AccountTransactionId);
        if (transaction != null && !transaction.Deleted)
        {
            transaction.Deleted = true;
            transaction.UpdatedOnUtc = DateTime.UtcNow;
            await _accountTransactionRepository.UpdateAsync(transaction);
        }

        expense.AccountTransactionId = 0;
        expense.UpdatedOnUtc = DateTime.UtcNow;
        await _expenseRepository.UpdateAsync(expense);
    }

    public virtual async Task UpdateLinkedTransactionAccountGroupAsync(Expense expense, int accountGroupId)
    {
        ArgumentNullException.ThrowIfNull(expense);
        if (expense.AccountTransactionId <= 0 || accountGroupId <= 0)
            return;

        var transaction = await _accountTransactionRepository.GetByIdAsync(expense.AccountTransactionId);
        if (transaction != null && !transaction.Deleted)
        {
            transaction.AccountGroupId = accountGroupId;
            transaction.Amount = expense.Amount;
            transaction.Currency = expense.CurrencyCode ?? "INR";
            transaction.PaymentMethodId = expense.PaymentMethodId > 0 ? expense.PaymentMethodId : transaction.PaymentMethodId;
            transaction.MonthId = expense.ExpenseDate.Month;
            transaction.YearId = expense.ExpenseDate.Year;
            transaction.Notes = expense.Title;
            transaction.UpdatedOnUtc = DateTime.UtcNow;
            await _accountTransactionRepository.UpdateAsync(transaction);
        }
    }

    public virtual async Task<Expense> GetExpenseBySalaryRecordIdAsync(int salaryRecordId)
    {
        if (salaryRecordId <= 0)
            return null;

        return await _expenseRepository.GetAllAsync(query =>
            query.Where(e => !e.Deleted && e.EmployeeSalaryRecordId == salaryRecordId))
            .ContinueWith(t => t.Result.FirstOrDefault());
    }

    #endregion

    #region Recurring Expense Methods

    public virtual async Task InsertRecurringExpenseAsync(RecurringExpense recurringExpense)
    {
        ArgumentNullException.ThrowIfNull(recurringExpense);
        recurringExpense.CreatedOnUtc = DateTime.UtcNow;
        recurringExpense.UpdatedOnUtc = DateTime.UtcNow;
        if (string.IsNullOrWhiteSpace(recurringExpense.CurrencyCode))
            recurringExpense.CurrencyCode = "INR";
        await _recurringExpenseRepository.InsertAsync(recurringExpense);
    }

    public virtual async Task UpdateRecurringExpenseAsync(RecurringExpense recurringExpense)
    {
        ArgumentNullException.ThrowIfNull(recurringExpense);
        recurringExpense.UpdatedOnUtc = DateTime.UtcNow;
        await _recurringExpenseRepository.UpdateAsync(recurringExpense);
    }

    public virtual async Task DeleteRecurringExpenseAsync(RecurringExpense recurringExpense)
    {
        ArgumentNullException.ThrowIfNull(recurringExpense);
        recurringExpense.Deleted = true;
        recurringExpense.UpdatedOnUtc = DateTime.UtcNow;
        await _recurringExpenseRepository.UpdateAsync(recurringExpense);
    }

    public virtual async Task<RecurringExpense> GetRecurringExpenseByIdAsync(int id)
    {
        return await _recurringExpenseRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<RecurringExpense>> GetAllRecurringExpensesAsync(int categoryId = 0, string title = null, int frequencyId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _recurringExpenseRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(r => !r.Deleted);
            if (categoryId > 0)
                query = query.Where(r => r.ExpenseCategoryId == categoryId);
            if (!string.IsNullOrEmpty(title))
                query = query.Where(r => r.Title.Contains(title.Trim()));
            if (frequencyId > 0)
                query = query.Where(r => r.FrequencyId == frequencyId);
            query = query.OrderByDescending(r => r.CreatedOnUtc);
            return query;
        }, pageIndex, pageSize);
    }

    public virtual async Task<IList<RecurringExpense>> GetDueRecurringExpensesAsync(DateTime asOf)
    {
        return await _recurringExpenseRepository.GetAllAsync(query =>
            query.Where(r => !r.Deleted && r.IsActive
                && (r.EndDate == null || r.EndDate == DateTime.MinValue || r.EndDate >= asOf)
                && (r.NextGenerateOnUtc == null || r.NextGenerateOnUtc <= asOf)));
    }

    #endregion
}
