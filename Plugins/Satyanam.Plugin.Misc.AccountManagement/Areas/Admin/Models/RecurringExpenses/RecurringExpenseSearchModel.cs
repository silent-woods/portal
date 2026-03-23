using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.RecurringExpenses;

public partial record RecurringExpenseSearchModel : BaseSearchModel
{
    #region Ctor

    public RecurringExpenseSearchModel()
    {
        AvailableCategories = new List<SelectListItem>();
        AvailableFrequencies = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Search.Title")]
    public string SearchTitle { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Search.Category")]
    public int SearchCategoryId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.RecurringExpense.Search.Frequency")]
    public int SearchFrequencyId { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    public IList<SelectListItem> AvailableFrequencies { get; set; }

    #endregion
}
