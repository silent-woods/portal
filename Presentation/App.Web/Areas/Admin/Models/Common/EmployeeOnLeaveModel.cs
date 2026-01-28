using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents a popular search term model
    /// </summary>
    public partial record EmployeeOnLeaveModel : BaseNopModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.EmployeeOnLeave.Today")]
        public string Date { get; set; }

        [NopResourceDisplayName("Admin.EmployeeOnLeave.Tomorrow")]
        public string Emplyoee { get; set; }

        #endregion
    }
}
