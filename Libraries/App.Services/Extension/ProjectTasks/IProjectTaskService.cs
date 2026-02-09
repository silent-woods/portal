using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;

namespace App.Services.ProjectTasks
{
    public partial interface IProjectTaskService
    {
        Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(string projectTaskName, int projectId, int statusId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime = 0);
        Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(int taskId = 0, int taskTypeId = 0, IList<int> employeeIds = null, IList<int> projectIds = null, string taskName = null, DateTime? from = null, DateTime? to = null, DateTime? dueDate = null, int SelectedStatusId = 0, int processWorkflowId = 0,
 int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime = 0);
        Task<ProjectTask> GetProjectTasksByIdAsync(int projectTaskId, bool showHidden = false);
        Task<ProjectTask> GetProjectTasksWithoutCacheByIdAsync(int projectTaskId, bool showHidden = false);
        Task<IList<ProjectTask>> GetProjectsTasksByIdsAsync(int[] projectTaskIds);
        Task InsertProjectTaskAsync(ProjectTask projectTask);
        Task UpdateProjectTaskAsync(ProjectTask projectTask);
        Task DeleteProjectTaskAsync(ProjectTask projectTask);
        Task<IList<ProjectTask>> GetProjectTasksByProjectId(int projectId, bool showHidden = false);
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