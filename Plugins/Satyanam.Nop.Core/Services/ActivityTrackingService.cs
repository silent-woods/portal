using App.Core;
using App.Data;
using App.Data.Extensions;
using App.Services.Messages;
using Humanizer;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Sql;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service
    /// </summary>
    public partial class ActivityTrackingService : IActivityTrackingService
    {
        #region Fields

        private readonly IRepository<ActivityTracking> _activityTrackingRepository;
      

        #endregion

        #region Ctor

        public ActivityTrackingService(IRepository<ActivityTracking> activityTrackingRepository)
        {
            _activityTrackingRepository = activityTrackingRepository;
  
        }

        #endregion

        #region Methods

        #region Campaings


        public virtual async Task<IPagedList<ActivityTracking>> GetAllActivityTrackingAsync(IList<int> employeeIds, DateTime? from, DateTime? to,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _activityTrackingRepository.Table;

            if (employeeIds != null && employeeIds.Any())
            {
                query = query.Where(c => employeeIds.Contains(c.EmployeeId));
            }

            if (from.HasValue)
            {
                query = query.Where(pr => pr.StartTime.Date >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(pr => pr.StartTime.Date <= to.Value);
            }

            query = query
         .OrderBy(c => c.EmployeeId)
         .ThenByDescending(c => c.StartTime.Date);


            return await Task.FromResult(new PagedList<ActivityTracking>(query.ToList(), pageIndex, pageSize));
        }


        public virtual async Task<IPagedList<ActivityTracking>> GetGroupedActivityTrackingSummaryAsync(
    IList<int> employeeIds, DateTime? from, DateTime? to,
    int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return await Task.FromResult(new PagedList<ActivityTracking>(new List<ActivityTracking>(), pageIndex, pageSize));
            }
            var query = _activityTrackingRepository.Table;

            if (employeeIds != null && employeeIds.Any())
                query = query.Where(c => employeeIds.Contains(c.EmployeeId));

            if (from.HasValue)
                query = query.Where(c => c.StartTime.Date >= from.Value);

            if (to.HasValue)
                query = query.Where(c => c.StartTime.Date <= to.Value);

            var grouped = query
                .GroupBy(c => c.EmployeeId)
                .Select(g => new ActivityTracking
                {
                    EmployeeId = g.Key,
                    ActiveDuration = g.Sum(x => x.ActiveDuration),
                    AwayDuration = g.Sum(x => x.AwayDuration),
                    OfflineDuration = g.Sum(x => x.OfflineDuration),
                    StoppedDuration = g.Sum(x => x.StoppedDuration),
                    TotalDuration = g.Sum(x => x.TotalDuration)
                })
                .OrderBy(g => g.EmployeeId) 
                .ToList();

            var pagedResult = new PagedList<ActivityTracking>(grouped, pageIndex, pageSize);
            return await Task.FromResult(pagedResult);
        }



        public virtual async Task<ActivityTracking> GetActivityTrackingByIdAsync(int Id)
        {
            return await _activityTrackingRepository.GetByIdAsync(Id);
        }

        public virtual async Task<IList<ActivityTracking>> GetActivityTrackingsByIdsAsync(int[] activityTrackingIds)
        {
            var activityTrackings = await _activityTrackingRepository.GetByIdsAsync(activityTrackingIds);

    

            return activityTrackings;
        }

        public virtual async Task InsertActivityTrackingAsync(ActivityTracking activityTracking)
        {
            await _activityTrackingRepository.InsertAsync(activityTracking);
            
        }

        public virtual async Task UpdateActivityTrackingAsync(ActivityTracking activityTracking)
        {
            await _activityTrackingRepository.UpdateAsync(activityTracking);
        }

        public virtual async Task DeleteActivityTrackingAsync(ActivityTracking activityTracking)
        {
            await _activityTrackingRepository.DeleteAsync(activityTracking);
        }

        public virtual async Task<IList<DailyActivityTrackingModel>> GetDateWiseStatusSummaryByEmployeeAsync(
      int employeeId, DateTime from, DateTime to)
        {
            var query = _activityTrackingRepository.Table
                .Where(x => x.EmployeeId == employeeId
                            && x.StartTime.Date >= from.Date
                            && x.StartTime.Date <= to.Date);

            var grouped = await query
                .GroupBy(x => x.StartTime.Date)
                .Select(g => new DailyActivityTrackingModel
                {
                    Date = g.Key,
                    ActiveDuration = g.Sum(x => x.ActiveDuration),
                    AwayDuration = g.Sum(x => x.AwayDuration),
                    OfflineDuration = g.Sum(x => x.OfflineDuration),
                    StoppedDuration = g.Sum(x => x.StoppedDuration),
                    TotalDuration = g.Sum(x => x.TotalDuration)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return grouped;
        }

        public virtual async Task<ActivityTracking> GetSummaryByEmployeeAndDateRangeAsync(int employeeId, DateTime from, DateTime to)
        {
            var fromDate = from.Date;
            var toDate = to.Date.AddDays(1); // to include the full "to" day

            var query = _activityTrackingRepository.Table
                .Where(at => at.EmployeeId == employeeId &&
                             at.StartTime >= fromDate &&
                             at.StartTime < toDate); // exclusive upper bound

            var data = await query.ToListAsync();

            var summary = new ActivityTracking
            {
                EmployeeId = employeeId,
                ActiveDuration = data.Sum(x => x.ActiveDuration),
                AwayDuration = data.Sum(x => x.AwayDuration),
                OfflineDuration = data.Sum(x => x.OfflineDuration),
                StoppedDuration = data.Sum(x => x.StoppedDuration),
                TotalDuration = data.Sum(x => x.TotalDuration),
            };

            return summary;
        }
                                                          
        
        
        public async Task<ActivityTracking> GetByEmployeeAndDateAsync(int employeeId, DateTime date)
        {
            var startDate = date.Date;
            var endDate = startDate.AddDays(1);

            return await _activityTrackingRepository.Table
                .Where(at => at.EmployeeId == employeeId
                             && at.StartTime >= startDate
                             && at.StartTime < endDate)
                .OrderBy(at => at.StartTime)
                .FirstOrDefaultAsync();
        }


        #endregion

        #endregion
    }
}