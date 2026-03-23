using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Expenses;

public partial record ExpenseSearchModel : BaseSearchModel
{
    #region Ctor

    public ExpenseSearchModel()
    {
        AvailableCategories = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Search.Title")]
    public string SearchTitle { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Search.Category")]
    public int SearchCategoryId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Search.FromDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchFromDate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Expense.Search.ToDate")]
    [UIHint("DateNullable")]
    public DateTime? SearchToDate { get; set; }

    public IList<SelectListItem> AvailableCategories { get; set; }

    #endregion
}
