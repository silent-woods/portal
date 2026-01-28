using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Departments

{
    /// <summary>
    /// Represents a department list model
    /// </summary>
    public partial record DepartmentListModel : BasePagedListModel<DepartmentModel>
    {
      
    }
}