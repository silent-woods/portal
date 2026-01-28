using App.Web.Framework.Models;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;

public partial record InvoicePaymentHistorySearchModel : BaseSearchModel
{
    #region Properties

    public int InvoiceId { get; set; }

    #endregion
}
