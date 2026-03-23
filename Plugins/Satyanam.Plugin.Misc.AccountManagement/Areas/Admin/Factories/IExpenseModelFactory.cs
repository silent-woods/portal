using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExpenseCategories;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Expenses;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.RecurringExpenses;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalaryComponentConfigs;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Factories;

public partial interface IExpenseModelFactory
{
    Task<ExpenseCategorySearchModel> PrepareExpenseCategorySearchModelAsync(ExpenseCategorySearchModel searchModel);
    Task<ExpenseCategoryListModel> PrepareExpenseCategoryListModelAsync(ExpenseCategorySearchModel searchModel);
    Task<ExpenseCategoryModel> PrepareExpenseCategoryModelAsync(ExpenseCategoryModel model, ExpenseCategory expenseCategory);
    Task<ExpenseSearchModel> PrepareExpenseSearchModelAsync(ExpenseSearchModel searchModel);
    Task<ExpenseListModel> PrepareExpenseListModelAsync(ExpenseSearchModel searchModel);
    Task<ExpenseModel> PrepareExpenseModelAsync(ExpenseModel model, Expense expense);
    Task<RecurringExpenseSearchModel> PrepareRecurringExpenseSearchModelAsync(RecurringExpenseSearchModel searchModel);
    Task<RecurringExpenseListModel> PrepareRecurringExpenseListModelAsync(RecurringExpenseSearchModel searchModel);
    Task<RecurringExpenseModel> PrepareRecurringExpenseModelAsync(RecurringExpenseModel model, RecurringExpense recurringExpense);
    Task<EmployeeSalarySearchModel> PrepareEmployeeSalarySearchModelAsync(EmployeeSalarySearchModel searchModel);
    Task<EmployeeSalaryListModel> PrepareEmployeeSalaryListModelAsync(EmployeeSalarySearchModel searchModel);
    Task<EmployeeSalaryModel> PrepareEmployeeSalaryModelAsync(EmployeeSalaryModel model, EmployeeMonthlySalary salaryRecord);
    Task<SalaryComponentConfigSearchModel> PrepareSalaryComponentConfigSearchModelAsync(SalaryComponentConfigSearchModel searchModel);
    Task<SalaryComponentConfigListModel> PrepareSalaryComponentConfigListModelAsync(SalaryComponentConfigSearchModel searchModel);
    Task<SalaryComponentConfigModel> PrepareSalaryComponentConfigModelAsync(SalaryComponentConfigModel model, SalaryComponentConfig component);
}
