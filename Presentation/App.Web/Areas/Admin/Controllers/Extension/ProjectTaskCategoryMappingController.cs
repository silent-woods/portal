using App.Core;
using App.Core.Domain.Security;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;

using App.Services.Security;

using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class ProjectTaskCategoryMappingController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IProjectTaskCategoryMappingModelFactory _projectTaskCategoryMappingModelFactory;
        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IProjectsService _projectsService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDateTimeHelper _dateTimeHelper;
       

        #endregion

        #region Ctor

        public ProjectTaskCategoryMappingController(IPermissionService permissionService,
            IProjectTaskCategoryMappingModelFactory projectTaskCategoryMappingModelFactory,
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IProjectsService projectsService,
            ITaskCategoryService taskCategoryService,
            IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper)
        {
            _permissionService = permissionService;
            _projectTaskCategoryMappingModelFactory = projectTaskCategoryMappingModelFactory;
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _projectsService = projectsService;
            _taskCategoryService = taskCategoryService;
            _workflowMessageService = workflowMessageService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.View))
                return AccessDeniedView();

            var model = await _projectTaskCategoryMappingModelFactory
                .PrepareProjectTaskCategoryMappingSearchModelAsync(new ProjectTaskCategoryMappingSearchModel());

            return View("/Areas/Admin/Views/Extension/Projects/_CreateOrUpdateProjectTaskCategoryMapping.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProjectTaskCategoryMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.View))
                return AccessDeniedView();

            var model = await _projectTaskCategoryMappingModelFactory
                .PrepareProjectTaskCategoryMappingListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create(int projectId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.Add))
                return AccessDeniedView();

            var model = await _projectTaskCategoryMappingModelFactory
                .PrepareProjectTaskCategoryMappingModelAsync(new ProjectTaskCategoryMappingModel(), null);

            model.ProjectId = projectId;
            var project = await _projectsService.GetProjectsByIdAsync(projectId);
            if (project != null)
                model.ProjectName = project.ProjectTitle;

            model.IsActive = true;
            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/Projects/ProjectTaskCategoryMappingCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProjectTaskCategoryMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.Add))
                return AccessDeniedView();

            var entity = model.ToEntity<ProjectTaskCategoryMapping>();

            // optional: duplicate check (avoid same category twice in same project)
            if (await _projectTaskCategoryMappingService.IsCategoryExistAsync(model.ProjectId, model.TaskCategoryId))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.TaskCategoryExist"));
                model = await _projectTaskCategoryMappingModelFactory.PrepareProjectTaskCategoryMappingModelAsync(model, entity, true);
                ViewBag.RefreshPage = false;
                return View("/Areas/Admin/Views/Extension/Projects/ProjectTaskCategoryMappingCreate.cshtml", model);
            }

            if (ModelState.IsValid)
            {
             

                await _projectTaskCategoryMappingService.InsertMappingAsync(entity);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.ProjectTaskCategoryMapping.Added");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;
            }

            return View("/Areas/Admin/Views/Extension/Projects/ProjectTaskCategoryMappingCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int projectId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _projectTaskCategoryMappingService.GetMappingByIdAsync(id);
            ViewBag.RefreshPage = false;
            if (entity == null)
                return RedirectToAction("List");

            var model = await _projectTaskCategoryMappingModelFactory.PrepareProjectTaskCategoryMappingModelAsync(null, entity);
            model.ProjectId = projectId;
            var project = await _projectsService.GetProjectsByIdAsync(projectId);
            if (project != null)
                model.ProjectName = project.ProjectTitle;

            return View("/Areas/Admin/Views/Extension/Projects/ProjectTaskCategoryMappingEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProjectTaskCategoryMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _projectTaskCategoryMappingService.GetMappingByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");

            // duplicate check
            if (await _projectTaskCategoryMappingService.IsCategoryExistAsync(model.ProjectId, model.TaskCategoryId) && entity.TaskCategoryId != model.TaskCategoryId)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.TaskCategoryExist"));
                model = await _projectTaskCategoryMappingModelFactory.PrepareProjectTaskCategoryMappingModelAsync(model, entity, true);
                ViewBag.RefreshPage = false;
                return View("/Areas/Admin/Views/Extension/Projects/ProjectTaskCategoryMappingEdit.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                entity = model.ToEntity(entity);
                await _projectTaskCategoryMappingService.UpdateMappingAsync(entity);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.ProjectTaskCategoryMapping.Updated");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;
            }

            return View("/Areas/Admin/Views/Extension/Projects/ProjectTaskCategoryMappingEdit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.Delete))
                return AccessDeniedView();

            var entity = await _projectTaskCategoryMappingService.GetMappingByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            await _projectTaskCategoryMappingService.DeleteMappingAsync(entity);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectTaskCategoryMapping, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _projectTaskCategoryMappingService.GetMappingsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _projectTaskCategoryMappingService.DeleteMappingAsync(item);
            }

            return Json(new { Result = true });
        }

        [HttpGet]
        public async Task<IActionResult> CopyMappings(int targetProjectId)
        {
            var model = new ProjectTaskCategoryMappingModel
            {
                TargetProjectId = targetProjectId
            };

            // Load all projects for dropdown except target itself
            var projects = await _projectsService.GetAllProjectsAsync("");
            model.AvailableProjects = projects
                .Where(p => p.Id != targetProjectId)
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                }).ToList();

            return View("/Areas/Admin/Views/Extension/Projects/CopyMappings.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CopyMappings(ProjectTaskCategoryMappingModel model)
        {
            if (model.SourceProjectId <= 0 || model.TargetProjectId <= 0)
            {
                return RedirectToAction("Edit", "Project", new { id = model.TargetProjectId });
            }

            // Get source mappings
            var sourceMappings = await _projectTaskCategoryMappingService
                .GetAllMappingsByProjectIdAsync(model.SourceProjectId);

            // Get existing mappings for the target project
            var existingMappings = await _projectTaskCategoryMappingService
                .GetAllMappingsByProjectIdAsync(model.TargetProjectId);

            var existingCategoryIds = existingMappings
                .Select(x => x.TaskCategoryId)
                .ToHashSet();

            // Copy only non-existing mappings
            foreach (var mapping in sourceMappings)
            {
                if (!existingCategoryIds.Contains(mapping.TaskCategoryId))
                {
                    var newMapping = new ProjectTaskCategoryMapping
                    {
                        ProjectId = model.TargetProjectId,
                        TaskCategoryId = mapping.TaskCategoryId,
                        OrderBy = mapping.OrderBy,
                        IsActive = mapping.IsActive
                    };

                    await _projectTaskCategoryMappingService.InsertMappingAsync(newMapping);
                }
            }

            // Close popup and refresh parent grid
            ViewBag.RefreshPage = true;
            return View("/Areas/Admin/Views/Extension/Projects/CopyMappings.cshtml", model);
        }

        #endregion
    }
}
