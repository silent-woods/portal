using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents an email account list model
    /// </summary>
    public partial record EmailAccountListModel : BasePagedListModel<EmailAccountModel>
    {
    }
}