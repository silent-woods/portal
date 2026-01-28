using App.Core.Domain.Extension.ProjectIntegrations;
using App.Core.Domain.ProjectIntegrations;
using App.Core.Domain.Security;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectIntegrations;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.ProjectIntegrations;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers;

public partial class ProjectIntegrationController : BaseAdminController
{
	#region Fields

	protected readonly ILocalizationService _localizationService;
	protected readonly INotificationService _notificationService;
	protected readonly IPermissionService _permissionService;
	protected readonly IProjectIntegrationModelFactory _projectIntegrationModelFactory;
	protected readonly IProjectIntegrationService _projectIntegrationService;

	#endregion

	#region Ctor

	public ProjectIntegrationController(ILocalizationService localizationService, 
		INotificationService notificationService,
		IPermissionService permissionService,
		IProjectIntegrationModelFactory projectIntegrationModelFactory, 
		IProjectIntegrationService projectIntegrationService)
	{
		_localizationService = localizationService;
		_notificationService = notificationService;
		_permissionService = permissionService;
		_projectIntegrationModelFactory = projectIntegrationModelFactory;
		_projectIntegrationService = projectIntegrationService;
	}

    #endregion

    #region Utilities

    protected virtual async Task<IList<ProjectIntegrationSettings>> InsertAzureSettingsAsync(int projectIntegrationMappingId)
    {
        var projectIntegrationSettings = new List<ProjectIntegrationSettings>();

        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzureOrganizationName,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzureProjectName,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzureClientId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzureClientSecret,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzureTenantId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzureUserId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AzurePersonalAccessToken,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });

        return projectIntegrationSettings;
    }

    protected virtual async Task<IList<ProjectIntegrationSettings>> InsertJiraSettingsAsync(int projectIntegrationMappingId)
    {
        var projectIntegrationSettings = new List<ProjectIntegrationSettings>();

        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.JiraClientId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.JiraClientSecret,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.JiraTenantId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.JiraUserId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });

        return projectIntegrationSettings;
    }

    protected virtual async Task<IList<ProjectIntegrationSettings>> InsertAsanaSettingsAsync(int projectIntegrationMappingId)
    {
        var projectIntegrationSettings = new List<ProjectIntegrationSettings>();

        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AsanaClientId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AsanaClientSecret,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AsanaTenantId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });
        projectIntegrationSettings.Add(new ProjectIntegrationSettings
        {
            ProjectIntegrationMappingId = projectIntegrationMappingId,
            KeyName = ProjectIntegrationDefaults.AsanaUserId,
            KeyValue = string.Empty,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        });

        return projectIntegrationSettings;
    }

    #endregion

    #region Project Integration Methods

    public virtual async Task<IActionResult> List()
	{
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.View))
            return AccessDeniedView();

		var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationSearchModelAsync(new ProjectIntegrationSearchModel());

        return View("/Areas/Admin/Views/Extension/ProjectIntegration/List.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(ProjectIntegrationSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.View))
            return AccessDeniedView();

        var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
	{
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.Add))
            return AccessDeniedView();

        var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationModelAsync(new ProjectIntegrationModel(), null);

        return View("/Areas/Admin/Views/Extension/ProjectIntegration/Create.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Create(ProjectIntegrationModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.Add))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var projectIntegration = new ProjectIntegration()
            {
                IntegrationName = model.IntegrationName,
                SystemName = model.SystemName,
                IsActive = model.IsActive,
                DisplayOrder = model.DisplayOrder,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };
            await _projectIntegrationService.InsertProjectIntegrationAsync(projectIntegration);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.System.ProjectIntegration.Added"));

            if (!continueEditing)
                return RedirectToAction(nameof(List));

            return RedirectToAction(nameof(Edit), new { id = projectIntegration.Id });
        }

        model = await _projectIntegrationModelFactory.PrepareProjectIntegrationModelAsync(model, null);

        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
	{
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.Edit))
            return AccessDeniedView();

        var projectIntegration = await _projectIntegrationService.GetProjectIntegrationByIdAsync(id);
        if (projectIntegration == null)
            return RedirectToAction(nameof(List));

        var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationModelAsync(null, projectIntegration);

        return View("/Areas/Admin/Views/Extension/ProjectIntegration/Edit.cshtml", model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(ProjectIntegrationModel model, bool continueEditing)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.Edit))
            return AccessDeniedView();

        var projectIntegration = await _projectIntegrationService.GetProjectIntegrationByIdAsync(model.Id);
        if (projectIntegration == null)
            return RedirectToAction(nameof(List));

        if (ModelState.IsValid)
        {
            projectIntegration.IntegrationName = model.IntegrationName;
            projectIntegration.SystemName = model.SystemName;
            projectIntegration.IsActive = model.IsActive;
            projectIntegration.DisplayOrder = model.DisplayOrder;
            projectIntegration.UpdatedOnUtc = DateTime.UtcNow;
            await _projectIntegrationService.UpdateProjectIntegrationAsync(projectIntegration);

            foreach (var selectedProjectId in model.SelectedProjectIds)
            {
                var existingProjectIntegrationMapping = await _projectIntegrationService.GetProjectIntegrationMappingsByProjectAndIntegrationIdAsync(
                    model.Id, selectedProjectId);
                if (existingProjectIntegrationMapping == null)
                {
                    ProjectIntegrationSettings projectIntegrationSettings = null;
                    var projectIntegrationMapping = new ProjectIntegrationMappings()
                    {
                        IntegrationId = model.Id,
                        ProjectId = selectedProjectId,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };
                    await _projectIntegrationService.InsertProjectIntegrationMappingsAsync(projectIntegrationMapping);

                    if (model.SystemName == ProjectIntegrationDefaults.Azure)
                    {
                        var azureSettings = await InsertAzureSettingsAsync(projectIntegrationMapping.Id);
                        foreach (var azureSetting in azureSettings)
                            await _projectIntegrationService.InsertProjectIntegrationSettingsAsync(azureSetting);
                    }
                    else if (model.SystemName == ProjectIntegrationDefaults.Jira)
                    {
                        var jiraSettings = await InsertJiraSettingsAsync(projectIntegrationMapping.Id);
                        foreach (var jiraSetting in jiraSettings)
                            await _projectIntegrationService.InsertProjectIntegrationSettingsAsync(jiraSetting);
                    }
                    else if (model.SystemName == ProjectIntegrationDefaults.Asana)
                    {
                        var asanaSettings = await InsertAsanaSettingsAsync(projectIntegrationMapping.Id);
                        foreach (var asanaSetting in asanaSettings)
                            await _projectIntegrationService.InsertProjectIntegrationSettingsAsync(asanaSetting);
                    }
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.System.ProjectIntegration.Updated"));

            if (!continueEditing)
                return RedirectToAction(nameof(List));

            return RedirectToAction(nameof(Edit), new { id = projectIntegration.Id });
        }

        model = await _projectIntegrationModelFactory.PrepareProjectIntegrationModelAsync(model, projectIntegration);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegration, PermissionAction.Delete))
            return AccessDeniedView();

        var projectIntegration = await _projectIntegrationService.GetProjectIntegrationByIdAsync(id);
        if (projectIntegration == null)
            return RedirectToAction(nameof(List));

        await _projectIntegrationService.DeleteProjectIntegrationAsync(projectIntegration);

        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.System.ProjectIntegration.Deleted"));

        return RedirectToAction(nameof(List));
    }

    #endregion

    #region Project Integration Mapping Methods

    [HttpPost]
    public virtual async Task<IActionResult> ProjectIntegrationMappings(ProjectIntegrationMappingsSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegrationMappings, PermissionAction.View))
            return AccessDeniedView();

        var projectIntegration = await _projectIntegrationService.GetProjectIntegrationByIdAsync(searchModel.ProjectIntegrationId);
        if (projectIntegration == null)
            return RedirectToAction(nameof(List));

        var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationMappingsListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> DeleteProjectIntegrationMapping(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegrationMappings, PermissionAction.Delete))
            return AccessDeniedView();

        var projectIntegrationMappings = await _projectIntegrationService.GetProjectIntegrationMappingsByIdAsync(id);
        if (projectIntegrationMappings == null)
            return RedirectToAction(nameof(List));

        await _projectIntegrationService.DeleteProjectIntegrationMappingsAsync(projectIntegrationMappings);

        return new NullJsonResult();
    }

    #endregion

    #region Product Integration Settings Methods

    [HttpPost]
    public virtual async Task<IActionResult> ProjectIntegrationSettings(ProjectIntegrationSettingsSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegrationSettings, PermissionAction.View))
            return AccessDeniedView();

        var projectIntegrationMappings = await _projectIntegrationService.GetProjectIntegrationMappingsByIdAsync(searchModel.ProjectIntegrationMappingId);
        if (projectIntegrationMappings == null)
            return RedirectToAction(nameof(List));

        var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationSettingsListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> ProjectIntegrationSettings(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegrationSettings, PermissionAction.View))
            return AccessDeniedView();

        var projectIntegrationMappings = await _projectIntegrationService.GetProjectIntegrationMappingsByIdAsync(id);
        if (projectIntegrationMappings == null)
            return RedirectToAction(nameof(List));

        var model = await _projectIntegrationModelFactory.PrepareProjectIntegrationSettingsSearchModelAsync(projectIntegrationMappings.IntegrationId,
            projectIntegrationMappings.Id, new ProjectIntegrationSettingsSearchModel());

        return View("/Areas/Admin/Views/Extension/ProjectIntegration/ProjectIntegrationSettings.cshtml", model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProjectIntegrationSettingsUpdate(ProjectIntegrationSettingsModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProjectIntegrationSettings, PermissionAction.Edit))
            return AccessDeniedView();

        var projectIntegrationSettings = await _projectIntegrationService.GetProjectIntegrationSettingsByIdAsync(model.Id);
        if (projectIntegrationSettings == null)
            return RedirectToAction(nameof(List));

        projectIntegrationSettings.KeyValue = model.Value;
        projectIntegrationSettings.UpdatedOnUtc = DateTime.UtcNow;
        await _projectIntegrationService.UpdateProjectIntegrationSettingsAsync(projectIntegrationSettings);

        return new NullJsonResult();
    }

    #endregion
}
