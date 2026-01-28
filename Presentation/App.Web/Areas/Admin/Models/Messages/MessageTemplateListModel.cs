using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a message template list model
    /// </summary>
    public partial record MessageTemplateListModel : BasePagedListModel<MessageTemplateModel>
    {
    }
}