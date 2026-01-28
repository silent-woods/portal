using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Activities;

namespace App.Services.Activities
{
    /// <summary>
    /// Activity service interface
    /// </summary>
    public partial interface IActivityService
    {
        /// <summary>
        /// Retrieves a paged list of activities based on the provided search criteria.
        /// </summary>
        /// <param name="activityName">The name of the activity to search for.</param>
        /// <param name="employeeId">The ID of the employee to filter activities by.</param>
        /// <param name="taskId">The ID of the task to filter activities by.</param>
        /// <param name="pageIndex">The index of the page to retrieve. Default is 0.</param>
        /// <param name="pageSize">The number of items per page. Default is int.MaxValue.</param>
        /// <param name="showHidden">Whether to include hidden activities. Default is false.</param>
        /// <param name="overridePublished">An optional flag to override publication status.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of an IPagedList of Activity.</returns>
        Task<IPagedList<Activity>> GetAllActivitiesAsync(string activityName, int employeeId, int projectId, string taskTitle,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        Task<IPagedList<Activity>> GetAllActivitiesAsync(string activityName, int employeeId, int taskId,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        Task<IPagedList<Activity>> GetAllActivitiesByActivityNameTaskIdAsync(string activityName, int taskId,
          int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        /// <summary>
        /// Retrieves an activity by its unique identifier.
        /// </summary>
        /// <param name="activityId">The unique identifier of the activity.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of the Activity.</returns>
        Task<Activity> GetActivityByIdAsync(int activityId);

        /// <summary>
        /// Retrieves a list of activities by their unique identifiers.
        /// </summary>
        /// <param name="activityIds">An array of unique identifiers of the activities.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of a list of Activity.</returns>
        Task<IList<Activity>> GetActivitiesByIdsAsync(int[] activityIds);

        /// <summary>
        /// Inserts a new activity into the database.
        /// </summary>
        /// <param name="activity">The activity to insert.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task InsertActivityAsync(Activity activity);

        Task InsertActivityWithTaskUpdateAsync(Activity activity);

        /// <summary>
        /// Updates an existing activity in the database.
        /// </summary>
        /// <param name="activity">The activity to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the activity parameter is null.</exception>
        Task UpdateActivityAsync(Activity activity);

        Task UpdateActivityWithTaskUpdateAsync(Activity activity, Activity prevActivity);

        /// <summary>
        /// Deletes an activity from the database.
        /// </summary>
        /// <param name="activity">The activity to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteActivityAsync(Activity activity);

        Task DeleteActivityWithTaskUpdateAsync(Activity activity);


        Task<Activity> GetActivityByTaskIdAndActivityNameAsync(int taskId, string ActivityName);



    }
}
