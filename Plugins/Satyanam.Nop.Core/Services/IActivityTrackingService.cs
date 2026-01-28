using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Campaings service interface
    /// </summary>
    public partial interface IActivityTrackingService
    {
        Task<IPagedList<ActivityTracking>> GetAllActivityTrackingAsync(IList<int> employeeIds, DateTime? from, DateTime? to,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<ActivityTracking> GetActivityTrackingByIdAsync(int Id);
        Task<IList<ActivityTracking>> GetActivityTrackingsByIdsAsync(int[] activityTrackingIds);

        Task InsertActivityTrackingAsync(ActivityTracking activityTracking);
        Task UpdateActivityTrackingAsync(ActivityTracking activityTracking);
        Task DeleteActivityTrackingAsync(ActivityTracking activityTracking);

        Task<IPagedList<ActivityTracking>> GetGroupedActivityTrackingSummaryAsync(
    IList<int> employeeIds, DateTime? from, DateTime? to,
    int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<DailyActivityTrackingModel>> GetDateWiseStatusSummaryByEmployeeAsync(
      int employeeId, DateTime from, DateTime to);
        Task<ActivityTracking> GetSummaryByEmployeeAndDateRangeAsync(int employeeId, DateTime from, DateTime to);

        Task<ActivityTracking> GetByEmployeeAndDateAsync(int employeeId, DateTime date);



    }
}