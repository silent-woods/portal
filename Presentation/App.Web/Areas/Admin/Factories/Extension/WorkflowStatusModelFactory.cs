using App.Core.Domain.Customers;
using App.Core.Domain.Designations;
using App.Core.Domain.Employees;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory implementation
    /// </summary>
    public partial class WorkflowStatusModelFactory : IWorkflowStatusModelFactory
    {
        #region Fields

        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly IDesignationService _designationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly IProcessWorkflowService _processWorkflowService;
        #endregion

        #region Ctor

        public WorkflowStatusModelFactory(IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IWorkflowStatusService workflowStatusService,
            IProcessWorkflowService processWorkflowService
            )
        {
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _designationService = designationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _workflowStatusService = workflowStatusService;
            _processWorkflowService = processWorkflowService;
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

        public virtual async Task<WorkflowStatusSearchModel> PrepareWorkflowStatusSearchModelAsync(WorkflowStatusSearchModel searchModel)
        {
          
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<WorkflowStatusListModel> PrepareWorkflowStatusListModelAsync
          (WorkflowStatusSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get project
            var workflowStatuses = await _workflowStatusService.GetAllWorkflowStatusAsync(processWorkflowId: searchModel.ProcessWorkflowId, statusNames: new List<string>
    {
        searchModel.SearchStatusName
    },
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new WorkflowStatusListModel().PrepareToGridAsync(searchModel, workflowStatuses, () =>
            {
                return workflowStatuses.SelectAwait(async workflowStatus =>
                {
                    //fill in model values from the entity

                    var workflowStatusModel = workflowStatus.ToModel<WorkflowStatusModel>();
                    workflowStatusModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(workflowStatus.CreatedOn, DateTimeKind.Utc);
                    workflowStatusModel.StatusName = workflowStatusModel.StatusName + "|||" + workflowStatusModel.ColorCode;
                    //ProcessWorkflow process = await _processWorkflowService.GetProcessWorkflowByIdAsync(workflowStatusModel.ProcessWorkflowId);
                    //if(process != null)
                    //    workflowStatusModel.ProcessWorkflowName = process.Name;

                    return workflowStatusModel;

                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<WorkflowStatusModel> PrepareWorkflowStatusModelAsync(WorkflowStatusModel model, WorkflowStatus workflowStatus, bool excludeProperties = false)
        {
            if (workflowStatus != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = workflowStatus.ToModel<WorkflowStatusModel>();

                 
                }
               
              
            }

            
            //await PrepareProjectListAsync(model);
          
            //await PrepareRoleListAsync(model);
            return model;
        }
        #endregion
    }
}
