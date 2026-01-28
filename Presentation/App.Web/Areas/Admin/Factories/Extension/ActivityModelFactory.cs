using App.Core.Domain.Employees;
using App.Core.Domain.Leaves;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Leaves;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.LeaveManagement;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Framework.Models.Extensions;
using System.Collections.Generic;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Core.Domain.Extension.ProjectTasks;

using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Web.Models.Boards;
using App.Web.Areas.Admin.Models.Extension.Activities;
using App.Services.Activities;
using App.Core.Domain.Activities;
using DocumentFormat.OpenXml.EMMA;
using App.Web.Areas.Admin.Models.Projects;
using App.Services.TimeSheets;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the leaveManagement model factory implementation
    /// </summary>
    public partial class ActivityModelFactory : IActivityModelFactory
    {
        #region Fields

        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectsService _projectService;
        private readonly IActivityService _activityService;
        private readonly ITimeSheetsService _timeSheetsService;
        #endregion

        #region Ctor

        public ActivityModelFactory(ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IBaseAdminModelFactory baseAdminModelFactory
,
            IProjectsService projectService,

            IProjectTaskService projectTaskService,
            IActivityService activityService,
            ITimeSheetsService timeSheetsService
            )
        {
            _leaveManagementService = leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _projectService = projectService;
            _activityService = activityService;
            _timeSheetsService = timeSheetsService;
        }

        #endregion

        #region Utilities

        private string FormatEnumValue(string enumValue)
        {
            // Check if the enum value contains underscores
            if (enumValue.Contains("_"))
            {
                var valueWithoutUnderscores = enumValue.Replace("_", string.Empty);

                // Remove any spaces that are followed by a capital letter
                var result = System.Text.RegularExpressions.Regex.Replace(valueWithoutUnderscores, "(?<=\\w) (?=[A-Z])", string.Empty);

                return result;
            }
            else
            {
                // Insert spaces before capital letters (ignoring the first character)
                return System.Text.RegularExpressions.Regex.Replace(enumValue, "(?<!^)(?=[A-Z])", " ");
            }
        }


        public virtual async Task PrepareProjectListAsync(ActivityModel model)
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
                    model.AvailableProjects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
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
        public virtual async Task PrepareEmployeeListAsync(ActivityModel model)
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

        public virtual async Task PrepareEmployeeListAsync(ActivitySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableEmployee.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                searchModel.AvailableEmployee.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectListAsync(ActivitySearchModel searchmodel)
        {
            if (searchmodel == null)
                throw new ArgumentNullException(nameof(searchmodel));

            searchmodel.AvailableProject.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var projectTaskName = "";
            var leaves = await _projectService.GetAllProjectsAsync(projectTaskName);
            foreach (var p in leaves)
            {
                if (p.StatusId != 4)
                {
                    searchmodel.AvailableProject.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        

        #endregion
        #region Methods

        public virtual async Task<ActivitySearchModel> PrepareActivitySearchModelAsync(ActivitySearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            await PrepareProjectListAsync(searchModel);
            await PrepareEmployeeListAsync(searchModel);

            return searchModel;
        }


        public virtual async Task<ActivityListModel> PrepareActivityListModelAsync(ActivitySearchModel searchModel)
        {

            var activities = await _activityService.GetAllActivitiesAsync(activityName: searchModel.SearchActivityName, searchModel.SearchEmployeeId,searchModel.SearchProjectId,searchModel.SearchTaskTitle,
                 pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                showHidden: true);

            //prepare grid model
            var model = await new ActivityListModel().PrepareToGridAsync(searchModel, activities, () =>
            {
                return activities.SelectAwait(async activityEntity =>
                {
     
                    
                    var activityModel = activityEntity.ToModel<ActivityModel>();
                   
                    Activity activity = await _activityService.GetActivityByIdAsync(activityEntity.Id);
                  
                    Employee employee = await _employeeService.GetEmployeeByIdAsync(activity.EmployeeId);
                    if (activity == null)
                        return null;

                    activityModel.TaskId = activity.TaskId;
                    activityModel.EmployeeId = activity.EmployeeId;
                    activityModel.ActivityName = activity.ActivityName;
                    activityModel.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(activityEntity.SpentHours, activityEntity.SpentMinutes);

                    activityModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(activity.CreateOnUtc, DateTimeKind.Utc);
                    activityModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(activity.UpdateOnUtc, DateTimeKind.Utc);
                    activityModel.SpentHours = activity.SpentHours;

                    if (employee != null)
                        activityModel.EmployeeName = employee.FirstName + " " + employee.LastName;

                    ProjectTask task = await _projectTaskService.GetProjectTasksByIdAsync(activity.TaskId);
                    if (task != null) {
                        activityModel.TaskTitle = task.TaskTitle;

                        var project = await _projectTaskService.GetProjectByTaskIdAsync(task.Id);
                        activityModel.ProjectName = project.ProjectTitle;
                        }
                    return activityModel;
                }).Where(x => x != null);
            });

            await PrepareProjectListAsync(searchModel);
            //prepare grid model
            return model;
        }
        public virtual async Task<ActivityModel> PrepareActivityModelAsync(ActivityModel model, Activity activity, bool excludeProperties = false)
        {
         

            if (activity != null)
            {
                // Fill in model values from the entity
                if (model == null)
                {
                    model = activity.ToModel<ActivityModel>();
                    model.CreatedOnUtc = activity.CreateOnUtc;
                    model.ActivityName = activity.ActivityName;
                    model.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(activity.SpentHours, activity.SpentMinutes);
                    var project = await _projectTaskService.GetProjectByTaskIdAsync(model.TaskId);
                    if(project != null)
                    {
                        model.ProjectId = project.Id;
                        model.ProjectName = project.ProjectTitle;
                    }
                }

            }

            
            await PrepareEmployeeListAsync(model);
            await PrepareProjectListAsync(model);
          


            return model;
        }

     

        #endregion
    }
}


