using App.Core;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial interface IExpenseService
{
    #region Expense Methods

    Task InsertExpenseAsync(Expense expense);

    Task UpdateExpenseAsync(Expense expense);

    Task DeleteExpenseAsync(Expense expense);

    Task<Expense> GetExpenseByIdAsync(int id);

    Task<IPagedList<Expense>> GetAllExpensesAsync(int categoryId = 0, string title = null, int submittedByEmployeeId = 0, DateTime? fromDate = null, DateTime? toDate = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<Expense> GetExpenseBySalaryRecordIdAsync(int salaryRecordId);

    Task CreateLinkedTransactionAsync(Expense expense, int accountGroupId, int paymentMethodId, int createdByEmployeeId);

    Task RemoveLinkedTransactionAsync(Expense expense);

    Task UpdateLinkedTransactionAccountGroupAsync(Expense expense, int accountGroupId);

    #endregion

    #region Recurring Expense Methods

    Task InsertRecurringExpenseAsync(RecurringExpense recurringExpense);

    Task UpdateRecurringExpenseAsync(RecurringExpense recurringExpense);

    Task DeleteRecurringExpenseAsync(RecurringExpense recurringExpense);

    Task<RecurringExpense> GetRecurringExpenseByIdAsync(int id);

    Task<IPagedList<RecurringExpense>> GetAllRecurringExpensesAsync(int categoryId = 0, string title = null, int frequencyId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IList<RecurringExpense>> GetDueRecurringExpensesAsync(DateTime asOf);

    #endregion
}
