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
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.SearchTransactionTypeId")]
	public int SearchTransactionTypeId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.SearchPaymentMethodId")]
    public int SearchPaymentMethodId { get; set; }

    public IList<SelectListItem> AvailableTransactionTypes { get; set; }

    public IList<SelectListItem> AvailablePaymentMethods { get; set; }

    #endregion
}
