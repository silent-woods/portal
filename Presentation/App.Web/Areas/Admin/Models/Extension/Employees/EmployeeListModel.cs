using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a employee list model
    /// </summary>
    public partial record EmployeeListModel : BasePagedListModel<EmployeeModel>
    {
      
    }
}