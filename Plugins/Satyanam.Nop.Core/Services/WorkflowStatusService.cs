using App.Core;
using App.Core.Domain.ProjectTasks;
using App.Data;
using App.Data.Extensions;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.ProjectTasks;
using Humanizer;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Sql;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service
    /// </summary>
    public partial class WorkflowStatusService : IWorkflowStatusService
    {
        #region Fields

        private readonly IRepository<WorkflowStatus> _workflowStatusRepository;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectsService _projectsService;

        #endregion

        #region Ctor

        public WorkflowStatusService(IRepository<WorkflowStatus> workflowStatusRepository, IProjectTaskService projectTaskService, IProjectsService projectsService)
        {
            _workflowStatusRepository = workflowStatusRepository;
            _projectTaskService = projectTaskService;
            _projectsService = projectsService;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Gets all Task Comments
        /// </summary>
        /// <param name="taskid">taskid</param>
        /// <param name="statusid">statusid</param>
        /// <param name="taskid">name</param>
        /// <param name="employeeid">employeeid</param>
        /// <param name="from">from</param>
        /// <param name="to">to</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>

        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the title
        /// </returns>

        public virtual async Task<IPagedList<WorkflowStatus>> GetAllWorkflowStatusAsync(int processWorkflowId =0, string statusName = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _workflowStatusRepository.Table;

            if (processWorkflowId !=0)
                query = query.Where(c => c.ProcessWorkflowId == processWorkflowId);

            if (!string.IsNullOrWhiteSpace(statusName))
                query = query.Where(c => c.StatusName.Contains(statusName));

            query = query.OrderBy(c => c.DisplayOrder);

            return await Task.FromResult(new PagedList<WorkflowStatus>(query.ToList(), pageIndex, pageSize));
        }


        /// <summary>
        /// Gets a TaskComments
        /// </summary>
        /// <param name="Id">identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the campaings
        /// </returns>
        public virtual async Task<WorkflowStatus> GetWorkflowStatusByIdAsync(int Id)
        {
            return await _workflowStatusRepository.GetByIdAsync(Id);
        }

        public virtual async Task<IList<WorkflowStatus>> GetWorkflowStatusByIdsAsync(int[] workflowStatusIds)
        {
            return await _workflowStatusRepository.GetByIdsAsync(workflowStatusIds);
        }

        /// <summary>
        /// Inserts a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertWorkflowStatusAsync(WorkflowStatus workflowStatus)
        {
            if (workflowStatus.IsDefaultQAStatus)
            {
                var existingQaDefaults = await _workflowStatusRepository.Table
                    .Where(ws => ws.ProcessWorkflowId == workflowStatus.ProcessWorkflowId &&
                                 ws.IsDefaultQAStatus &&
                                 ws.Id != workflowStatus.Id)
                    .ToListAsync();
                foreach (var ws in existingQaDefaults)
                {
                    ws.IsDefaultQAStatus = false;
                    await _workflowStatusRepository.UpdateAsync(ws);
                }
            }
            if (workflowStatus.IsDefaultDeveloperStatus)
            {
                var existingDevDefaults = await _workflowStatusRepository
                    .Table
                    .Where(ws => ws.ProcessWorkflowId == workflowStatus.ProcessWorkflowId &&
                                 ws.IsDefaultDeveloperStatus &&
                                 ws.Id != workflowStatus.Id)
                    .ToListAsync();

                foreach (var ws in existingDevDefaults)
                {
                    ws.IsDefaultDeveloperStatus = false;
                    await _workflowStatusRepository.UpdateAsync(ws);
                }
            }
            await _workflowStatusRepository.InsertAsync(workflowStatus);
            
        }
        /// <summary>
        /// Updates the taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateWorkflowStatusAsync(WorkflowStatus workflowStatus)
        {
            if (workflowStatus.IsDefaultQAStatus)
            {
                var existingQaDefaults = await _workflowStatusRepository.Table
                    .Where(ws => ws.ProcessWorkflowId == workflowStatus.ProcessWorkflowId &&
                                 ws.IsDefaultQAStatus &&
                                 ws.Id != workflowStatus.Id)
                    .ToListAsync();
                foreach (var ws in existingQaDefaults)
                {
                    ws.IsDefaultQAStatus = false;
                    await _workflowStatusRepository.UpdateAsync(ws);
                }
            }
            if (workflowStatus.IsDefaultDeveloperStatus)
            {
                var existingDevDefaults = await _workflowStatusRepository
                    .Table
                    .Where(ws => ws.ProcessWorkflowId == workflowStatus.ProcessWorkflowId &&
                                 ws.IsDefaultDeveloperStatus &&
                                 ws.Id != workflowStatus.Id)
                    .ToListAsync();
                foreach (var ws in existingDevDefaults)
                {
                    ws.IsDefaultDeveloperStatus = false;
                    await _workflowStatusRepository.UpdateAsync(ws);
                }
            }
            await _workflowStatusRepository.UpdateAsync(workflowStatus);
        }

        /// <summary>
        /// Deletes a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteWorkflowStatusAsync(WorkflowStatus workflowStatus)
        {
            await _workflowStatusRepository.DeleteAsync(workflowStatus);
        }

        public virtual async Task<int> GetDefaultStatusIdByWorkflowId(int processWorkflowId)
        {
            var defaultStatus = await _workflowStatusRepository.Table
                .Where(s => s.ProcessWorkflowId == processWorkflowId)
                .OrderBy(s => s.DisplayOrder)
                .FirstOrDefaultAsync();

            return defaultStatus?.Id ?? 0; 
        }

        public async Task<bool> IsTaskOverdue(int taskId)
        {
            var task = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task == null || !task.DueDate.HasValue)
                return false;
            var workflowStatuses = await GetAllWorkflowStatusAsync(task.ProcessWorkflowId,"");
            if (workflowStatuses == null || !workflowStatuses.Any())
                return false;
            var newStatus = workflowStatuses.OrderBy(s => s.DisplayOrder).FirstOrDefault();
            var activeStatus = workflowStatuses.FirstOrDefault(s => s.IsDefaultDeveloperStatus);
            bool isInNewOrActive = (task.StatusId == newStatus?.Id) || (task.StatusId == activeStatus?.Id);
            bool isDueDatePassed = task.DueDate.Value.Date < DateTime.UtcNow.Date;
            return isInNewOrActive && isDueDatePassed;
        }

        public virtual async Task<int> GetAssignToIdByStatus(ProjectTask projectTask)
        {
            var status = await GetWorkflowStatusByIdAsync(projectTask.StatusId);
            int employeeId = projectTask.AssignedTo;
            switch (status?.StatusName?.ToLowerInvariant())
            {
                case "code review":
                    employeeId = await _projectsService.GetProjectCoordinatorIdByIdAsync(projectTask.ProjectId);
                    if (employeeId == 0)
                        employeeId = await _projectsService.GetProjectLeaderIdByIdAsync(projectTask.ProjectId);
                    if (employeeId == 0)
                        employeeId = await _projectsService.GetProjectManagerIdByIdAsync(projectTask.ProjectId);
                    break;

                case "ready to test":
                    employeeId = await _projectsService.GetProjectQAIdByIdAsync(projectTask.ProjectId);
                    break;

                case "test failed":
                    employeeId = projectTask.DeveloperId;
                    break;

                case "ready for live":
                    employeeId = projectTask.DeveloperId;
                    break;

                case "qa on live":
                    employeeId = await _projectsService.GetProjectQAIdByIdAsync(projectTask.ProjectId);
                    break;

                case "closed":
                    employeeId = projectTask.DeveloperId;
                    break;

                case "active":
                    employeeId = projectTask.DeveloperId;
                    break;
                case "code review done":
                    employeeId = projectTask.DeveloperId;
                    break;
            }
            return employeeId;

        }

        #endregion
    }
}