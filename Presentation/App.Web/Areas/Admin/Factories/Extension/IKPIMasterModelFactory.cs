using App.Core.Domain.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the kpimaster model factory
    /// </summary>
    public partial interface IKPIMasterModelFactory
    {
        Task<KPIMasterSearchModel> PrepareKPIMasterSearchModelAsync(KPIMasterSearchModel searchModel);

        Task<KPIMasterListModel> PrepareKPIMasterListModelAsync(KPIMasterSearchModel searchModel);

        Task<KPIMasterModel> PrepareKPIMasterModelAsync(KPIMasterModel model, KPIMaster kPIMaster, bool excludeProperties = false);
    }
}