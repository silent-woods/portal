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
using App.Web.Areas.Admin.Models.Extension.ProcessRules;
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory implementation
    /// </summary>
    public partial class ProcessRulesModelFactory : IProcessRulesModelFactory
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
        private readonly IProcessRulesService _processRulesService;
        #endregion

        #region Ctor

        public ProcessRulesModelFactory(IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IWorkflowStatusService workflowStatusService,
            IProcessWorkflowService processWorkflowService
,
            IProcessRulesService processRulesService)
        {
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _designationService = designationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _workflowStatusService = workflowStatusService;
            _processWorkflowService = processWorkflowService;
            _processRulesService = processRulesService;
        }

        #endregion
        #region Utilities

        public virtual async Task PrepareStatusListAsync(ProcessRulesModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.StateList.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
        
            var allWorkflowStatus = await _workflowStatusService.GetAllWorkflowStatusAsync(model.ProcessWorkflowId);
            foreach (var p in allWorkflowStatus)
            {
                model.StateList.Add(new SelectListItem
                {
                    Text = p.StatusName,
                    Value = p.Id.ToString()
                });
            }
        }
       
       
        #endregion

        #region Methods

        public virtual async Task<ProcessRulesSearchModel> PrepareProcessRulesSearchModelAsync(ProcessRulesSearchModel searchModel)
        {
          
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<ProcessRulesListModel> PrepareProcessRulesListModelAsync
          (ProcessRulesSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var processRules = await _processRulesService.GetAllProcessRulesAsync(processWorkflowId: searchModel.ProcessWorkflowId,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var model = await new ProcessRulesListModel().PrepareToGridAsync(searchModel, processRules, () =>
            {
                return processRules.SelectAwait(async processRule =>
                {
                    var processRulesModel = processRule.ToModel<ProcessRulesModel>();
                    processRulesModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(processRule.CreatedOn, DateTimeKind.Utc);
                    WorkflowStatus fromWorkflowStatus =await _workflowStatusService.GetWorkflowStatusByIdAsync(processRule.FromStateId);
                    if (fromWorkflowStatus != null)
                        processRulesModel.FromStateName = fromWorkflowStatus.StatusName;
                    WorkflowStatus toWorkflowState = await _workflowStatusService.GetWorkflowStatusByIdAsync(processRule.ToStateId);
                    if (toWorkflowState != null)
                        processRulesModel.ToStateName = toWorkflowState.StatusName;
                    return processRulesModel;

                });
            });
            return model;
        }
        public virtual async Task<ProcessRulesModel> PrepareProcessRulesModelAsync(ProcessRulesModel model, ProcessRules processRules, bool excludeProperties = false)
        {
            if (processRules != null)
            {
                if (model == null)
                {
                    model = processRules.ToModel<ProcessRulesModel>();
                }           
            }        
            await PrepareStatusListAsync(model);
            return model;
        }
        #endregion
    }
}
