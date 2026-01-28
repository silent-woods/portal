using App.Core;
using App.Core.Domain.Localization;
using App.Core.Domain.WeeklyQuestion;
using App.Core.Infrastructure;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Services.WeeklyQuestion;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.WeeklyQuestions;
using App.Web.Areas.Admin.WeeklyQuestion;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class WeeklyQuestionController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IWeeklyQuestionService _weeklyQuestionService;
        private readonly IWeeklyQuestionsModelFactory _weeklyQuestionsModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public WeeklyQuestionController(IPermissionService permissionService,
           INotificationService notificationService,
            ILocalizationService localizationService,
            IEmployeeService employeeService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            ILogger logger,
            IWeeklyQuestionService weeklyQuestionService,
            IWeeklyQuestionsModelFactory weeklyQuestionsModelFactory)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _employeeService = employeeService;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _logger = logger;
            _weeklyQuestionService = weeklyQuestionService;
            _weeklyQuestionsModelFactory = weeklyQuestionsModelFactory;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _weeklyQuestionsModelFactory.PrepareWeeklyQuestionsSearchModelAsync(new WeeklyQuestionsSearchModel());
            return View("/Areas/Admin/Views/Extension/WeeklyReportQuestion/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(WeeklyQuestionsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _weeklyQuestionsModelFactory.PrepareWeeklyQuestionsListModelAsync(searchModel);
            return Json(model);
        }
        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.Add))
                return AccessDeniedView();
            var model = await _weeklyQuestionsModelFactory.PrepareWeeklyQuestionsModelAsync(new WeeklyQuestionsModel(), null);

            return View("/Areas/Admin/Views/Extension/WeeklyReportQuestion/Create.cshtml", model);

        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(WeeklyQuestionsModel model, bool continueEditing, int downloadId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.Add))
                return AccessDeniedView();
            var weeklyQuestions = model.ToEntity<WeeklyQuestions>();

            if (ModelState.IsValid)
            {
                weeklyQuestions.CreatedOn = DateTime.UtcNow;
                await _weeklyQuestionService.InsertWeeklyQuestionsAsync(weeklyQuestions);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.WeeklyQuestions.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = weeklyQuestions.Id });
            }

            await _weeklyQuestionsModelFactory.PrepareWeeklyQuestionsModelAsync(model, weeklyQuestions);

            return View("/Areas/Admin/Views/Extension/WeeklyReportQuestion/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.Edit))
                return AccessDeniedView();

            var timeSheet = await _weeklyQuestionService.GetWeeklyQuestionsByIdAsync(id);
            if (timeSheet == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _weeklyQuestionsModelFactory.PrepareWeeklyQuestionsModelAsync(null, timeSheet);

            return View("/Areas/Admin/Views/Extension/WeeklyReportQuestion/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(WeeklyQuestionsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var timeSheet = await _weeklyQuestionService.GetWeeklyQuestionsByIdAsync(model.Id);
            if (timeSheet == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                timeSheet = model.ToEntity(timeSheet);
               
                await _weeklyQuestionService.UpdateWeeklyQuestionsAsync(timeSheet);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.WeeklyQuestions.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/WeeklyReportQuestion/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var timeSheet = await _weeklyQuestionService.GetWeeklyQuestionsByIdAsync(id);
            if (timeSheet == null)
                return RedirectToAction("List");

            await _weeklyQuestionService.DeleteWeeklyQuestionsAsync(timeSheet);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.WeeklyQuestions.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWeeklyQuestions, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _weeklyQuestionService.GetWeeklyQuestionsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _weeklyQuestionService.DeleteWeeklyQuestionsAsync(item);
            }
            return Json(new { Result = true });
        }

    }
}