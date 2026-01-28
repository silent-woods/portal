using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Templates
{
    /// <summary>
    /// Represents a topic template list model
    /// </summary>
    public partial record TopicTemplateListModel : BasePagedListModel<TopicTemplateModel>
    {
    }
}