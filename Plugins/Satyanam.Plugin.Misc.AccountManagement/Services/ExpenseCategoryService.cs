using App.Core;
using App.Data;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class ExpenseCategoryService : IExpenseCategoryService
{
    #region Fields

    protected readonly IRepository<ExpenseCategory> _expenseCategoryRepository;

    #endregion

    #region Ctor

    public ExpenseCategoryService(IRepository<ExpenseCategory> expenseCategoryRepository)
    {
        _expenseCategoryRepository = expenseCategoryRepository;
    }

    #endregion

    #region Methods

    public virtual async Task InsertExpenseCategoryAsync(ExpenseCategory expenseCategory)
    {
        ArgumentNullException.ThrowIfNull(expenseCategory);
        expenseCategory.CreatedOnUtc = DateTime.UtcNow;
        expenseCategory.UpdatedOnUtc = DateTime.UtcNow;
        await _expenseCategoryRepository.InsertAsync(expenseCategory);
    }

    public virtual async Task UpdateExpenseCategoryAsync(ExpenseCategory expenseCategory)
    {
        ArgumentNullException.ThrowIfNull(expenseCategory);
        expenseCategory.UpdatedOnUtc = DateTime.UtcNow;
        await _expenseCategoryRepository.UpdateAsync(expenseCategory);
    }

    public virtual async Task DeleteExpenseCategoryAsync(ExpenseCategory expenseCategory)
    {
        ArgumentNullException.ThrowIfNull(expenseCategory);
        expenseCategory.Deleted = true;
        expenseCategory.UpdatedOnUtc = DateTime.UtcNow;
        await _expenseCategoryRepository.UpdateAsync(expenseCategory);
    }

    public virtual async Task<ExpenseCategory> GetExpenseCategoryByIdAsync(int id)
    {
        return await _expenseCategoryRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<ExpenseCategory>> GetAllExpenseCategoriesAsync(string name = null, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        return await _expenseCategoryRepository.GetAllPagedAsync(query =>
        {
            query = query.Where(c => !c.Deleted);
            if (!showHidden)
                query = query.Where(c => c.IsActive);
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.Contains(name.Trim()));
            query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);
            return query;
        }, pageIndex, pageSize);
    }

    public virtual async Task<IList<ExpenseCategory>> GetActiveExpenseCategoriesAsync()
    {
        return await _expenseCategoryRepository.GetAllAsync(query =>
            query.Where(c => !c.Deleted && c.IsActive).OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name));
    }

    #endregion
}
