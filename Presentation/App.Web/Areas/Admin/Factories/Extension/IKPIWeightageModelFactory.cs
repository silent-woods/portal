using App.Core.Domain.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the kpiweightage model factory
    /// </summary>
    public partial interface IKPIWeightageModelFactory
    {
        Task<KPIWeightageSearchModel> PrepareKPIWeightageSearchModelAsync(KPIWeightageSearchModel searchModel);

        Task<KPIWeightageListModel> PrepareKPIWeightageListModelAsync(KPIWeightageSearchModel searchModel);

        Task<KPIWeightageModel> PrepareKPIWeightageModelAsync(KPIWeightageModel model, KPIWeightage kPIWeightage, bool excludeProperties = false);
    }
}