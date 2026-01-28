using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Framework.Models.Extensions;
using App.Web.Models.Boards;
using App.Web.Models.Extensions.ProjectTasks;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial class ProjectTaskModelFactory : IProjectTaskModelFactory
    {
        #region Fields
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectsService _projectService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly ITaskCommentsService _taskCommentsService;
        private readonly ITaskChangeLogService _taskChangeLogService;
        private readonly IProcessWorkflowService _processWorkflowService;
        private readonly IProcessRulesService _processRulesService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICommonPluginService _commonPluginService;
        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IFollowUpTaskService _followUpTaskService;
        #endregion

        #region Ctor
        public ProjectTaskModelFactory(ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IProjectsService projectService,
            IProjectTaskService projectTaskService,
            ITimeSheetsService timeSheetsService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            ITaskCommentsService taskCommentsService,
            ITaskChangeLogService taskChangeLogService,
            IProcessWorkflowService processWorkflowService,
            IProcessRulesService processRulesService,
            IWorkflowStatusService workflowStatusService,
            ICommonPluginService commonPluginService,
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            ITaskCategoryService taskCategoryService,
            IFollowUpTaskService followUpTaskService)
        {
            _leaveManagementService = leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _projectTaskService = projectTaskService;
            _projectService = projectService;
            _timeSheetsService = timeSheetsService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _taskCommentsService = taskCommentsService;
            _taskChangeLogService = taskChangeLogService;
            _processWorkflowService = processWorkflowService;
            _processRulesService = processRulesService;
            _workflowStatusService = workflowStatusService;
            _commonPluginService = commonPluginService;
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _taskCategoryService = taskCategoryService;
            _followUpTaskService = followUpTaskService;
        }
        #endregion

        #region Utilities
        public virtual async Task PrepareProcessWorkflowListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var project = await _projectService.GetProjectsByIdAsync(model.ProjectId);
            if (project != null && !string.IsNullOrEmpty(project.ProcessWorkflowIds))
            {
                var workflowIds = project.ProcessWorkflowIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();
                var workflows = await _processWorkflowService.GetProcessWorkflowsByIdsAsync(workflowIds.ToArray());
                foreach (var workflow in workflows)
                {
                    model.AvailableProcessWorkflows.Add(new SelectListItem
                    {
                        Text = workflow.Name,
                        Value = workflow.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareTaskCategoriesListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.AvailableTaskCategories = new List<SelectListItem>
    {
        new SelectListItem { Text = "Select", Value = "0" }
    };

            if (model.ProjectId > 0)
            {
                var mappings = await _projectTaskCategoryMappingService.GetAllMappingsAsync(model.ProjectId,isActive:true);
                foreach (var mapping in mappings)
                {
                    var category = await _taskCategoryService.GetTaskCategoryByIdAsync(mapping.TaskCategoryId);
                    if (category != null)
                    {
                        model.AvailableTaskCategories.Add(new SelectListItem
                        {
                            Text = category.DisplayName,
                            Value = category.Id.ToString(),
                            Selected = (category.Id == model.TaskCategoryId)
                        });
                    }
                }
            }
        }
        public virtual async Task PrepareAllProcessWorkflowListAsync(ProjectTaskSearchModel searchModel)
        {           
            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync("");
            searchModel.AvailableProcessWorkflow.Add(new SelectListItem
            {
                Text = "All",
                Value = null
            });
            foreach (var workflow in workflows)
                {
                searchModel.AvailableProcessWorkflow.Add(new SelectListItem
                    {
                        Text = workflow.Name,
                        Value = workflow.Id.ToString()
                    });
                }
        }

        public virtual async Task PrepareParentTaskListAsync(ProjectTaskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            foreach(var projectid in searchModel.SelectedProjectIds) {
                var parentTasks = await _projectTaskService.GetParentTasksByProjectIdAsync(projectid);
                searchModel.AvailableParentTasks = parentTasks
                    .Select(t => new SelectListItem
                    {
                        Text = t.TaskTitle,
                        Value = t.Id.ToString()
                    })
                    .ToList();
            }
        }
        public virtual async Task PrepareStatusListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var status = await _processRulesService.GetPossibleStatusIds(model.ProcessWorkflowId,model.StatusId);
            foreach (var p in status)
            {
                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(p);
                if(workflowStatus !=null)
                    model.StatusList.Add(new SelectListItem
                    {
                        Text = $"{workflowStatus.StatusName}|||{workflowStatus.ColorCode}",
                        Value = workflowStatus.Id.ToString()
                    });
            }
        }
        public virtual async Task PrepareProjectListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Projects.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var projectTaskName = "";
            var projects = await _projectService.GetAllProjectsAsync(projectTaskName);

            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.ProjectName = p.ProjectTitle;
                    model.Projects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareDeliveryOnTimeFilterListAsync(ProjectTaskSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });
            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "Yes",
                Value = "1"
            });
            searchModel.AvailableDeliveryOnTime.Add(new SelectListItem
            {
                Text = "No",
                Value = "2"
            });
        }
        public virtual async Task PrepareProjectListByEmployeeAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var projects = await _projectService.GetProjectListByEmployee(model.EmployeeId);

            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.ProjectName = p.ProjectTitle;
                    model.Projects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareEmployeeListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
          
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareEmployeeListAsync(ProjectTaskSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareJuniorsEmployeeListAsync(ProjectTaskSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);

            foreach (var p in employees)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(p);
                if (employee != null)
                    model.AvailableEmployees.Add(new SelectListItem
                    {
                        Text = employee.FirstName + " " + employee.LastName,
                        Value = employee.Id.ToString()
                    });
            }
        }

        public virtual async Task PrepareJuniorsEmployeeListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);
            foreach (var p in employees)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(p);
                if (employee != null)
                    model.AvailableEmployees.Add(new SelectListItem
                    {
                        Text = employee.FirstName + " " + employee.LastName,
                        Value = employee.Id.ToString()
                    });
            }
        }
        public virtual async Task PrepareProjectListAsync(ProjectTaskSearchModel searchmodel)
        {
            if (searchmodel == null)
                throw new ArgumentNullException(nameof(searchmodel));
            var leaves = await _projectService.GetAllProjectsAsync();
            foreach (var p in leaves)
            {
                if (p.StatusId != 4)
                {
                    searchmodel.AvailableProjects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }

        public virtual async Task PrepareProjectListByEmployeeAsync(ProjectTaskSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var project = await _projectService.GetProjectListByEmployee(model.EmployeeId);
            foreach (var p in project)
            {
                model.AvailableProjects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareTaskCommentsListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model != null)
            {
                var taskComments = await _taskCommentsService.GetAllTaskCommentsAsync(model.Id, 0, 0, null, null);

                if (model.TaskCommentsModel == null)
                    model.TaskCommentsModel = new List<TaskCommentsModel>();

                foreach (var taskComment in taskComments)
                {
                    var commentModel = new TaskCommentsModel();
                    commentModel.Id = taskComment.Id;
                    commentModel.TaskId = taskComment.TaskId;
                    commentModel.EmployeeId = taskComment.EmployeeId;
                    var employee = await _employeeService.GetEmployeeByIdAsync(taskComment.EmployeeId);
                    if (employee != null)
                        commentModel.EmployeeName = employee.FirstName + " " + employee.LastName;
                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskComment.StatusId);
                    if (workflowStatus != null)
                        commentModel.StatusName = workflowStatus.StatusName;
                    commentModel.StatusId = taskComment.StatusId;
                    commentModel.Description = taskComment.Description;
                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    commentModel.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(taskComment.CreatedOn, istTimeZone);
                    model.TaskCommentsModel.Add(commentModel);
                }
            }
        }

        public virtual async Task PrepareTaskChangeLogListAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model != null)
            {
                var taskLogs = await _taskChangeLogService.GetAllTaskChangeLogAsync(model.Id,0,0,0,null,null,0);

                if (model.TaskChangeLogModel == null)
                    model.TaskChangeLogModel = new List<TaskChangeLogModel>();
                foreach (var taskLog in taskLogs)
                {
                    var taskLogModel = new TaskChangeLogModel();
                    taskLogModel.Id = taskLog.Id;
                    taskLogModel.TaskId = taskLog.TaskId;
                    taskLogModel.EmployeeId = taskLog.EmployeeId;
                    var employee = await _employeeService.GetEmployeeByIdAsync(taskLog.EmployeeId);
                    if (employee != null)
                        taskLogModel.EmployeeName = employee.FirstName + " " + employee.LastName;

                    taskLogModel.Notes = taskLog.Notes;
                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    taskLogModel.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(taskLog.CreatedOn, istTimeZone);

                    model.TaskChangeLogModel.Add(taskLogModel);
                }
            }
        }

        #endregion
        #region Methods
        public virtual async Task<ProjectTaskSearchModel> PrepareProjectTaskSearchModelAsync(ProjectTaskSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            var statusList = new List<SelectListItem>();
     
            await PrepareEmployeeListAsync(searchModel);
            await PrepareProjectListAsync(searchModel);
            await PrepareAllProcessWorkflowListAsync(searchModel);
            await PrepareDeliveryOnTimeFilterListAsync(searchModel);
            await PrepareParentTaskListAsync(searchModel);

            statusList.Insert(0, new SelectListItem
            {
                Text = "All",
                Value = null
            });
            searchModel.AvailableQaRequired.Insert(0, new SelectListItem
            {
                Text = "All",
                Value = null
            });
            searchModel.AvailableQaRequired.Insert(1, new SelectListItem
            {
                Text = "QA Required",
                Value = "1"
            });
            searchModel.AvailableQaRequired.Insert(2, new SelectListItem
            {
                Text = "QA Not-Required",
                Value = "2"
            });
            searchModel.AvailableStatus = statusList;
            searchModel.AvailableTaskTypes = Enum.GetValues(typeof(TaskTypeEnum))
                .Cast<TaskTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();
            return searchModel;
        }

        public virtual async Task<ProjectTaskListModel> PrepareProjectTaskListModelAsync(ProjectTaskSearchModel searchModel)
        {

            var projectTask = await _projectTaskService.GetAllProjectTasksAsync(projectTaskName: searchModel.SearchTaskTitle, searchModel.SearchProjectId,
                searchModel.SearchStatusId, pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                showHidden: true);
            var model = await new ProjectTaskListModel().PrepareToGridAsync(searchModel, projectTask, () =>
            {
                return projectTask.SelectAwait(async projectTasks =>
                {
                    var projectTaskModel = projectTasks.ToModel<ProjectTaskModel>();                  
                    Project project = new Project();
                    project = await _projectService.GetProjectsByIdAsync(projectTasks.ProjectId);
                    Employee employee = await _employeeService.GetEmployeeByIdAsync(projectTasks.AssignedTo);
                    if (project == null)
                        return null;
                    if (project.IsDeleted == true)
                        return null;
                    projectTaskModel.ProjectId = project.Id;
                    projectTaskModel.StatusId = project.StatusId;
                    projectTaskModel.ProjectName = project.ProjectTitle;
                    projectTaskModel.EstimationTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(projectTasks.EstimatedTime);
                    projectTaskModel.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(projectTasks.SpentHours, projectTasks.SpentMinutes);
                    projectTaskModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(projectTasks.CreatedOnUtc, DateTimeKind.Utc);
                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(projectTasks.StatusId);
                    if (workflowStatus != null)
                        projectTaskModel.Status = workflowStatus.StatusName;
                    var parentTask = await _projectTaskService.GetProjectTasksByIdAsync(projectTasks.ParentTaskId);
                    if (parentTask != null)
                        projectTaskModel.ParentTaskName = parentTask.TaskTitle;
                    if (employee != null)
                    projectTaskModel.AssignedEmployee = employee.FirstName + " " + employee.LastName;
                    return projectTaskModel;
                }).Where(x => x != null);
            });
            await PrepareProjectListAsync(searchModel);

            return model;
        }
        public virtual async Task<ProjectTaskModel> PrepareProjectTaskModelAsync(ProjectTaskModel model, ProjectTask projectTask, bool excludeProperties = false)
        {           
            if (projectTask != null)
            {
                if (model == null)
                {
                    model = new ProjectTaskModel();
                    model.Id = projectTask.Id;
                    model.TaskTitle = projectTask.TaskTitle;
                    model.CreatedOnUtc = projectTask.CreatedOnUtc;
                    model.IsManualDOT = projectTask.IsManualDOT;
                    model.Description = projectTask.Description;
                    model.Tasktypeid = projectTask.Tasktypeid;
                    model.EstimatedTime = projectTask.EstimatedTime;
                    model.DueDate = projectTask.DueDate;
                    model.StatusId = projectTask.StatusId;
                    model.ProjectId = projectTask.ProjectId;
                    model.ProcessWorkflowId = projectTask.ProcessWorkflowId;
                    model.ParentTaskId = projectTask.ParentTaskId;
                    model.IsSync = projectTask.IsSync;
                    model.TaskCategoryId = projectTask.TaskCategoryId;
                    model.SearchPeriodId = (int)SearchPeriodEnum.CustomRange;

                    var project = await _projectService.GetProjectsByIdAsync(projectTask.ProjectId);
                    if (project != null)
                        model.ProjectName = project.ProjectTitle;
                    var followupTask = await _followUpTaskService.GetFollowUpTaskByTaskIdAsync(projectTask.Id);
                    if (followupTask != null)
                    {
                        model.FollowUpTaskId = followupTask.Id;
                    }
                    var taskTypeEnum = await TaskTypeEnum.Select.ToSelectListAsync();
                    model.TaskTypeList = taskTypeEnum.Select(store => new SelectListItem
                    {
                        Value = store.Value,
                        Text = store.Text,
                        Selected = model.Tasktypeid.ToString() == store.Value
                    }).ToList();
                    model.AssignedTo = projectTask.AssignedTo;
                    model.EstimationTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(model.EstimatedTime);                  
                    var totalSpent = await _timeSheetsService.ConvertSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes);
                    var spentTimeTable = await _timeSheetsService.GetSpentTimeWithTypesById(projectTask.Id);
                    var developerEmployee = await _employeeService.GetEmployeeByIdAsync(projectTask.DeveloperId);
                    if (developerEmployee != null)
                        model.DeveloperName = developerEmployee.FirstName + " " + developerEmployee.LastName;

                    model.SpentTimeTable = $@"
<table style='width:100%; border-collapse:collapse;'>
    <thead>
        <tr style='background-color:#f2f2f2; text-align:left;'>
            <th style='padding:5px; border:1px solid #ddd;'>Category</th>
            <th style='padding:5px; border:1px solid #ddd;'>Billable</th>
            <th style='padding:5px; border:1px solid #ddd;'>Non-Billable</th>
            <th style='padding:5px; border:1px solid #ddd; white-space:nowrap;'>Total</th>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td style='padding:5px; border:1px solid #ddd;'>Development</td>
            <td style='padding:5px; border:1px solid #ddd;'>{spentTimeTable.BillableDevelopmentTime}</td>
            <td style='padding:5px; border:1px solid #ddd;'>{spentTimeTable.NotBillableDevelopmentTime}</td>
            <td style='padding:5px; border:1px solid #ddd; white-space:nowrap;'>{spentTimeTable.TotalDevelopmentTime}</td>
        </tr>
        <tr>
            <td style='padding:5px; border:1px solid #ddd;'>QA</td>
            <td style='padding:5px; border:1px solid #ddd;'>{spentTimeTable.BillableQATime}</td>
            <td style='padding:5px; border:1px solid #ddd;'>{spentTimeTable.NotBillableQATime}</td>
            <td style='padding:5px; border:1px solid #ddd; white-space:nowrap;'>{spentTimeTable.TotalQATime}</td>
        </tr>
        <tr style='font-weight:bold; background-color:#f9f9f9;'>
            <td style='padding:5px; border:1px solid #ddd;'>Total</td>
            <td style='padding:5px; border:1px solid #ddd;'>{spentTimeTable.TotalBillableTime}</td>
            <td style='padding:5px; border:1px solid #ddd;'>{spentTimeTable.TotalNotBillableTime}</td>
            <td style='padding:5px; border:1px solid #ddd; white-space:nowrap;'>{spentTimeTable.TotalSpentTime}</td>
        </tr>
    </tbody>
</table>";

                    var employee = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                    if (employee != null)
                    {
                        model.AssignedEmployee = employee.FirstName + " " + employee.LastName;
                    }
                }
                var emp = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }
            var taskTypeList = await TaskTypeEnum.Select.ToSelectListAsync();

            model.TaskTypeList = taskTypeList.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.Tasktypeid.ToString() == store.Value
            }).ToList();
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            model.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.SearchPeriodId.ToString() == store.Value
            }).ToList();
            var childTasks = await _projectTaskService.GetProjectTasksByParentIdAsync(model.Id);
            if (childTasks != null && childTasks.Any())
            {
                var sb = new StringBuilder();
                sb.AppendLine("<table style='width:100%; border-collapse:collapse; margin-top:15px;'>");
                sb.AppendLine("<thead><tr style='background-color:#f2f2f2; text-align:left;'>");
                sb.AppendLine("<th style='padding:5px; border:1px solid #ddd;'>Child Task</th>");
                sb.AppendLine("</tr></thead><tbody>");

                foreach (var child in childTasks)
                {
                    sb.AppendLine("<tr>");
                    sb.AppendLine($"<td style='padding:5px; border:1px solid #ddd;'><a href='/ProjectTask/Edit?id={child.Id}' target='_blank' class='child-task-link'>{child.TaskTitle}</a></td>");
                    sb.AppendLine("</tr>");
                }

                sb.AppendLine("</tbody></table>");
                model.ChildTaskTable = sb.ToString();
            }
            await PrepareTaskCommentsListAsync(model);
            await PrepareEmployeeListAsync(model);
            await PrepareProcessWorkflowListAsync(model);
            await PrepareStatusListAsync(model);
            await PrepareTaskChangeLogListAsync(model);
            await PrepareTaskCategoriesListAsync(model);

            return model;
        }

        public virtual async Task<ProjectTaskModel> PrepareProjectTaskModelByEmployeeAsync(ProjectTaskModel model, ProjectTask projectTask, bool excludeProperties = false)
        {
            if (projectTask != null)
            {
                if (model == null)
                {
                    model = projectTask.ToModel<ProjectTaskModel>();
                    model.CreatedOnUtc = projectTask.CreatedOnUtc;
                    model.IsManualDOT = projectTask.IsManualDOT;
                    model.ParentTaskId = projectTask.ParentTaskId;
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                    if (employee != null)
                    {
                        model.AssignedEmployee = employee.FirstName + " " + employee.LastName;
                    }
                }
                var emp = await _employeeService.GetEmployeeByIdAsync(model.AssignedTo);
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }
            var taskTypeList = await TaskTypeEnum.Select.ToSelectListAsync();
            model.TaskTypeList = taskTypeList.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.Tasktypeid.ToString() == store.Value
            }).ToList();
            await PrepareJuniorsEmployeeListAsync(model);
            await PrepareProjectListByEmployeeAsync(model);
            return model;
        }
        #endregion
    }
}


