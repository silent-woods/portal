using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a Experience list model
    /// </summary>
    public partial record ExperienceListModel : BasePagedListModel<ExperienceModel>
    {
      
    }
}