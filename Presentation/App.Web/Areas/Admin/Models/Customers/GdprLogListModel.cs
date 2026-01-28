using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a GDPR request list model
    /// </summary>
    public partial record GdprLogListModel : BasePagedListModel<GdprLogModel>
    {
    }
}