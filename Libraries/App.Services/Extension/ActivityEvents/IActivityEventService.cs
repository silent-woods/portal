using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.ActivityEvents;

namespace App.Services.ActivityEvents
{
    /// <summary>
    /// Activity Event service interface
    /// </summary>
    public partial interface IActivityEventService
    {
        /// <summary>
        /// Retrieves a paged list of activity events based on the provided search criteria.
        /// </summary>
        /// <param name="employeeId">The ID of the employee to filter activity events by.</param>
        /// <param name="taskId">The ID of the task to filter activity events by.</param>
        /// <param name="pageIndex">The index of the page to retrieve. Default is 0.</param>
        /// <param name="pageSize">The number of items per page. Default is int.MaxValue.</param>
        /// <param name="showHidden">Whether to include hidden activity events. Default is false.</param>
        /// <param name="overridePublished">An optional flag to override publication status.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of an IPagedList of ActivityEvent.</returns>
        Task<IPagedList<ActivityEvent>> GetAllActivityEventAsync(
            int employeeId,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            bool? overridePublished = null);

        /// <summary>
        /// Retrieves an activity event by its unique identifier.
        /// </summary>
        /// <param name="activityEventId">The unique identifier of the activity event.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of the ActivityEvent.</returns>
        Task<ActivityEvent> GetActivityEventByIdAsync(int activityEventId);

        /// <summary>
        /// Retrieves a list of activity events by their unique identifiers.
        /// </summary>
        /// <param name="activityEventIds">An array of unique identifiers of the activity events.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of a list of ActivityEvent.</returns>
        Task<IList<ActivityEvent>> GetActivityEventsByIdsAsync(int[] activityEventIds);

        /// <summary>
        /// Inserts a new activity event into the database.
        /// </summary>
        /// <param name="activityEvent">The activity event to insert.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task InsertActivityEventAsync(ActivityEvent activityEvent);

        /// <summary>
        /// Updates an existing activity event in the database.
        /// </summary>
        /// <param name="activityEvent">The activity event to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the activityEvent parameter is null.</exception>
        Task UpdateActivityEventAsync(ActivityEvent activityEvent);

        /// <summary>
        /// Deletes an activity event from the database.
        /// </summary>
        /// <param name="activityEvent">The activity event to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteActivityEventAsync(ActivityEvent activityEvent);


        Task<IList<ActivityEvent>> GetActivityEventsByTimeSheetIdAsync(int timesheetId);
    }
}
