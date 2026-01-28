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
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2019.Drawing.Model3D;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory implementation
    /// </summary>
    public partial class ProjectEmployeeMappingModelFactory : IProjectEmployeeMappingModelFactory
    {
        #region Fields

        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly IDesignationService _designationService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        #endregion

        #region Ctor

        public ProjectEmployeeMappingModelFactory(IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            IDesignationService designationService,
            IBaseAdminModelFactory baseAdminModelFactory
            )
        {
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _designationService = designationService;
            _baseAdminModelFactory = baseAdminModelFactory;
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

        public virtual async Task<ProjectEmployeeMappingSearchModel> PrepareProjectEmployeeMappingSearchModelAsync(ProjectEmployeeMappingSearchModel searchModel)
        {
          
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<ProjectEmployeeMappingListModel> PrepareProjectEmployeeMappingListModelAsync
          (ProjectEmployeeMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get project
            var project = await _projectEmployeeMappingService.GetAllProjectsEmployeeMappingAsync(projectempName: searchModel.SearchProjectemp,projectid:searchModel.ProjectId,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new ProjectEmployeeMappingListModel().PrepareToGridAsync(searchModel, project, () =>
            {
                return project.SelectAwait(async projects =>
                {
                    //fill in model values from the entity

                    var projectModel = projects.ToModel<ProjectEmployeeMappingModel>();
                    projectModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(projects.CreateOnUtc, DateTimeKind.Utc);

                    Project project = new Project();
                    project = await _projectService.GetProjectsByIdAsync(projectModel.ProjectId);
                    if(project !=null)
                    projectModel.ProjectName = project.ProjectTitle;


                    Designation designation = new Designation();
                    designation = await _designationService.GetDesignationByIdAsync(projectModel.RoleId);
                    if(designation != null)
                    projectModel.Role = designation.Name;
                  
                    Employee emp = new Employee();
                   
                    emp = await _employeeService.GetEmployeeByIdAsync(projectModel.EmployeeId);
                    if (emp != null)
                        projectModel.EmployeeName = emp.FirstName + " " + emp.LastName;
                    return projectModel;

                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<ProjectEmployeeMappingModel> PrepareProjectEmployeeMappingModelAsync(ProjectEmployeeMappingModel model, ProjectEmployeeMapping projectEmployeeMapping, bool excludeProperties = false)
        {
            if (projectEmployeeMapping != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = projectEmployeeMapping.ToModel<ProjectEmployeeMappingModel>();

                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }
                }
                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);

              
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
            }

            await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableEmployees, false);
            foreach (var employeeItem in model.AvailableEmployees)
            {
                employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
                    && model.SelectedEmployeeId.Contains(employeeId);
            }

            await PrepareProjectListAsync(model);
          
            await PrepareRoleListAsync(model);
            return model;
        }
        #endregion
    }
}
