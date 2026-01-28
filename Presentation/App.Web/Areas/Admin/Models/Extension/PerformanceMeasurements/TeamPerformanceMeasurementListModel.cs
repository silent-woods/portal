using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a TeamPerformanceMeasurement list model
    /// </summary>
    public partial record TeamPerformanceMeasurementListModel : BasePagedListModel<TeamPerformanceMeasurementModel>
    {
    }
}