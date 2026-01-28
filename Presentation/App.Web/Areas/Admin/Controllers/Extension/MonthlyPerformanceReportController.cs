using App.Services.Security;
using Microsoft.AspNetCore.Mvc;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Areas.Admin.Models.Extension.MonthlyPerformanceReports;
using DocumentFormat.OpenXml.EMMA;
using App.Core.Domain.TimeSheets;
using App.Web.Models.Boards;
namespace App.Web.Areas.Admin.Controllers.Extension
{
    public partial class MonthlyPerformanceReport : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ITimeSheetModelFactory _timeSheetModelFactory;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProjectsService _projectsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectTaskModelFactory _projectTaskModelFactory;
        private readonly IEmployeeService _employeeService;
        private readonly IMonthlyPerformanceReportModelFactory _monthlyPerformanceReportModelFactory;
        #endregion

        #region Ctor

        public MonthlyPerformanceReport(IPermissionService permissionService,
            ITimeSheetModelFactory timeSheetModelFactory,
            ITimeSheetsService timeSheetsService,
            INotificationService notificationService,
            ILocalizationService localizationService, IProjectsService projectsService, IProjectTaskService projectTaskService,
            IProjectTaskModelFactory projectTaskModelFactory,
            IEmployeeService employeeService, IMonthlyPerformanceReportModelFactory monthlyPerformanceReportModelFactory
            )
        {
            _permissionService = permissionService;
            _timeSheetModelFactory = timeSheetModelFactory;
            _timeSheetsService = timeSheetsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _projectsService = projectsService;
            _projectTaskService = projectTaskService;
            _projectTaskModelFactory = projectTaskModelFactory;
            _employeeService = employeeService;
            _monthlyPerformanceReportModelFactory = monthlyPerformanceReportModelFactory;
        }

        #endregion
        public virtual async Task<IActionResult> Report()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();
            //prepare model
            var model = await _monthlyPerformanceReportModelFactory.PrepareTimeSheetSearchModelAsync(new MonthlyPerformanceReportSearchModel());

             
            return View("/Areas/Admin/Views/Extension/PerformanceReports/EmployeePerformanceReport.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Report(MonthlyPerformanceReportSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();
        
            //prepare model
            var model = await _monthlyPerformanceReportModelFactory.PrepareMonthlyPerformanceReportListModelAsync(searchModel);

            return Json(model);
        }

        //public async Task<IActionResult> GetSummary(int searchEmployeeId, DateTime? From, DateTime? To)
        //{

        //    var timeSheetReports = await _timeSheetsService.GetAllTimeSheetReportAsync(searchEmployeeId, From, To);

        //    var totalTask = timeSheetReports.Count;
        //    var totalDeliveredOnTime = 0;
        //    decimal totalEstimatedHours = 0;
        //    decimal totalSpentHours = 0;
        //    decimal extraTime = 0;

        //    foreach (var report in timeSheetReports)
        //    {
        //        var task = await _projectTaskService.GetProjectTasksByIdAsync(report.TaskId);
        //        if (task != null)
        //        {
        //            var spentTime = await _timeSheetsService.GetSpeantTimeByEmployeeAndTaskAsync(report.EmployeeId, report.TaskId);
        //            var estimatedHours = task.EstimatedTime;
        //            var allowedVariations = report.AllowedVariations;

        //            if (task.DeliveryOnTime)
        //            {
        //                totalDeliveredOnTime++;
        //            }

        //            totalEstimatedHours += estimatedHours;
        //            totalSpentHours += (decimal)spentTime; // Cast spentTime to decimal
        //            extraTime += (decimal)(spentTime - estimatedHours >= 0 ? spentTime - estimatedHours : 0); // Cast the result to decimal
        //        }
        //    }
        //    var resultPercentage = totalTask == 0 ? 0 : Math.Round((totalDeliveredOnTime / (double)totalTask) * 100, 2);

        //    return Json(new
        //    {
        //        TotalTask = totalTask,
        //        TotalDeliveredOnTime = totalDeliveredOnTime,
        //        ResultPercentage = resultPercentage,
        //        TotalEstimatedHours = totalEstimatedHours,
        //        TotalSpentHours = totalSpentHours,
        //        ExtraTime = extraTime
        //    });
        //}

        public async Task<IActionResult> ChangeDOT(int taskId)
        {
            var task = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
            if(task != null)
            {
                if(task.DeliveryOnTime == false)
                {
                    task.DeliveryOnTime = true;
                }
                else 
                {
                    task.DeliveryOnTime = false;
                }

                task.IsManualDOT = true;

                await _projectTaskService.UpdateProjectTaskAsync(task);
            }
            return Json("Success");
        }

    }
}
