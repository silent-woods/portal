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
using App.Core.Domain.ProjectTasks;
using App.Services.TimeSheets;

namespace App.Services.Activities
{
    /// <summary>
    /// Activity service
    /// </summary>
    public partial class ActivityService : IActivityService
    {
        #region Fields

        private readonly IRepository<Activity> _activityRepository;
        private readonly IRepository<TimeSheet> _timesheetRepository;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
       


        #endregion

        #region Ctor

        public ActivityService(IRepository<Activity> activityRepository, IRepository<TimeSheet>  timesheetRepository,IRepository<ProjectTask> projectTaskRepository
           )
        {
            _activityRepository = activityRepository;
            _timesheetRepository = timesheetRepository;
            _projectTaskRepository = projectTaskRepository;
     


        }

        #endregion
        #region Utilities

        protected virtual async Task<(int SpentHours, int SpentMinutes)> ConvertSpentTimeAsync(string spentTime)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(spentTime) || !spentTime.Contains(":") || spentTime == null)
                {
                    throw new ArgumentException("Invalid spentTime format. Expected format: HH:MM");
                }

                var timeParts = spentTime.Split(':');
                if (timeParts.Length != 2 || !int.TryParse(timeParts[0], out int hours) || !int.TryParse(timeParts[1], out int minutes))
                {
                    throw new ArgumentException("Invalid spentTime format. Expected format: HH:MM");
                }

                return (hours, minutes);
            });
        }


        protected virtual async Task<string> ConvertSpentTimeAsync(int SpentHours, int SpentMinutes)
        {
            return await Task.FromResult($"{SpentHours:D2}:{SpentMinutes:D2}");
        }
        protected virtual async Task<(int SpentHours, int SpentMinutes)> AddSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes)
        {
            return await Task.Run(() =>
            {
                // Convert everything to total minutes
                int totalMinutes = (prevSpentHours * 60 + prevSpentMinutes) + (spentHours * 60 + spentMinutes);

                // Convert back to hours and minutes
                int resultHours = totalMinutes / 60;
                int resultMinutes = totalMinutes % 60;

                // Return as a tuple (SpentHours, SpentMinutes)
                return (resultHours, resultMinutes);
            });
        }

        protected virtual async Task<(int SpentHours, int SpentMinutes)> SubtractSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes)
        {
            return await Task.Run(() =>
            {
                // Convert everything to total minutes
                int totalPrevMinutes = (prevSpentHours * 60) + prevSpentMinutes;
                int totalSpentMinutes = (spentHours * 60) + spentMinutes;

                // Subtract minutes (ensure it doesn't go negative)
                int remainingMinutes = totalPrevMinutes - totalSpentMinutes;
                if (remainingMinutes < 0)
                {
                    remainingMinutes = 0; // Prevent negative time
                }

                // Convert back to hours and minutes
                int resultHours = remainingMinutes / 60;
                int resultMinutes = remainingMinutes % 60;

                return (resultHours, resultMinutes);
            });
        }

        protected virtual async Task<decimal> ConvertToTotalHours(int spentHours, int spentMinutes)
        {
            if (spentHours < 0 || spentMinutes < 0 || spentMinutes >= 60)
            {
                throw new ArgumentException("Invalid time input. Minutes should be between 0 and 59.");
            }

            return spentHours + (spentMinutes / 60.0m);
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
        public virtual async Task<IPagedList<Activity>> GetAllActivitiesAsync(string activityName,int employeeId ,int projectId ,string taskTitle,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _activityRepository.GetAllAsync(async query =>
            {
                // Joining with ProjectTask table
                var activityWithTaskQuery = from activity in query
                                            join task in _projectTaskRepository.Table on activity.TaskId equals task.Id
                                            where (string.IsNullOrWhiteSpace(activityName) || activity.ActivityName.Contains(activityName)) &&
                                                  (employeeId == 0 || activity.EmployeeId == employeeId) &&
                                                  (projectId == 0 || task.ProjectId == projectId) &&
                                                  (string.IsNullOrWhiteSpace(taskTitle) || task.TaskTitle.Contains(taskTitle))
                                            select activity;

                return activityWithTaskQuery.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<Activity>(query.ToList(), pageIndex, pageSize);
        }


        public virtual async Task<IPagedList<Activity>> GetAllActivitiesAsync(string activityName, int employeeId, int taskId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _activityRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(activityName))
                    query = query.Where(c => c.ActivityName.Contains(activityName.Trim()));
                if (employeeId != 0)
                    query = query.Where(c => c.EmployeeId == employeeId);
                if (taskId != 0)
                    query = query.Where(c => c.TaskId == taskId);

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<Activity>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IPagedList<Activity>> GetAllActivitiesByActivityNameTaskIdAsync(string activityName, int taskId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _activityRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(activityName))
                    query = query.Where(c => c.ActivityName==activityName.Trim());
                
                if (taskId != 0)
                    query = query.Where(c => c.TaskId == taskId);

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<Activity>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get activty by id
        /// </summary>
        /// <param name="activityId"></param>
        /// <returns></returns>
        public virtual async Task<Activity> GetActivityByIdAsync(int activityId)
        {
            return await _activityRepository.GetByIdAsync(activityId);
        }

        /// <summary>
        /// Get activty by ids
        /// </summary>
        /// <param name="activityIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<Activity>> GetActivitiesByIdsAsync(int[] activityIds)
        {
            return await _activityRepository.GetByIdsAsync(activityIds, cache => default, false);
        }

        /// <summary>
        /// Insert activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public virtual async Task InsertActivityAsync(Activity activity)
        {
            await _activityRepository.InsertAsync(activity);
        }
        /// <summary>
        /// Insert activity with spent hours update in timesheet
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public virtual async Task InsertActivityWithTaskUpdateAsync(Activity activity)
        {
            var task = await _projectTaskRepository.GetByIdAsync(activity.TaskId);
            if(task != null)
            {
                //task.SpentHours += activity.SpentHours;
                (task.SpentHours, task.SpentMinutes) = await AddSpentTimeAsync(task.SpentHours, task.SpentMinutes, activity.SpentHours,activity.SpentMinutes);

                await _projectTaskRepository.UpdateAsync(task);
            }
            await _activityRepository.InsertAsync(activity);
        }

        /// <summary>
        /// Update activity
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateActivityAsync(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            await _activityRepository.UpdateAsync(activity);
        }

        /// <summary>
        /// Update activity with update spent hours in timesheet
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="previous activity"></param>
        /// 
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateActivityWithTaskUpdateAsync(Activity activity, Activity prevActivity)
        {
            if (activity == null)
                throw new ArgumentNullException(nameof(activity));

            var task = await _projectTaskRepository.GetByIdAsync(activity.TaskId);
            if (task != null)
            {
                if (activity.TaskId != prevActivity.TaskId)
                {
                    var prevTask = await _projectTaskRepository.GetByIdAsync(prevActivity.TaskId);
                    if (prevTask != null)
                    {
                        //prevTask.SpentHours -= prevActivity.SpentHours;
                        (prevTask.SpentHours, prevTask.SpentMinutes) = await SubtractSpentTimeAsync(prevTask.SpentHours, prevTask.SpentMinutes, prevActivity.SpentHours, prevActivity.SpentMinutes);

                        await _projectTaskRepository.UpdateAsync(prevTask);
                    }
                    var newTask = await _projectTaskRepository.GetByIdAsync(activity.TaskId);
                    if (newTask != null)
                    {
                        //newTask.SpentHours += activity.SpentHours;
                        (newTask.SpentHours, newTask.SpentMinutes) = await AddSpentTimeAsync(newTask.SpentHours, newTask.SpentMinutes, activity.SpentHours, activity.SpentMinutes);
                        await _projectTaskRepository.UpdateAsync(newTask);

                    }
                }
                else
                {
                    var newTask = await _projectTaskRepository.GetByIdAsync(activity.TaskId);
                    if(newTask != null)
                    {
                        //newTask.SpentHours -= prevActivity.SpentHours;
                        (newTask.SpentHours, newTask.SpentMinutes) = await SubtractSpentTimeAsync(newTask.SpentHours, newTask.SpentMinutes, prevActivity.SpentHours, prevActivity.SpentMinutes);
                        //newTask.SpentHours += activity.SpentHours;
                        (newTask.SpentHours, newTask.SpentMinutes) = await AddSpentTimeAsync(newTask.SpentHours, newTask.SpentMinutes, activity.SpentHours, activity.SpentMinutes);
                        await _projectTaskRepository.UpdateAsync(newTask);
                    }
                }
            }

            await _activityRepository.UpdateAsync(activity);
        }

        /// <summary>
        /// delete activity by record
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public virtual async Task DeleteActivityAsync(Activity activity)
        {
            await _activityRepository.DeleteAsync(activity, false);
        }

        /// <summary>
        /// delete activity by record with update of spent hours in timesheet
        /// </summary>
        /// <param name="activity"></param>
        /// <returns></returns>
        public virtual async Task DeleteActivityWithTaskUpdateAsync(Activity activity)
        {
            var task = await _projectTaskRepository.GetByIdAsync(activity.TaskId);
            if (task != null)
            {
                //task.SpentHours -= activity.SpentHours;
                (task.SpentHours, task.SpentMinutes) = await SubtractSpentTimeAsync(task.SpentHours, task.SpentMinutes, activity.SpentHours, activity.SpentMinutes);
                await _projectTaskRepository.UpdateAsync(task);
            }
            await _activityRepository.DeleteAsync(activity, false);
        }

        public virtual async Task<Activity> GetActivityByTaskIdAndActivityNameAsync(int taskId,string ActivityName)
        {

            // Ensure that activityName is not null or empty
            if (string.IsNullOrWhiteSpace(ActivityName))
                throw new ArgumentException("Activity name cannot be null or empty.", nameof(ActivityName));

            // Retrieve the activity based on TaskId and ActivityName
            var activity = await _activityRepository.Table
                .FirstOrDefaultAsync(a => a.TaskId == taskId && a.ActivityName==ActivityName);

            return activity;
        }


        #endregion
    }
}