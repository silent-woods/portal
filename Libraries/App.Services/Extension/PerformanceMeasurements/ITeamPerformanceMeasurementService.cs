using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.PerformanceMeasurements;

namespace App.Services.PerformanceMeasurements
{
    /// <summary>
    /// TeamPerformanceMeasurement service interface
    /// </summary>
    public partial interface ITeamPerformanceMeasurementService
    {
        /// <summary>
        /// Gets all TeamPerformanceMeasurement
        /// </summary>
        Task<IPagedList<TeamPerformanceMeasurement>> GetAllTeamPerformanceMeasurementAsync(
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get TeamPerformanceMeasurement by id
        /// </summary>
        /// <param name="teamPerformanceId"></param>
        /// <returns></returns>
        Task<TeamPerformanceMeasurement> GetTeamPerformanceMeasurementByIdAsync(int teamPerformanceId);

        /// <summary>
        /// Insert TeamPerformanceMeasurement
        /// </summary>
        /// <param name="teamPerformance"></param>
        /// <returns></returns>
        Task InsertTeamPerformanceMeasurementAsync(TeamPerformanceMeasurement teamPerformance);

        /// <summary>
        /// Update TeamPerformanceMeasurement
        /// </summary>
        /// <param name="teamPerformance"></param>
        /// <returns></returns>
        Task UpdateTeamPerformanceMeasurementAsync(TeamPerformanceMeasurement teamPerformance);

        /// <summary>
        /// Delete TeamPerformanceMeasurement
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        Task DeleteTeamPerformanceMeasurementAsync(TeamPerformanceMeasurement teamPerformance);

        /// <summary>
        /// Get TeamPerformanceMeasurement by ids
        /// </summary>
        /// <param name="teamPerformanceIds"></param>
        /// <returns></returns>
        Task<IList<TeamPerformanceMeasurement>> GetTeamPerformanceMeasurementByIdsAsync(int[] teamPerformanceIds);
        // Task<IList<int>> GetTeamPerformanceMeasurementBykpiNameAsync(string kpiName);
        // Task<IList<int>> GetTeamPerformanceMeasurementByEmployeeNameAsync(string employeename);
        Task<bool> IsEmployeeCanAddRatings(int employeeId);

        Task<IList<Employee>> GetEmployeeForRatingReminder(int employeeId, int previousMonth, int previousYear);
    }
}