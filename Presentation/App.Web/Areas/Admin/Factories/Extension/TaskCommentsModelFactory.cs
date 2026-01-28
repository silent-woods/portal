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
    public partial class TaskCommentsModelFactory : ITaskCommentsModelFactory
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
        #endregion

        #region Ctor

        public TaskCommentsModelFactory(IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ITaskCommentsService taskCommentsService,
            IProjectTaskService projectTaskService
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

        public virtual async Task<TaskCommentsSearchModel> PrepareTaskCommentsSearchModelAsync(TaskCommentsSearchModel searchModel)
        {
          
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<TaskCommentsListModel> PrepareTaskCommentsListModelAsync
          (TaskCommentsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get project
            var taskComments = await _taskCommentsService.GetAllTaskCommentsAsync(taskid: searchModel.SearchTaskId,searchModel.SearchStatusId,searchModel.SearchEmployeeId,searchModel.From,searchModel.To,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new TaskCommentsListModel().PrepareToGridAsync(searchModel, taskComments, () =>
            {
                return taskComments.SelectAwait(async taskComment =>
                {
                    //fill in model values from the entity

                    var taskCommentsModel = taskComment.ToModel<TaskCommentsModel>();
                  

                   
                    ProjectTask projectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskCommentsModel.TaskId);
                    if(projectTask != null)
                        taskCommentsModel.TaskName= projectTask.TaskTitle;

                    
                    Employee emp = await _employeeService.GetEmployeeByIdAsync(taskCommentsModel.EmployeeId);
                    if (emp != null)
                        taskCommentsModel.EmployeeName = emp.FirstName + " " + emp.LastName;

               


                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");


                    if (taskCommentsModel.CreatedOn != null)
                        taskCommentsModel.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(taskComment.CreatedOn, istTimeZone);


                    return taskCommentsModel;

                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<TaskCommentsModel> PrepareTaskCommentsModelAsync(TaskCommentsModel model, TaskComments taskComments, bool excludeProperties = false)
        {
            if (taskComments != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = taskComments.ToModel<TaskCommentsModel>();

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
