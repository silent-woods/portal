using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents an URL record list model
    /// </summary>
    public partial record UrlRecordListModel : BasePagedListModel<UrlRecordModel>
    {
    }
}