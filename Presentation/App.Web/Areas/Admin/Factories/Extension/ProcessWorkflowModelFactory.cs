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
using App.Web.Areas.Admin.Models.Extension.ProcessWorkflows;
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
    public partial class ProcessWorkflowModelFactory : IProcessWorkflowModelFactory
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
        private readonly IProcessWorkflowService _processWorkflowService;
        #endregion

        #region Ctor

        public ProcessWorkflowModelFactory(IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ITaskCommentsService taskCommentsService,
            IProjectTaskService projectTaskService,
            ITaskChangeLogService taskChangeLogService,
            IProcessWorkflowService processWorkflowService
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
            _processWorkflowService = processWorkflowService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareProjectListAsync(ProjectModel model)
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

        public virtual async Task<ProcessWorkflowSearchModel> PrepareProcessWorkflowSearchModelAsync(ProcessWorkflowSearchModel searchModel)
        {
          
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<ProcessWorkflowListModel> PrepareProcessWorkflowListModelAsync
          (ProcessWorkflowSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get project
            var processWorkflows = await _processWorkflowService.GetAllProcessWorkflowsAsync(name:searchModel.SearchProcessWorkflowName,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new ProcessWorkflowListModel().PrepareToGridAsync(searchModel, processWorkflows, () =>
            {
                return processWorkflows.SelectAwait(async processWorkflow =>
                {
                    //fill in model values from the entity

                    var processWorkflowModel = processWorkflow.ToModel<ProcessWorkflowModel>();


                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                    if (processWorkflow.CreatedOn != null)
                        processWorkflowModel.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(processWorkflow.CreatedOn, istTimeZone);


                    return processWorkflowModel;

                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<ProcessWorkflowModel> PrepareProcessWorkflowModelAsync(ProcessWorkflowModel model, ProcessWorkflow processWorkflow, bool excludeProperties = false)
        {
            if (processWorkflow != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = processWorkflow.ToModel<ProcessWorkflowModel>();

                   
                }
              

              
         
            }

         
            return model;
        }
        #endregion
    }
}
