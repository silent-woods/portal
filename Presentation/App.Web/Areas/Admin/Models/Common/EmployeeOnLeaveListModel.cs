using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents a popular search term list model
    /// </summary>
    public partial record EmployeeOnLeaveListModel : BasePagedListModel<EmployeeOnLeaveModel>
    {
    }
}