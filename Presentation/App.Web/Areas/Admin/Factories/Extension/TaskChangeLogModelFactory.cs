using App.Core.Domain.Customers;
using App.Core.Domain.Designations;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory implementation
    /// </summary>
    public partial class TaskChangeLogModelFactory : ITaskChangeLogModelFactory
    {
        #region Fields

        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly IDesignationService _designationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ITaskCommentsService _taskCommentsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly ITaskChangeLogService _taskChangeLogService;
        private readonly IWorkflowStatusService _workflowStatusService;
        #endregion

        #region Ctor

        public TaskChangeLogModelFactory(IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ITaskCommentsService taskCommentsService,
            IProjectTaskService projectTaskService,
            ITaskChangeLogService taskChangeLogService,
            IWorkflowStatusService workflowStatusService
            )
        {
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _designationService = designationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _taskCommentsService = taskCommentsService;
            _projectTaskService = projectTaskService;
            _taskChangeLogService = taskChangeLogService;
            _workflowStatusService = workflowStatusService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareProjectListAsync(ProjectEmployeeMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Projects.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var projectName = "";
            var project = await _projectService.GetAllProjectsAsync(projectName);
            foreach (var p in project)
            {
                model.Projects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareEmployeeListAsync(ProjectEmployeeMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Projectsemp.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.Projectsemp.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareRoleListAsync(ProjectEmployeeMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Roles.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var roleName = "";
            var role = await _designationService.GetAllDesignationAsync(roleName);
            foreach (var p in role)
            {
                model.Roles.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        #endregion

        #region Methods

        public virtual async Task<TaskChangeLogSearchModel> PrepareTaskChangeLogSearchModelAsync(TaskChangeLogSearchModel searchModel)
        {
          
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<TaskChangeLogListModel> PrepareTaskChangeLogListModelAsync
          (TaskChangeLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get project
            var taskChangeLogs = await _taskChangeLogService.GetAllTaskChangeLogAsync(taskid: searchModel.SearchTaskId,searchModel.SearchStatusId,searchModel.SearchEmployeeId,searchModel.SearchLogTypeId,searchModel.From,searchModel.To,searchModel.SearchAssignedToId,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new TaskChangeLogListModel().PrepareToGridAsync(searchModel, taskChangeLogs, () =>
            {
                return taskChangeLogs.SelectAwait(async taskChangeLog =>
                {
                    //fill in model values from the entity

                    var taskChangeLogModel = taskChangeLog.ToModel<TaskChangeLogModel>();


                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    if (taskChangeLog.CreatedOn != null)
                        taskChangeLogModel.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(taskChangeLog.CreatedOn, istTimeZone);

                    ProjectTask projectTask = new ProjectTask();
                    projectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskChangeLogModel.TaskId);
                    if(projectTask != null)
                        taskChangeLogModel.TaskName= projectTask.TaskTitle;

                    Employee emp = await _employeeService.GetEmployeeByIdAsync(taskChangeLogModel.EmployeeId);
                    if (emp != null)
                        taskChangeLogModel.EmployeeName = emp.FirstName + " " + emp.LastName;

                 Employee assignedTo = await _employeeService.GetEmployeeByIdAsync(taskChangeLogModel.AssignedTo);
                    if (assignedTo != null)
                        taskChangeLogModel.AssignedToName = assignedTo.FirstName + " " + assignedTo.LastName;

                 

                    var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(taskChangeLogModel.StatusId);
                    if(workflowStatus !=null)
                    taskChangeLogModel.StatusName = workflowStatus.StatusName;


                    var selectedOptionLogType = taskChangeLogModel.LogTypeId;
                    taskChangeLogModel.LogTypeName = ((LogTypeEnum)selectedOptionLogType).ToString();

                    return taskChangeLogModel;

                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<TaskChangeLogModel> PrepareTaskChangeLogModelAsync(TaskChangeLogModel model, TaskChangeLog taskChangeLog, bool excludeProperties = false)
        {
            if (taskChangeLog != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = taskChangeLog.ToModel<TaskChangeLogModel>();

                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }
                }
                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);

              
         
            }

         
            return model;
        }
        #endregion
    }
}
