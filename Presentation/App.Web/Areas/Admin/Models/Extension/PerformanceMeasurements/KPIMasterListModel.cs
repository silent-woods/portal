using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a kpimaster list model
    /// </summary>
    public partial record KPIMasterListModel : BasePagedListModel<KPIMasterModel>
    {
    }
}