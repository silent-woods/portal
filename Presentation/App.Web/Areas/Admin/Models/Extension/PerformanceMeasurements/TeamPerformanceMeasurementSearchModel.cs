using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a TeamPerformanceMeasurement search model
    /// </summary>
    public partial record TeamPerformanceMeasurementSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.TeamPerformanceMeasurement.List.KPIName")]
        public string KPIName { get; set; }
        [NopResourceDisplayName("Admin.TeamPerformanceMeasurement.List.EmployeeName")]
        public string EmployeeName { get; set; }
        #endregion
    }
}