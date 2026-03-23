using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExpenseCategories;

public partial record ExpenseCategorySearchModel : BaseSearchModel
{
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Search.Name")]
    public string SearchName { get; set; }
}
