using App.Core.Domain.Projects;
using App.Core.Domain.TimeSheets;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.dailyreport;
using App.Web.Areas.Admin.Models.TimeSheets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class DailyReportController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ITimeSheetModelFactory _timeSheetModelFactory;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IEmployeeService _employeeService;
         private readonly IProjectsService _projectService;
        private readonly IProjectTaskService _projectTaskService;

        #endregion

        #region Ctor

        public DailyReportController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ITimeSheetModelFactory timeSheetModelFactory,
            ITimeSheetsService timeSheetsService,
            IEmployeeService employeeService,
            IProjectsService projectService,
            IProjectTaskService projectTaskService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _timeSheetModelFactory = timeSheetModelFactory;
            _timeSheetsService = timeSheetsService;
            _employeeService = employeeService;
            _projectService = projectService;
            _projectTaskService = projectTaskService;
        }
        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(TimeSheetModel model)
        {
            model.Employee.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.reports.Employee.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task<TimeSheetModel> PrepareDailyreportModelAsync(TimeSheetModel model, TimeSheet timeshit, bool excludeProperties = false)
        {

            if (timeshit != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = timeshit.ToModel<TimeSheetModel>();
                }
            }
            var teamPerformanceMeasurements = await _timeSheetsService.GetAllreportAsync(0, int.MaxValue, false, null);

            var teamPerformanceMeasurement = teamPerformanceMeasurements
                                             .Where(x => x.EmployeeId == model.EmployeeId);
            foreach (var item in teamPerformanceMeasurements)
            {
                foreach (var mdl in model.report)
                {
                    mdl.EmployeeId = item.EmployeeId;
                    //mdl.EstimatedHours = item.EstimatedHours;
                    mdl.ProjectName = item.ProjectId.ToString();

                }
            }

            await PrepareEmployeeListAsync(model);
            return model;
        }

        public virtual async Task PrepareDailyreportreviewAsync(TimeSheetModel model)
        {
            var teamPerformanceMeasurements = await _timeSheetsService.GetAllreportAsync(0, int.MaxValue, false, null);
            if (model.reports.EmployeeId > 0)
            {
                //var teamPerformanceMeasurement = teamPerformanceMeasurements
                //                                 .Where(x => x.EmployeeId == model.EmployeeId);
               var teamPerformanceMeasurement = teamPerformanceMeasurements
    .Where(x =>
        x.EmployeeId == model.reports.EmployeeId &&
        x.UpdateOnUtc.Date >= model.reports.StartDate.Date &&
        x.UpdateOnUtc.Date <= model.reports.EndDate.Date
    )
    .ToList();
                foreach (var item in teamPerformanceMeasurement)
                {
                    Project project = new Project();
                    project = await _projectService.GetProjectsByIdAsync(item.ProjectId);
                    DailyReportModel reportModel = new DailyReportModel();
                    var  projectTask = await _projectTaskService.GetProjectTasksByIdAsync(item.TaskId);
                    reportModel.EmployeeId = item.EmployeeId;
                    reportModel.ProjectName = project.ProjectTitle;
                    reportModel.Task = projectTask.TaskTitle; //modify
                    reportModel.Date = item.UpdateOnUtc;
                    //reportModel.EstimatedHours = item.EstimatedHours;
                    if (item.Billable == true)
                    {
                        reportModel.ProductiveHours = item.SpentHours;
                    }
                    else
                    {
                        reportModel.RNDHours = item.SpentHours;
                    }

                    decimal totalHours = reportModel.EstimatedHours;
                    decimal productiveHours = reportModel.ProductiveHours;
                    decimal rndHours = reportModel.RNDHours;

                    decimal productivityRatio = 0;

                    reportModel.Total = reportModel.ProductiveHours + reportModel.RNDHours;
                    decimal total = reportModel.Total;
                    if (total > 0 && totalHours > 0)
                    {
                        // Using your provided formula
                        productivityRatio = Math.Round((totalHours / total) * 100, 2);
                    }
                    // Assign the calculated productivity ratio to the report model
                    reportModel.ProductivityRatio = productivityRatio;

                    // Round productivity ratio to two decimal places
                    reportModel.ProductivityRatio = Math.Round(productivityRatio, 2);


                    model.report.Add(reportModel);
                }
            }

        }

        #endregion

        #endregion

        public async Task<IActionResult> dailyReportCreate(int id, int employeeid, DateTime startdate, DateTime enddate)
        {

            TimeSheetModel model = new TimeSheetModel();
            TimeSheet timeshit = new TimeSheet();
            model.reports.EmployeeId = employeeid;
            model.reports.StartDate = startdate;
            model.reports.EndDate = enddate;

            await PrepareDailyreportModelAsync(model, timeshit);
            await PrepareDailyreportreviewAsync(model);

            return View("~/Areas/Admin/Views/Extension/DailyReport/dailyReportCreate.cshtml", model);

        }

    }
}