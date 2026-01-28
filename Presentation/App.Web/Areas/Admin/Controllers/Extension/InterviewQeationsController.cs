using App.Core.Domain.Employees;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.InterviewQeations;
using App.Web.Areas.Admin.InterviewQeations.Models;
using App.Web.Framework;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Services.Recruitements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class InterviewQeationsController : BaseAdminController
    #region Fields
    {

        private readonly IRecruitementService _recruitementservice;
        private readonly IPermissionService _permissionService;
        private readonly IQuestionsModelFactory _recruitementModelFactory;
        private readonly INotificationService _notificationService;
        private readonly IDownloadService _downloadService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public InterviewQeationsController(

            IPermissionService permissionService,
            IRecruitementService recruitementservice,
            IQuestionsModelFactory recruitementModelFactory,
            IDownloadService downloadService,
            INotificationService notificationService,
            ILocalizationService localizationService)
        {

            _recruitementservice = recruitementservice;
            _permissionService = permissionService;
            _recruitementModelFactory = recruitementModelFactory;

            _downloadService = downloadService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities


        #endregion

        #region Methods


        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.View))
                return AccessDeniedView();
          var model = await _recruitementModelFactory.PreparerecruitementSearchModelAsync(new RecruitementSearchModel());

            return View("/Areas/Admin/Views/Extension/InterviewQeations/List.cshtml", model);

        }

        [HttpPost]
        public virtual async Task<IActionResult> List(RecruitementSearchModel searchmodel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.View))
                return await AccessDeniedDataTablesJson();
            var model = await _recruitementModelFactory.PreparerecruitementListModelAsync(searchmodel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.Add))
                return AccessDeniedView();
            var model = await _recruitementModelFactory.PrepareRecruitementModelAsync(new RecruitementModel(), null);

            return View("/Areas/Admin/Views/Extension/InterviewQeations/Create.cshtml", model);

        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(RecruitementModel model, bool continueEditing, int downloadId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.Add))
                return AccessDeniedView();
            var timeSheet = model.ToEntity<Questions>();

            if (ModelState.IsValid)
            {
                timeSheet.CreatedOn = DateTime.UtcNow;
                timeSheet.UpdatedOn = DateTime.UtcNow;
                await _recruitementservice.InsertrecAsync(timeSheet);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.InterviewQuestion.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }

            await _recruitementModelFactory.PrepareRecruitementModelAsync(model, timeSheet);

            return View("/Areas/Admin/Views/Extension/InterviewQeations/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.Edit))
                return AccessDeniedView();
            var rts = await _recruitementservice.GetrecruitementByIdAsync(id);
            if (rts == null)

                //no record found with the specified id
                return RedirectToAction("List");

            var model = await _recruitementModelFactory.PrepareRecruitementModelAsync(null, rts);

            return View("/Areas/Admin/Views/Extension/InterviewQeations/Edit.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(RecruitementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.Edit))
                return AccessDeniedView();
            var rts = await _recruitementservice.GetrecruitementByIdAsync(model.Id);
            if (rts == null)

                //no record found with the specified id
                return RedirectToAction("List");

            rts.Id = model.Id;
            rts.Question = model.Question;
            rts.Question_answers = model.Question_answers;
            rts.CategoryId = model.CategoryId;
            rts.QuestionTypeId = model.questiontypeId;
            rts.DownloadId = model.DownloadId;
            rts.QuestionLevelId = model.QuestionLevelId;
            rts.DisplayOrder = model.DisplayOrder;
            rts.Published = model.Published;
         

            if (ModelState.IsValid)
            {
                rts.UpdatedOn = DateTime.UtcNow;
                await _recruitementservice.UpdateAsync(rts);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.InterviewQuestion.Updated"));
            }

            if (!continueEditing)
            {
                return RedirectToAction("List");
            }

            await _recruitementModelFactory.PrepareRecruitementModelAsync(model, rts);

            return View("/Areas/Admin/Views/Extension/InterviewQeations/Edit.cshtml", model);

        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.Delete))
                return AccessDeniedView();
            var rts = await _recruitementservice.GetrecruitementByIdAsync(id);
            if (rts == null)
                return RedirectToAction("List");

            await _recruitementservice.DeleteAsync(rts);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.InterviewQuestion.Deleted"));

            return RedirectToAction("List");
        }


       
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageQuestion, PermissionAction.Delete))
                return AccessDeniedView();
            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var rts = await _recruitementservice.GetrecruitementIdsAsync(selectedIds.ToArray());

            await _recruitementservice.DeleterecruitementAsync(rts);

            return Json(new { Result = true });

        }
        #endregion
    }
}


