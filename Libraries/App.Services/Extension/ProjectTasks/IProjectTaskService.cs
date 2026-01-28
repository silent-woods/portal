using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;

namespace App.Services.ProjectTasks
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface IProjectTaskService
    {
        /// <summary>
        /// Retrieves a paged list of project tasks based on the provided search criteria.
        /// </summary>
        /// <param name="projectTaskName">The name of the project task to search for.</param>
        /// <param name="pageIndex">The index of the page to retrieve. Default is 0.</param>
        /// <param name="pageSize">The number of items per page. Default is int.MaxValue.</param>
        /// <param name="showHidden">Whether to include hidden tasks. Default is false.</param>
        /// <param name="overridePublished">An optional flag to override publication status.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of an IPagedList of ProjectTask.</returns>
        Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(string projectTaskName, int projectId, int statusId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime = 0);

        Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(int taskId, int taskTypeId, IList<int> employeeIds, IList<int> projectIds, string taskName, DateTime? from, DateTime? to, DateTime? dueDate, int SelectedStatusId, int processWorkflowId,
  int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime = 0);
        /// <summary>
        /// Retrieves a project task by its unique identifier.
        /// </summary>
        /// <param name="projectTaskId">The unique identifier of the project task.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of the ProjectTask.</returns>
        Task<ProjectTask> GetProjectTasksByIdAsync(int projectTaskId, bool showHidden = false);
        Task<ProjectTask> GetProjectTasksWithoutCacheByIdAsync(int projectTaskId, bool showHidden = false);
        /// <summary>
        /// Retrieves a list of project tasks by their unique identifiers.
        /// </summary>
        /// <param name="projectTaskIds">An array of unique identifiers of the project tasks.</param>
        /// <returns>A task that represents the asynchronous operation, with a result of a list of ProjectTask.</returns>
        Task<IList<ProjectTask>> GetProjectsTasksByIdsAsync(int[] projectTaskIds);

        /// <summary>
        /// Inserts a new project task into the database.
        /// </summary>
        /// <param name="projectTask">The project task to insert.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task InsertProjectTaskAsync(ProjectTask projectTask);

        /// <summary>
        /// Updates an existing project task in the database.
        /// </summary>
        /// <param name="projectTask">The project task to update.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the project task parameter is null.</exception>
        Task UpdateProjectTaskAsync(ProjectTask projectTask);

        /// <summary>
        /// Deletes a project task from the database.
        /// </summary>
        /// <param name="projectTask">The project task to delete.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task DeleteProjectTaskAsync(ProjectTask projectTask);

        Task<IList<ProjectTask>> GetProjectTasksByProjectId(int projectId, bool showHidden = false);

        Task<IList<ProjectTask>> GetAllProjectTasksAsync(bool showHidden = false);

        Task<IList<ProjectTask>> GetAllProjectTasksByDateAsync(DateTime from, DateTime to, bool showHidden = false);

        Task<IList<ProjectTask>> GetProjectTasksByProjectIdForTimeSheet(int projectId, bool showHidden = false);
        Task<ProjectTask> GetProjectTaskByTitleAndProjectAsync(string taskTitle, int projectId, bool showHidden = false);


        Task<Project> GetProjectByTaskIdAsync(int taskId);

        Task<List<ProjectTask>> GetProjectTasksByIdsAsync(List<int> taskIds);

        Task<IList<ProjectTask>> GetParentTasksByProjectIdAsync(int projectId, bool showHidden = false);

        Task<IList<ProjectTask>> GetBugChildTasksByParentTaskIdAsync(int parentTaskId, bool showHidden = false);

        Task<IList<ProjectTask>> GetProjectTasksByParentIdAsync(int parentTaskId, bool showHidden = false);

        Task<(int Hours, int Minutes)> GetBugTimeByTaskIdAsync(int taskId);

        Task<decimal?> CalculateWorkQualityAsync(int projectTaskId);

        Task UpdateParentTaskWorkQualityAsync(ProjectTask oldTask, ProjectTask newTask);

        Task<decimal?> CalculateDeliveryPerformanceAsync(int projectTaskId);

        Task<bool> HasBugTasksAsync(int parentTaskId, bool showHidden = false);
    }
}