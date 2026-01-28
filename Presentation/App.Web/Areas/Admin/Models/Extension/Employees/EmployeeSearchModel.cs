using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a employee search model
    /// </summary>
    public partial record EmployeeSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Employee.Fields.SearchEmployeeName")]
        public string SearchEmployeeName { get; set; }

        #endregion
    }
}