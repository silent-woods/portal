using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Projects;
using App.Core.Domain.Activities;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using Pipelines.Sockets.Unofficial.Arenas;
using StackExchange.Profiling.Internal;
using App.Core.Domain.ActivityEvents;

namespace App.Services.ActivityEvents
{
    /// <summary>
    /// Activity service
    /// </summary>
    public partial class ActivityEventService : IActivityEventService
    {
        #region Fields

        private readonly IRepository<Activity> _activityRepository;
        private readonly IRepository<ActivityEvent> _activityEventRepository;

        private readonly IRepository<TimeSheet> _timesheetRepository;


        #endregion

        #region Ctor

        public ActivityEventService(IRepository<Activity> activityRepository, IRepository<TimeSheet>  timesheetRepository,
            IRepository<ActivityEvent> activityEventRepository
           )
        {
            _activityRepository = activityRepository;
            _timesheetRepository = timesheetRepository;
            _activityEventRepository = activityEventRepository;


        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all activity
        /// </summary>
        /// <param name="activityName"></param>
        /// <param name="employeeId"></param>
        /// <param name="taskId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<ActivityEvent>> GetAllActivityEventAsync(int employeeId ,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        { 
            var query = await _activityEventRepository.GetAllAsync(async query =>
            {
                
                if (employeeId !=0)
                    query = query.Where(c => c.EmployeeId == employeeId);
                
                

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<ActivityEvent>(query.ToList(), pageIndex, pageSize);
        }



        /// <summary>
        /// Get activty by id
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public virtual async Task<ActivityEvent> GetActivityEventByIdAsync(int activityEventId)
        {
            return await _activityEventRepository.GetByIdAsync(activityEventId, cache => default);
        }

        /// <summary>
        /// Get activty by ids
        /// </summary>
        /// <param name="activityIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<ActivityEvent>> GetActivityEventsByIdsAsync(int[] activityEventIds)
        {
            return await _activityEventRepository.GetByIdsAsync(activityEventIds, cache => default, false);
        }

        /// <summary>
        /// Insert activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public virtual async Task InsertActivityEventAsync(ActivityEvent activityEvent)
        {
            await _activityEventRepository.InsertAsync(activityEvent);
        }

        /// <summary>
        /// Update activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateActivityEventAsync(ActivityEvent activityEvent)
        {
            if (activityEvent == null)
                throw new ArgumentNullException(nameof(activityEvent));

            await _activityEventRepository.UpdateAsync(activityEvent);
        }

        /// <summary>
        /// delete activity by record
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public virtual async Task DeleteActivityEventAsync(ActivityEvent activityEvent)
        {
            await _activityEventRepository.DeleteAsync(activityEvent, false);
        }

        /// <summary>
        /// get activity events by timesheet id
        /// </summary>
        /// <param name="timesheetId"></param>
        /// <returns></returns>
        public virtual async Task<IList<ActivityEvent>> GetActivityEventsByTimeSheetIdAsync(int timesheetId)
        {
            return await _activityEventRepository.Table
                .Where(ae => ae.TimesheetId == timesheetId)
                .ToListAsync();
        }

        #endregion
    }
}