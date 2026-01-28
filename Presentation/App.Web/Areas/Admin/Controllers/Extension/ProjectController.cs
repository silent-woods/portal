using App.Core;
using App.Core.Domain.Projects;
using App.Core.Domain.Security;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{       
    public partial class ProjectController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IProjectModelFactory _projectModelFactory;
        private readonly IProjectsService _projectsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public ProjectController(IPermissionService permissionService,
            IProjectModelFactory projectModelFactory,
            IProjectsService projectsService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IDateTimeHelper dateTimeHelper
            )
        {
            _permissionService = permissionService;
            _projectModelFactory = projectModelFactory;
            _projectsService = projectsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _projectModelFactory.PrepareProjectsSearchModelAsync(new ProjectSearchModel());
            return View("/Areas/Admin/Views/Extension/Projects/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProjectSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _projectModelFactory.PrepareProjectsListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _projectModelFactory.PrepareProjectsModelAsync(new ProjectModel(), null);

            return View("/Areas/Admin/Views/Extension/Projects/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProjectModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.Add))
                return AccessDeniedView();

            var project = model.ToEntity<Project>();

            if (ModelState.IsValid)
            {
                project.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();
                project.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                if (model.SelectedProcessWorkflowIds != null && model.SelectedProcessWorkflowIds.Any())
                {
                    project.ProcessWorkflowIds = string.Join(",", model.SelectedProcessWorkflowIds.Select(d => d.ToString()));
                }

                await _projectsService.InsertProjectsAsync(project);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.project.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = project.Id });
            }

            //prepare model
            model = await _projectModelFactory.PrepareProjectsModelAsync(model, project, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Projects/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.Edit))
                return AccessDeniedView();

            var project = await _projectsService.GetProjectsByIdAsync(id);
            if (project == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _projectModelFactory.PrepareProjectsModelAsync(null, project);

            if (project.ProcessWorkflowIds != null && project.ProcessWorkflowIds != "")
                model.SelectedProcessWorkflowIds = project.ProcessWorkflowIds.Split(',').Select(int.Parse).ToList();

            return View("/Areas/Admin/Views/Extension/Projects/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProjectModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var project = await _projectsService.GetProjectsByIdAsync(model.Id);

            if (project == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                project = model.ToEntity(project);
                project.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                if (model.SelectedProcessWorkflowIds != null && model.SelectedProcessWorkflowIds.Any())
                    project.ProcessWorkflowIds = string.Join(",", model.SelectedProcessWorkflowIds.Select(d => d.ToString()));

                await _projectsService.UpdateProjectsAsync(project);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.projects.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = project.Id });
            }

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Projects/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var project = await _projectsService.GetProjectsByIdAsync(id);
            if (project == null)
                return RedirectToAction("List");

            await _projectsService.DeleteProjectsAsync(project);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.projects.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProject, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _projectsService.GetProjectsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
                await _projectsService.DeleteProjectsAsync(item);
            return Json(new { Result = true });
        }

        #endregion
    }
}