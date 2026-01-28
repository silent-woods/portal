using App.Core.Domain.PerformanceMeasurements;
using App.Web.Models.PerformanceMeasurements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the TeamPerformanceMeasurement model factory
    /// </summary>
    public partial interface IPerformanceModelFactory
    {
        //Task<TeamPerformanceMeasurementSearchModel> PrepareTeamPerformanceMeasurementSearchModelAsync(TeamPerformanceMeasurementSearchModel searchModel);

        //Task<TeamPerformanceMeasurementListModel> PrepareTeamPerformanceMeasurementListModelAsync(TeamPerformanceMeasurementSearchModel searchModel);

        Task<PerformanceMeasurementModel> PreparePerformanceMeasurementModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
        Task<AvgMeasurementModel> PrepareMonthlyReviewModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
        Task<AvgMeasurementModel> PrepareYearlyReviewModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);

        Task<AvgMeasurementModel> PrepareProjectLeaderReviewModelAsync(PerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
    }
}