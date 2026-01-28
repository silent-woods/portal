using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a education list model
    /// </summary>
    public partial record EducationListModel : BasePagedListModel<EducationModel>
    {
      
    }
}