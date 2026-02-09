using App.Core;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.ProjectTasks;
using App.Data;
using App.Data.Extensions;
using App.Services.Employees;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using Humanizer;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LinqToDB.Sql;

namespace Satyanam.Nop.Core.Services
{
    public partial class TaskChangeLogService : ITaskChangeLogService
    {
        #region Fields

        private readonly IRepository<TaskChangeLog> _taskChangeLogRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly ICheckListMasterService _checkListMasterService;
        private readonly ITaskCategoryService _taskCategoryService;
        #endregion

        #region Ctor

        public TaskChangeLogService(IRepository<TaskChangeLog> taskChangeLogRepository, IEmployeeService employeeService, ITimeSheetsService timeSheetsService, IProcessWorkflowService processWorkflowService, IWorkflowStatusService workflowStatusService, IProjectTaskService projectTaskService, ICheckListMasterService checkListMasterService, ITaskCategoryService taskCategoryService)
        {
            _taskChangeLogRepository = taskChangeLogRepository;
            _employeeService = employeeService;
            _timeSheetsService = timeSheetsService;
            _processWorkflowService = processWorkflowService;
            _workflowStatusService = workflowStatusService;
            _projectTaskService = projectTaskService;
            _checkListMasterService = checkListMasterService;
            _taskCategoryService = taskCategoryService;
        }

        #endregion

        #region Methods

        #region TaskChangeLog

        public virtual async Task<IPagedList<TaskChangeLog>> GetAllTaskChangeLogAsync(int taskid, int statusid, int employeeid, int logTypeId,DateTime? from, DateTime? to, int assignedTo,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _taskChangeLogRepository.Table;
            if (statusid > 0)
                query = query.Where(c => c.StatusId == statusid);

            if(taskid > 0)
                query = query.Where(c => c.TaskId == taskid);

            if (statusid > 0)
                query = query.Where(c => c.StatusId == statusid);

            if (assignedTo > 0)
                query = query.Where(c => c.AssignedTo == assignedTo);

            if(logTypeId >0)
                query = query.Where(c => c.LogTypeId == logTypeId);

            if (from.HasValue)
                query = query.Where(pr => from.Value <= pr.CreatedOn);
            if (to.HasValue)
                query = query.Where(pr => to.Value >= pr.CreatedOn);

            query = query.OrderByDescending(c => c.Id);

            return await Task.FromResult(new PagedList<TaskChangeLog>(query.ToList(), pageIndex, pageSize));
        }

        public virtual async Task<TaskChangeLog> GetTaskChangeLogByIdAsync(int Id)
        {
            return await _taskChangeLogRepository.GetByIdAsync(Id);
        }
        public virtual async Task<IList<TaskChangeLog>> GetTaskChangeLogByIdsAsync(int[] taskCommentIds)
        {
            return await _taskChangeLogRepository.GetByIdsAsync(taskCommentIds);
        }

        public virtual async Task InsertTaskChangeLogAsync(TaskChangeLog taskComments)
        {
            await _taskChangeLogRepository.InsertAsync(taskComments);
        }

        public virtual async Task UpdateTaskChangeLogAsync(TaskChangeLog taskComments)
        {
            await _taskChangeLogRepository.UpdateAsync(taskComments);
        }

        public virtual async Task DeleteTaskChangeLogAsync(TaskChangeLog taskComments)
        {
            await _taskChangeLogRepository.DeleteAsync(taskComments);
        }

        public virtual async Task InsertTaskChangeLogByUpdateTaskAsync(ProjectTask prevProjectTask, ProjectTask newProjectTask,int currentEmployeeId, List<TaskChecklistItemDto> checklistEntries = null) 
        {
            if (prevProjectTask != null && newProjectTask != null)
            {
                if (prevProjectTask.StatusId != newProjectTask.StatusId)
                {
                    var projectChangeLog = new TaskChangeLog();

                    var oldStatus = prevProjectTask.StatusId > 0
                        ? await _workflowStatusService.GetWorkflowStatusByIdAsync(prevProjectTask.StatusId)
                        : null;

                    var newStatus = newProjectTask.StatusId > 0
                        ? await _workflowStatusService.GetWorkflowStatusByIdAsync(newProjectTask.StatusId)
                        : null;

                    string oldStatusName = oldStatus?.StatusName ?? "N/A";
                    string newStatusName = newStatus?.StatusName ?? "N/A";

                    var logNote = $"Status Change From {oldStatusName} To {newStatusName}";

                    if (checklistEntries != null && checklistEntries.Any())
                    {
                        var checklistDetails = new StringBuilder();

                        foreach (var entry in checklistEntries)
                        {
                            var checklistItem = await _checkListMasterService.GetCheckListByIdAsync(entry.CheckListId);
                            if (checklistItem != null)
                            {
                                string emoji = entry.IsChecked ? "✅" : "❌";
                                checklistDetails.Append($"• {checklistItem.Title} {emoji}<br>");
                            }
                        }
                        logNote += "<br>" + checklistDetails.ToString();
                    }

                    projectChangeLog.LogTypeId = 1;
                    projectChangeLog.Notes = logNote;
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }


                if (prevProjectTask.AssignedTo != newProjectTask.AssignedTo)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();

                    var oldEmployee = prevProjectTask.AssignedTo > 0
                        ? await _employeeService.GetEmployeeByIdAsync(prevProjectTask.AssignedTo)
                        : null;

                    var newEmployee = newProjectTask.AssignedTo > 0
                        ? await _employeeService.GetEmployeeByIdAsync(newProjectTask.AssignedTo)
                        : null;

                    projectChangeLog.LogTypeId = 2;

                    string oldName = oldEmployee != null ? $"{oldEmployee.FirstName} {oldEmployee.LastName}" : "N/A";
                    string newName = newEmployee != null ? $"{newEmployee.FirstName} {newEmployee.LastName}" : "N/A";

                    projectChangeLog.Notes = $"AssignedTo Change From {oldName} To {newName}";

                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }

                string prevEstimation = await _timeSheetsService.ConvertToHHMMFormat(prevProjectTask.EstimatedTime);
                string newEstimation = await _timeSheetsService.ConvertToHHMMFormat(newProjectTask.EstimatedTime);

                if (prevEstimation != newEstimation)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();

                    projectChangeLog.LogTypeId = 3;
                    projectChangeLog.Notes = $"Estimation Time Change From {prevEstimation} To {newEstimation}";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }


                if (prevProjectTask.DueDate != newProjectTask.DueDate)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();
                    projectChangeLog.LogTypeId = 5;
                    projectChangeLog.Notes = $"Due Date Change {(prevProjectTask.DueDate.HasValue ? prevProjectTask.DueDate.Value.ToString("d-MMM-yyyy") : "N/A")} To {(newProjectTask.DueDate.HasValue ? newProjectTask.DueDate.Value.ToString("d-MMM-yyyy") : "N/A")}";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }

                if (prevProjectTask.TaskTitle != newProjectTask.TaskTitle)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();
                    projectChangeLog.LogTypeId = 4;
                    projectChangeLog.Notes = $"Task Name Change To '{prevProjectTask.TaskTitle}' To '{newProjectTask.TaskTitle}'";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }

                if (prevProjectTask.ProcessWorkflowId != newProjectTask.ProcessWorkflowId)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();

                    var oldWorkflow = prevProjectTask.ProcessWorkflowId > 0
                        ? await _processWorkflowService.GetProcessWorkflowByIdAsync(prevProjectTask.ProcessWorkflowId)
                        : null;

                    var newWorkflow = newProjectTask.ProcessWorkflowId > 0
                        ? await _processWorkflowService.GetProcessWorkflowByIdAsync(newProjectTask.ProcessWorkflowId)
                        : null;

                    string oldName = oldWorkflow != null ? $"{oldWorkflow.Name}" : "N/A";
                    string newName = newWorkflow != null ? $"{newWorkflow.Name}" : "N/A";
                    projectChangeLog.LogTypeId = 4;
                    projectChangeLog.Notes = $"Process Workflow is Change From {oldName} To {newName}";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }
                if (prevProjectTask.TaskCategoryId != newProjectTask.TaskCategoryId)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();
                    
                    var oldTaskCategory = prevProjectTask.TaskCategoryId > 0
                        ? await _taskCategoryService.GetTaskCategoryByIdAsync(prevProjectTask.TaskCategoryId)
                        : null;

                    var newTaskCategory = newProjectTask.TaskCategoryId > 0
                        ? await _taskCategoryService.GetTaskCategoryByIdAsync(newProjectTask.TaskCategoryId)
                        : null;
                    string oldName = oldTaskCategory != null ? $"{oldTaskCategory.DisplayName}" : "N/A";
                    string newName = newTaskCategory != null ? $"{newTaskCategory.DisplayName}" : "N/A";
                    projectChangeLog.LogTypeId = 4;
                    projectChangeLog.Notes = $"Task Category is Change From {oldName} To {newName}";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }
                if (prevProjectTask.Tasktypeid != newProjectTask.Tasktypeid)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();
                    projectChangeLog.LogTypeId = 4;
                    var prevSelectedOption = prevProjectTask.Tasktypeid;
                    string prevStatus = ((TaskTypeEnum)prevSelectedOption).ToString();
                    var newSelectedOption = newProjectTask.Tasktypeid;
                    string newStatus = ((TaskTypeEnum)newSelectedOption).ToString();
                    projectChangeLog.Notes = $"Task Type Change From {prevStatus} To {newStatus}";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }

                if (prevProjectTask.ParentTaskId != newProjectTask.ParentTaskId)
                {
                    var parentChangeLog = new TaskChangeLog();
                 
                    var oldParent = prevProjectTask.ParentTaskId > 0
                        ? await _projectTaskService.GetProjectTasksByIdAsync(prevProjectTask.ParentTaskId)
                        : null;

                    var newParent = newProjectTask.ParentTaskId > 0
                        ? await _projectTaskService.GetProjectTasksByIdAsync(newProjectTask.ParentTaskId)
                        : null;

                    string oldParentName = oldParent != null ? oldParent.TaskTitle : "N/A";
                    string newParentName = newParent != null ? newParent.TaskTitle : "N/A";
                    parentChangeLog.LogTypeId = 4; 
                    parentChangeLog.Notes = $"Parent Task changed from {oldParentName} to {newParentName}";
                    parentChangeLog.StatusId = newProjectTask.StatusId;
                    parentChangeLog.TaskId = newProjectTask.Id;
                    parentChangeLog.CreatedOn = DateTime.UtcNow;
                    parentChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    parentChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(parentChangeLog);
                }

                if (prevProjectTask.IsSync != newProjectTask.IsSync)
                {
                    TaskChangeLog projectChangeLog = new TaskChangeLog();
                    projectChangeLog.LogTypeId = 4; 
                    projectChangeLog.Notes = $"IsSync status changed from '{(prevProjectTask.IsSync ? "Yes" : "No")}' to '{(newProjectTask.IsSync ? "Yes" : "No")}'";
                    projectChangeLog.StatusId = newProjectTask.StatusId;
                    projectChangeLog.TaskId = newProjectTask.Id;
                    projectChangeLog.CreatedOn = DateTime.UtcNow;
                    projectChangeLog.AssignedTo = newProjectTask.AssignedTo;
                    projectChangeLog.EmployeeId = currentEmployeeId;

                    await InsertTaskChangeLogAsync(projectChangeLog);
                }
            }
        }

        public async Task<DateTime?> GetCurrentStatusStartDateAsync(int taskId,int statusId)
        {
            var logs = await _taskChangeLogRepository.Table
                .Where(x => x.TaskId == taskId)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new
                {
                    x.StatusId,
                    x.CreatedOn
                })
                .ToListAsync();

            if (!logs.Any())
                return null;

            DateTime? statusStartDate = null;

            foreach (var log in logs)
            {
                if (log.StatusId == statusId)
                {
                    statusStartDate = log.CreatedOn;
                    continue;
                }
                if (statusStartDate.HasValue)
                    break;
            }
            return statusStartDate;
        }
        #endregion
        #endregion
    }
}