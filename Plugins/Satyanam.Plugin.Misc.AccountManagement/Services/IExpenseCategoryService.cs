using App.Core;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial interface IExpenseCategoryService
{
    Task InsertExpenseCategoryAsync(ExpenseCategory expenseCategory);

    Task UpdateExpenseCategoryAsync(ExpenseCategory expenseCategory);

    Task DeleteExpenseCategoryAsync(ExpenseCategory expenseCategory);

    Task<ExpenseCategory> GetExpenseCategoryByIdAsync(int id);

    Task<IPagedList<ExpenseCategory>> GetAllExpenseCategoriesAsync(string name = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<IList<ExpenseCategory>> GetActiveExpenseCategoriesAsync();
}
