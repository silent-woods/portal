using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Templates
{
    /// <summary>
    /// Represents a manufacturer template list model
    /// </summary>
    public partial record ManufacturerTemplateListModel : BasePagedListModel<ManufacturerTemplateModel>
    {
    }
}