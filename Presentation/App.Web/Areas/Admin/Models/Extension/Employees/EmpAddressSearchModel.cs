using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Employees
{
    /// <summary>
    /// Represents a Address search model
    /// </summary>
    public partial record EmpAddressSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Address.List.Address")]
        public string Address { get; set; }
        public int employeeId { get; set; }

        #endregion
    }
}