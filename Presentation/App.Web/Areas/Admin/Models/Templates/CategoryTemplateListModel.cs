using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Templates
{
    /// <summary>
    /// Represents a category template list model
    /// </summary>
    public partial record CategoryTemplateListModel : BasePagedListModel<CategoryTemplateModel>
    {
    }
}