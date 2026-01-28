using App.Core;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Security;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class ProjectEmployeeMappingController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IProjectEmployeeMappingModelFactory _projectEmployeeMappingModelFactory;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IProjectsService _projectsService;
        private readonly IDesignationService _designationService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
        #endregion

        #region Ctor

        public ProjectEmployeeMappingController(IPermissionService permissionService,
            IProjectEmployeeMappingModelFactory projectEmployeeMappingModelFactory,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IProjectsService projectsService,
            IDesignationService designationService,
            IWorkflowMessageService workflowMessageService,
            IDateTimeHelper dateTimeHelper,
            IEmployeeService employeeService
            )
        {
            _permissionService = permissionService;
            _projectEmployeeMappingModelFactory = projectEmployeeMappingModelFactory;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _projectsService = projectsService;
            _designationService = designationService;
            _workflowMessageService = workflowMessageService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingSearchModelAsync(new ProjectEmployeeMappingSearchModel());

            return View("/Areas/Admin/Views/Extension/Projects/_CreateOrUpdateProjectEmpMapping.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProjectEmployeeMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.View))
                return AccessDeniedView();


            //prepare model
            var model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create(int projectId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(new ProjectEmployeeMappingModel(), null);
            model.ProjectId = projectId;
            var project = await _projectsService.GetProjectsByIdAsync(projectId);
            if (project != null)
                model.ProjectName = project.ProjectTitle;
            model.IsActive = true;
            ViewBag.RefreshPage = false;

            return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProjectEmployeeMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;

            var projectEmpMapping = model.ToEntity<ProjectEmployeeMapping>();

            if (await _projectEmployeeMappingService.IsEmployeeExist(model.ProjectId, model.EmployeeId))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.EmployeeExist"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingCreate.cshtml", model);
            }
            if (await _projectEmployeeMappingService.CheckTeamLeaderExist(model.ProjectId, model.RoleId))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.ProjectTeamLeaderAlreadyExsit"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingCreate.cshtml", model);
            }
            if (await _projectEmployeeMappingService.CheckManagerExist(model.ProjectId, model.RoleId))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.ProjectManagerAlreadyExsit"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingCreate.cshtml", model);
            }


            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingCreate.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                projectEmpMapping.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();

                await _projectEmployeeMappingService.InsertProjectEmployeeMappingAsync(projectEmpMapping);

                await _workflowMessageService.SendTeamMemberAddedMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                projectEmpMapping.EmployeeId, projectEmpMapping.RoleId, projectEmpMapping.ProjectId);
                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.ProjectEmployeeMapping.Added");
                TempData["SuccessMessage"] = successMessage;
                ViewBag.RefreshPage = true;

            }

            //prepare model
            //model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int projectId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.Edit))
                return AccessDeniedView();

            var projectEmpMapping = await _projectEmployeeMappingService.GetProjectsEmployeeMappingByIdAsync(id);
            ViewBag.RefreshPage = false;
            if (projectEmpMapping == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(null, projectEmpMapping);
            model.ProjectId = projectId;
            var project = await _projectsService.GetProjectsByIdAsync(projectId);
            if (project != null)
                model.ProjectName = project.ProjectTitle;

            return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProjectEmployeeMappingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var projectEmpMapping = await _projectEmployeeMappingService.GetProjectsEmployeeMappingByIdAsync(model.Id);
            if (projectEmpMapping == null)
                return RedirectToAction("List");

            int prevRoleId = projectEmpMapping.RoleId;
            int prevEmployeeId = projectEmpMapping.EmployeeId;

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;

            if (await _projectEmployeeMappingService.IsEmployeeExist(model.ProjectId, model.EmployeeId) && prevEmployeeId != model.EmployeeId)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.EmployeeExist"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingEdit.cshtml", model);
            }

            if (await _projectEmployeeMappingService.CheckTeamLeaderExist(model.ProjectId, model.RoleId) && prevRoleId != model.RoleId)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.ProjectTeamLeaderAlreadyExsit"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingEdit.cshtml", model);
            }
            if (await _projectEmployeeMappingService.CheckManagerExist(model.ProjectId, model.RoleId) && prevRoleId != model.RoleId)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.ProjectManagerAlreadyExsit"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);
                ViewBag.RefreshPage = false;
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingEdit.cshtml", model);
            }


            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _projectEmployeeMappingModelFactory.PrepareProjectEmployeeMappingModelAsync(model, projectEmpMapping, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingEdit.cshtml", model);
            }
            if (ModelState.IsValid)
            {
                projectEmpMapping = model.ToEntity(projectEmpMapping);

                await _projectEmployeeMappingService.UpdateProjectEmployeeMappingAsync(projectEmpMapping);

                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.ProjectEmployeeMapping.Added");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Projects/ProjectEmpMappingEdit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var projectEmpMapping = await _projectEmployeeMappingService.GetProjectsEmployeeMappingByIdAsync(id);
            if (projectEmpMapping == null)
                return RedirectToAction("List");

            await _projectEmployeeMappingService.DeleteProjectEmployeeMappingAsync(projectEmpMapping);



            return new NullJsonResult();
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectEmployeeMapping, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _projectEmployeeMappingService.GetprojectsEmployeeMappingByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _projectEmployeeMappingService.DeleteProjectEmployeeMappingAsync(item);
            }
            return Json(new { Result = true });
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetEmployeeDesignation(int employeeId)
        {
            // Fetch the employee designation based on employeeId
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                return Json(new { success = false, message = "Employee not found" });
            }

            // Return the designation information
            return Json(new { success = true, roleId = employee.DesignationId });
        }

        #endregion
    }
}