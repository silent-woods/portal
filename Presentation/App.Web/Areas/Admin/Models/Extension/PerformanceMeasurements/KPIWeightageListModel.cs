using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a kpiweightage list model
    /// </summary>
    public partial record KPIWeightageListModel : BasePagedListModel<KPIWeightageModel>
    {
    }
}