using App.Core;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Services.WeeklyQuestion;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.WeeklyQuestions;
using App.Web.Areas.Admin.Models.WeeklyReports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class WeeklyReportingController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWeeklyQuestionService _weeklyQuestionService;
        private readonly IProjectsService _projectsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IWeeklyUpdateService _weeklyUpdateService;
        private readonly IEmployeeService _employeeService;
        #endregion

        #region Ctor

        public WeeklyReportingController(IPermissionService permissionService,
            IWeeklyQuestionService weeklyQuestionService,
            IProjectsService projectsService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext
,
            IWeeklyUpdateService weeklyUpdateService,
            IEmployeeService employeeService = null)
        {
            _permissionService = permissionService;
            _weeklyQuestionService = weeklyQuestionService;
            _projectsService = projectsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _weeklyUpdateService = weeklyUpdateService;
            _employeeService = employeeService;
        }

        #endregion


        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(WeeklyReportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Employees.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var employeeName = "";
            var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employee)
            {
                model.Employees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task<WeeklyReportModel> PrepareDailreportlistAsync(WeeklyReportModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.DateWiseReports = new Dictionary<DateTime, List<WeeklyQuestionData>>();


            //var reportList = await _weeklyUpdateService.GetByreportIdAsync(model.EmployeeId);
            //var list = reportList.Where(x => x.EmployeeId == model.EmployeeId && x.CreatedOn.Date == model.CreatedOn.Date).ToList();
            var reportlist = await _weeklyUpdateService.GetAllWeeklyUpdateAsync(0, int.MaxValue, false, null);
            foreach (var item in reportlist)
            {
                var date = GetWeekOfYear(item.CreatedOn);
            }
            foreach (var trainee in reportlist)
            {
                var jsonString = trainee.Qdata;
                var marks = JsonConvert.DeserializeObject<List<string>>(jsonString);

                foreach (var mark in marks)
                {
                    var reportData = JsonConvert.DeserializeObject<List<WeeklyQuestionData>>(mark);

                    foreach (var item in reportData)
                    {
                        DateTime reportDate = trainee.CreatedOn.Date;
                        if (model.DateWiseReports.ContainsKey(reportDate))
                        {
                            model.DateWiseReports[reportDate].Add(item);
                        }
                        else
                        {
                            model.DateWiseReports[reportDate] = new List<WeeklyQuestionData> { item };
                        }
                    }
                }
            }

            return model;
        }


        public static int GetWeekOfYear(DateTime date)
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            return cultureInfo.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        }
        #endregion

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyReports, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            WeeklyReportModel model = new WeeklyReportModel();
            await PrepareEmployeeListAsync(model);


            return View("/Areas/Admin/Views/Extension/Weeklyreports/listCreate.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(WeeklyReportModel model, int employeeid)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyReports, PermissionAction.View))
                return AccessDeniedView();

            model.EmployeeId = employeeid;
            //prepare model

            await PrepareEmployeeListAsync(model);
            await PrepareDailreportlistAsync(model);
            return View("/Areas/Admin/Views/Extension/Weeklyreports/listCreate.cshtml", model);
        }
        public virtual async Task<IActionResult> WeeklyreportCreate()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyReports, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var questions = _weeklyQuestionService.GetAllWquestion();
            var questionViewModels = questions.Select(q => new WeeklyQuestionsModel
            {
                QuestionText = q.QuestionText,
                ControlTypeId = q.ControlTypeId,
                ControlValue = q.ControlValue,
                DesignationId = q.DesignationId,
                // Populate DropDownOptions if applicable
            }).ToList();

            var viewModel = new WeeklyReportModel
            {
                WeeklyQuestions = questionViewModels
            };
            await PrepareEmployeeListAsync(viewModel);

            return View("/Areas/Admin/Views/Extension/Weeklyreports/Create.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> WeeklyreportCreate(WeeklyReportModel weeklyReportModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyReports, PermissionAction.Add))
                return AccessDeniedView();
            if (ModelState.IsValid)
            {
                // Your existing logic to save the weekly report
                var weeklyupdate = weeklyReportModel.ToEntity<WeeklyReports>();
                weeklyupdate.Qdata = JsonConvert.SerializeObject(weeklyReportModel.ControlValue);
                await _weeklyUpdateService.InsertWeeklyUpdateAsync(weeklyupdate);
            }
            return View(weeklyReportModel);
        }
    }

}