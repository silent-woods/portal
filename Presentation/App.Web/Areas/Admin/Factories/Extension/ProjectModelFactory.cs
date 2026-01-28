using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Projects;
using App.Core.Domain.Leaves;
using App.Core.Domain.Projects;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Projects;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the project model factory implementation
    /// </summary>
    public partial class ProjectModelFactory : IProjectModelFactory
    {
        #region Fields

        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly IProcessWorkflowService _processWorkflowService;
        #endregion

        #region Ctor

        public ProjectModelFactory(IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService
,
            IProcessWorkflowService processWorkflowService)
        {
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _processWorkflowService = processWorkflowService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(ProjectModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Projects.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
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


        public virtual async Task PrepareProcessWorkflowListAsync(ProjectModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
 
           
            var allProcessWorkflow = await _processWorkflowService.GetAllProcessWorkflowsAsync("");
            foreach (var p in allProcessWorkflow)
            {
                model.AvailableProcessWorkflows.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectLeaderListAsync(ProjectModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Projects.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
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
        #endregion

        #region Methods

        public virtual async Task<ProjectSearchModel> PrepareProjectsSearchModelAsync(ProjectSearchModel searchModel)
        {
            
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<ProjectListModel> PrepareProjectsListModelAsync
          (ProjectSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            
            //get project
            var project = await _projectService.GetAllProjectsAsync(projectName: searchModel.SearchProjectTitle,
                showHidden: false,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new ProjectListModel().PrepareToGridAsync(searchModel, project, () =>
            {
                return project.SelectAwait(async projects =>
                {
                    //fill in model values from the entity
                    var selectedAvailableOption = projects.StatusId;
                   
                    var projectModel = projects.ToModel<ProjectModel>();
                    projectModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(projects.CreateOnUtc, DateTimeKind.Utc);
                    projectModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(projects.UpdateOnUtc, DateTimeKind.Utc);

                    Employee emp = new Employee();

                    var projectManagerId = await _projectService.GetProjectManagerIdByIdAsync(projectModel.Id);

                    emp = await _employeeService.GetEmployeeByIdAsync(projectManagerId);
                    if(emp!= null)
                    projectModel.ProjectManagerName = emp.FirstName + " " + emp.LastName;

                    var projectleaderId = await _projectService.GetProjectLeaderIdByIdAsync(projectModel.Id);
                    emp = await _employeeService.GetEmployeeByIdAsync(projectleaderId);
                    if(emp != null)
                    projectModel.ProjectLeaderName = emp.FirstName + " " + emp.LastName;
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null)  projectModel.Status = ((ProjectStatusEnum)selectedAvailableOption).ToString();
                    
                   
                    return projectModel;
                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<ProjectModel> PrepareProjectsModelAsync(ProjectModel model, Project project, bool excludeProperties = false)
        {
            var projectStatus = await ProjectStatusEnum.New.ToSelectListAsync();
            if (project != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = project.ToModel<ProjectModel>();
                   
                }
             
            }
            model.ProjectStatus = projectStatus.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StatusId.ToString() == store.Value
            }).ToList();
            await PrepareEmployeeListAsync(model);
            await PrepareProcessWorkflowListAsync(model);
            //await PrepareProjectLeaderListAsync(model);
            return model;
        }
        #endregion
    }
}
