using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data.Extensions;
using App.Services;
using App.Services.Activities;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the timesheet model factory implementation
    /// </summary>
    public partial class TimeSheetModelFactory : ITimeSheetModelFactory
    {
        #region Fields

        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IActivityService _activityService;
     
        #endregion

        #region Ctor

        public TimeSheetModelFactory(IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            ITimeSheetsService timeSheetsService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IProjectTaskService projectTaskService,
            IActivityService activityService
            )
        {
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _timeSheetsService = timeSheetsService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _activityService = activityService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(TimeSheetModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Employee.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.Employee.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectListAsync(TimeSheetModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            //model.Projects.Add(new SelectListItem
            //{
            //    Text = "Select",
            //    Value = null
            //});
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

        public virtual async Task PrepareEmployeeListAsync(TimeSheetSearchModel model)
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
        public virtual async Task PrepareProjectListAsync(TimeSheetSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
        
            var projectName = "";
            var project = await _projectService.GetAllProjectsAsync(projectName);
            foreach (var p in project)
            {
                model.AvailableProjects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
        }


        public virtual async Task PrepareBillableTypeListAsync(TimeSheetSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

          
          
                model.AvailableBillableType.Add(new SelectListItem
                {
                    Text = "All",
                    Value = "0"
                }) ;

            model.AvailableBillableType.Add(new SelectListItem
            {
                Text = "Yes",
                Value = "1"
            });
            model.AvailableBillableType.Add(new SelectListItem
            {
                Text = "No",
                Value = "2"
            });



        }
        #endregion

        #region Methods

        public virtual async Task<TimeSheetSearchModel> PrepareTimeSheetSearchModelAsync(TimeSheetSearchModel searchModel)
        {
            await PrepareEmployeeListAsync(searchModel);
            await PrepareProjectListAsync(searchModel);
            await PrepareBillableTypeListAsync(searchModel);
            searchModel.SetGridPageSize();
            searchModel.From = await _dateTimeHelper.GetUTCAsync();
            searchModel.To = await _dateTimeHelper.GetUTCAsync();

            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();


            return searchModel;
        }

        public virtual async Task<TimeSheetListModel> PrepareTimeSheetListModelAsync
          (TimeSheetSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get timesheet
            var timeSheet = await _timeSheetsService.GetAllTimeSheetAsync(employeeIds: searchModel.SelectedEmployeeIds,projectIds:searchModel.SelectedProjectIds,
                taskName:searchModel.TaskName,to:searchModel.To,from:searchModel.From,SelectedBillable: searchModel.BillableType,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new TimeSheetListModel().PrepareToGridAsync(searchModel, timeSheet, () =>
            {
                return timeSheet.SelectAwait(async timesheet =>
                {
                    //fill in model values from the entity
                   
                    var TimeSheetModel = timesheet.ToModel<TimeSheetModel>();
                    TimeSheetModel.SpentDates = timesheet.SpentDate.ToString("MM/dd/yyyy");
                    TimeSheetModel.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(timesheet.SpentHours, timesheet.SpentMinutes);      
                   
                    TimeSheetModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.CreateOnUtc, DateTimeKind.Utc);
                    TimeSheetModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.UpdateOnUtc, DateTimeKind.Utc);
                    if (TimeSheetModel.EmployeeId != 0 || TimeSheetModel.EmployeeId != null)
                    {
                        Employee emp = await _employeeService.GetEmployeeByIdAsync(TimeSheetModel.EmployeeId);
                        if(emp != null)
                        TimeSheetModel.EmployeeName = emp.FirstName + " " + emp.LastName;
                    }
                    if (TimeSheetModel.ProjectId != 0)
                    {
                        Project project = await _projectService.GetProjectsByIdAsync(TimeSheetModel.ProjectId);
                        if (project != null)
                        TimeSheetModel.ProjectName = project.ProjectTitle;
                    }

                    if (TimeSheetModel.TaskId != 0)
                    {
                        ProjectTask projectTask = await _projectTaskService.GetProjectTasksByIdAsync(TimeSheetModel.TaskId);
                        if (projectTask != null)
                        {
                            TimeSheetModel.TaskName = projectTask.TaskTitle;
                            TimeSheetModel.EstimatedTimeHHMM = await  _timeSheetsService.ConvertToHHMMFormat(projectTask.EstimatedTime);
                        }
                    }
                    if (TimeSheetModel.ActivityId != 0)
                    {
                        var activity = await _activityService.GetActivityByIdAsync(TimeSheetModel.ActivityId);
                        if(activity !=null)
                        TimeSheetModel.ActivityName = activity.ActivityName;
                    }

                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");


                    if (timesheet.StartTime != null)
                        TimeSheetModel.StartTime = TimeZoneInfo.ConvertTimeFromUtc(timesheet.StartTime.Value, istTimeZone);

                    if (timesheet.EndTime != null)
                        TimeSheetModel.EndTime = TimeZoneInfo.ConvertTimeFromUtc(timesheet.EndTime.Value, istTimeZone);

                    if (timesheet.CreateOnUtc != null)
                        TimeSheetModel.CreateOn = TimeZoneInfo.ConvertTimeFromUtc(timesheet.CreateOnUtc, istTimeZone);

                    if (timesheet.UpdateOnUtc != null)
                        TimeSheetModel.UpdateOn = TimeZoneInfo.ConvertTimeFromUtc(timesheet.UpdateOnUtc, istTimeZone);



                    return TimeSheetModel;
                }).Where(x => x != null);
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false)
        {
            if (timeSheet != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = timeSheet.ToModel<TimeSheetModel>();
                    model.EmployeeId = timeSheet.EmployeeId;
                    model.ProjectId =timeSheet.ProjectId;
                   
                    model.SpentTime = await _timeSheetsService.ConvertSpentTimeAsync(timeSheet.SpentHours, timeSheet.SpentMinutes);
                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }

                    var task = await _projectTaskService.GetProjectTasksByIdAsync(timeSheet.TaskId);
                    if (task != null)
                    {
                        model.EstimatedHours = task.EstimatedTime;
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
            return model;
        }
        #endregion
    }
}
