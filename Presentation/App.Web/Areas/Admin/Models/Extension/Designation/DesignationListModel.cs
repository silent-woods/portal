using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Designation
{
    /// <summary>
    /// Represents a customer list model
    /// </summary>
    public partial record DesignationListModel : BasePagedListModel<DesignationModel>
    {
    }
}