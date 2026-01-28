using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Employees
{
    /// <summary>
    /// Represents a Education search model
    /// </summary>
    public partial record EducationSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Education.List.Education")]
        public string Education { get; set; }
        public int employeeId { get; set; } 
        #endregion
    }
}