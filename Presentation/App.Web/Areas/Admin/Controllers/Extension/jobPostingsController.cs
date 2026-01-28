using App.Core.Domain.JobPostings;
using App.Core.Domain.Security;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.JobPostings;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class jobPostingsController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IJobPostingsModelFactory _jobPostingsModelFactory;
        private readonly IJobPostingService _jobPostingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public jobPostingsController(IPermissionService permissionService,
           IJobPostingsModelFactory jobPostingsModelFactory,
        IJobPostingService jobPostingService,
        INotificationService notificationService,
            ILocalizationService localizationService
            )
        {
            _permissionService = permissionService;
            _jobPostingsModelFactory = jobPostingsModelFactory;
            _jobPostingService = jobPostingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsSearchModelAsync(new JobPostingSearchModel());
            return View("/Areas/Admin/Views/Extension/jobPosting/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(JobPostingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(new JobPostingModel(), null);

            return View("/Areas/Admin/Views/Extension/JobPosting/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(JobPostingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Add))
                return AccessDeniedView();

            var timeSheet = model.ToEntity<JobPosting>();
            model.AvailablePosition.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.Position.select"),
                Value = "0"
            });

            if (ModelState.IsValid)
            {
                timeSheet.CreatedOn = DateTime.UtcNow;
                timeSheet.UpdatedOn = DateTime.UtcNow;

                await _jobPostingService.InsertJobPostingAsync(timeSheet);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.JobPosting.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }
            //prepare model
            model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(model, timeSheet, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/JobPosting/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Edit))
                return AccessDeniedView();

            var jobposting = await _jobPostingService.GetJobPostingByIdAsync(id);
            if (jobposting == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(null, jobposting);

            return View("/Areas/Admin/Views/Extension/JobPosting/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(JobPostingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(model.Id);
            if (jobPosting == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                jobPosting = model.ToEntity(jobPosting);
                jobPosting.UpdatedOn = DateTime.UtcNow;

                await _jobPostingService.UpdateJobPostingAsync(jobPosting);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.JobPosting.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = jobPosting.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/JobPosting/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(id);
            if (jobPosting == null)
                return RedirectToAction("List");

            await _jobPostingService.DeleteJobPostingAsync(jobPosting);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.JobPosting.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _jobPostingService.GetJobPostingByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _jobPostingService.DeleteJobPostingAsync(item);
            }
            return Json(new { Result = true });
        }
    }
}