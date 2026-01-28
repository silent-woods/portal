using App.Core;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.Localization;
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
using App.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Web.Controllers
{
    public partial class WeeklyReportingController : BasePublicController
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
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
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
            IEmployeeService employeeService = null,
            IWorkflowMessageService workflowMessageService = null,
            LocalizationSettings localizationSettings = null)
        {
            _permissionService = permissionService;
            _weeklyQuestionService = weeklyQuestionService;
            _projectsService = projectsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _weeklyUpdateService = weeklyUpdateService;
            _employeeService = employeeService;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        #endregion
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

            var reportlist = await _weeklyUpdateService.GetAllWeeklyUpdateAsync(0, int.MaxValue, false, null);
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

        public async Task<IActionResult> List()
        {
            // Prepare model
            WeeklyReportModel model = new WeeklyReportModel();
            await PrepareEmployeeListAsync(model);
            await PrepareDailreportlistAsync(model);
            return View("/Themes/DefaultClean/Views/Extension/Weeklyreports/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(WeeklyReportModel model, int employeeid)
        {
            model.EmployeeId = employeeid;
            //prepare model

           
            return View("/Themes/DefaultClean/Views/Extension/Weeklyreports/List.cshtml", model);
        }
        public virtual async Task<IActionResult> WeeklyreportCreate()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyReports))
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

            return View("/Themes/DefaultClean/Views/Extension/Weeklyreports/WeeklyreportCreate.cshtml", viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> WeeklyreportCreate(WeeklyReportModel weeklyReportModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyReports))
                return AccessDeniedView();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (ModelState.IsValid)
            {
                // Your existing logic to save the weekly report
                var weeklyupdate = weeklyReportModel.ToEntity<WeeklyReports>();
                weeklyupdate.Qdata = JsonConvert.SerializeObject(weeklyReportModel.ControlValue);
                weeklyupdate.EmployeeId = customer.Id;
                await _weeklyUpdateService.InsertWeeklyUpdateAsync(weeklyupdate);
                await _workflowMessageService.SendWeeklyUpdatetoHrMessageAsync(weeklyupdate, _localizationSettings.DefaultAdminLanguageId);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Weeklyupdate hr mail send"));
                return RedirectToRoute("Homepage");
            }
            return View(weeklyReportModel);
        }

    }

}