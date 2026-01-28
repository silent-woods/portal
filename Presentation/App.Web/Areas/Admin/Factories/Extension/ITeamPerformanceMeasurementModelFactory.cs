using App.Core.Domain.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the TeamPerformanceMeasurement model factory
    /// </summary>
    public partial interface ITeamPerformanceMeasurementModelFactory
    {
        Task<TeamPerformanceMeasurementSearchModel> PrepareTeamPerformanceMeasurementSearchModelAsync(TeamPerformanceMeasurementSearchModel searchModel);

        Task<TeamPerformanceMeasurementListModel> PrepareTeamPerformanceMeasurementListModelAsync(TeamPerformanceMeasurementSearchModel searchModel);

        Task<TeamPerformanceMeasurementModel> PrepareTeamPerformanceMeasurementModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
        Task<AvgMeasurementModel> PrepareMonthlyReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
        Task<AvgMeasurementModel> PrepareYearlyReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
        Task<AvgMeasurementModel> PrepareProjectLeaderReviewModelAsync(TeamPerformanceMeasurementModel model, TeamPerformanceMeasurement teamPerformance, bool excludeProperties = false);
    }
}