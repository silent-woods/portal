
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Framework.Models.Extensions;
using App.Web.Models.Extensions.TimeSheets;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial class TimeSheetModelFactory : ITimeSheetModelFactory
    {

        #region Fields

        private readonly IProjectsService _projectService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        private readonly ITimeSheetsService _timeSheetsService;
        //private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        #endregion

        #region Ctor

        public TimeSheetModelFactory(IProjectsService projectsService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService,
            ITimeSheetsService timeSheetsService,
            //IBaseAdminModelFactory baseAdminModelFactory,
            IProjectTaskService projectTaskService,
            IProjectEmployeeMappingService projectEmployeeMappingService
            )
        {
            _projectService = projectsService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _timeSheetsService = timeSheetsService;
            //_baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
        }

        #endregion
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(TimeSheetModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.AvailableEmployees.Add(new SelectListItem
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

        public virtual async Task PrepareAssignedToEmployeeListAsync(TimeSheetModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var employeeIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);
            foreach (var id in employeeIds)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(id);
                if (employee != null)
                    model.AvailableAssignedToEmployees.Add(new SelectListItem
                    {
                        Text = employee.FirstName + " " + employee.LastName,
                        Value = employee.Id.ToString()
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
        
            var project = await _projectService.GetProjectListByEmployee(model.EmployeeId);
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
            var employees = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(model.EmployeeId);
            
         
            
            foreach (var p in employees)
            {
                var employee = await _employeeService.GetEmployeeByIdAsync(p);
                if(employee != null)
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = employee.FirstName + " " + employee.LastName,
                    Value = employee.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectListAsync(TimeSheetSearchModel model)
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
        public virtual async Task PrepareBillableTypeListAsync(TimeSheetSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

           

            model.AvailableBillableType.Add(new SelectListItem
            {
                Text = "All",
                Value = "0"
            });

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
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();
            return searchModel;
        }

        //public virtual async Task<TimeSheetModel> PrepareTimeSheetListModelAsync
        //  (TimeSheetSearchModel searchModel)
        //{
        //    if (searchModel == null)
        //        throw new ArgumentNullException(nameof(searchModel));

        //    //get timesheet
        //    var timeSheet = await _timeSheetsService.GetAllTimeSheetAsync(employeeIds: searchModel.EmployeeId, projectIds: searchModel.SelectedProjectIds,
        //        taskName: searchModel.TaskName, to: searchModel.To, from: searchModel.From,
        //        showHidden: true,
        //        pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
        //    //prepare grid model
        //    var model = await new TimeSheetListModel().PrepareToGridAsync(searchModel, timeSheet, () =>
        //    {
        //        return timeSheet.SelectAwait(async timesheet =>
        //        {
        //            //fill in model values from the entity

        //            var TimeSheetModel = timesheet.ToModel<TimeSheetModel>();
        //            TimeSheetModel.SpentDates = timesheet.SpentDate.ToString("MM/dd/yyyy");
        //            TimeSheetModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.CreateOnUtc, DateTimeKind.Utc);
        //            TimeSheetModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.UpdateOnUtc, DateTimeKind.Utc);
        //            if (TimeSheetModel.EmployeeId != 0 || TimeSheetModel.EmployeeId != null)
        //            {
        //                Employee emp = await _employeeService.GetEmployeeByIdAsync(TimeSheetModel.EmployeeId);
        //                if (emp != null)
        //                    TimeSheetModel.EmployeeName = emp.FirstName + " " + emp.LastName;
        //            }
        //            if (TimeSheetModel.ProjectId != 0)
        //            {
        //                Project project = await _projectService.GetProjectsByIdAsync(TimeSheetModel.ProjectId);
        //                if (project != null)
        //                    TimeSheetModel.ProjectName = project.ProjectTitle;
        //            }

        //            if (TimeSheetModel.TaskId != 0)
        //            {
        //                ProjectTask projectTask = await _projectTaskService.GetProjectTasksByIdAsync(TimeSheetModel.TaskId);
        //                if (projectTask != null)
        //                {
        //                    TimeSheetModel.TaskName = projectTask.TaskTitle;
        //                    TimeSheetModel.EstimatedHours = projectTask.EstimatedTime;
        //                }
        //            }
        //            return TimeSheetModel;
        //        }).Where(x => x != null);
        //    });
        //    //prepare grid model
        //    return model;
        //}
        public virtual async Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false)
        {
            //if (timeSheet != null)
            //{
            //    //fill in model values from the entity
            //    if (model == null)
            //    {
            //        model = timeSheet.ToModel<TimeSheetModel>();
            //        model.EmployeeId = timeSheet.EmployeeId;
            //        model.ProjectId = timeSheet.ProjectId;
            //        var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            //        if (employee != null)
            //        {
            //            model.EmployeeName = employee.FirstName + " " + employee.LastName;
            //        }

            //    }

            //    var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);

            //    if (emp != null)
            //    {
            //        model.SelectedEmployeeId.Add(emp.Id);
            //    }
            //}

            //await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableEmployees, false);
            //foreach (var employeeItem in model.AvailableEmployees)
            //{
            //    employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
            //        && model.SelectedEmployeeId.Contains(employeeId);
            //}
            await PrepareEmployeeListAsync(model);
            await PrepareAssignedToEmployeeListAsync(model);
            await PrepareProjectListAsync(model);
            return model;
        }
        #endregion
    }
}
