using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;

public partial record AccountTransactionSearchModel : BaseSearchModel
{
    #region Ctor

    public AccountTransactionSearchModel()
    {
        AvailableTransactionTypes = new List<SelectListItem>();
        AvailablePaymentMethods = new List<SelectListItem>();
        AvailablePeriods = new List<SelectListItem>();
        AvailableAccountGroups = new List<SelectListItem>();
        AvailableMonths = new List<SelectListItem>();
        AvailableYears = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.Period")]
    public int SearchPeriodId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.DateFrom")]
    public string SearchDateFrom { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.DateTo")]
    public string SearchDateTo { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.SearchTransactionTypeId")]
    public int SearchTransactionTypeId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.SearchPaymentMethodId")]
    public int SearchPaymentMethodId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.AccountGroup")]
    public int SearchAccountGroupId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.MonthId")]
    public int SearchMonthId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.YearId")]
    public int SearchYearId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Search.Entity")]
    public string SearchEntityValues { get; set; }

    public IList<SelectListItem> AvailablePeriods { get; set; }
    public IList<SelectListItem> AvailableTransactionTypes { get; set; }
    public IList<SelectListItem> AvailablePaymentMethods { get; set; }
    public IList<SelectListItem> AvailableAccountGroups { get; set; }
    public IList<SelectListItem> AvailableMonths { get; set; }
    public IList<SelectListItem> AvailableYears { get; set; }

    #endregion
}
