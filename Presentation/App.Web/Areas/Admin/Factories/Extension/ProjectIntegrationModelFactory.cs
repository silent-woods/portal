using App.Core.Domain.ProjectIntegrations;
using App.Data.Extensions;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.ProjectIntegrations;
using App.Services.Projects;
using App.Web.Areas.Admin.Models.ProjectIntegrations;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories;

public partial class ProjectIntegrationModelFactory : IProjectIntegrationModelFactory
{
    #region Fields

    protected readonly IDateTimeHelper _dateTimeHelper;
	protected readonly ILocalizationService _localizationService;
	protected readonly IProjectIntegrationService _projectIntegrationService;
    protected readonly IProjectsService _projectsService;

	#endregion

	#region Ctor

	public ProjectIntegrationModelFactory(IDateTimeHelper dateTimeHelper,
        ILocalizationService localizationService,
		IProjectIntegrationService projectIntegrationService,
        IProjectsService projectsService)
	{
        _dateTimeHelper = dateTimeHelper;
		_localizationService = localizationService;
		_projectIntegrationService = projectIntegrationService;	
        _projectsService = projectsService;
	}

    #endregion

    #region Utilities

    protected virtual async Task<ProjectIntegrationMappingsSearchModel> PrepareProjectIntegrationMappingsSearchModelAsync(int integrationId,
        ProjectIntegrationMappingsSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.ProjectIntegrationId = integrationId;

        searchModel.SetGridPageSize();

        return searchModel;
    }

    #endregion

    #region Project Integration Methods

    public virtual async Task<ProjectIntegrationSearchModel> PrepareProjectIntegrationSearchModelAsync(ProjectIntegrationSearchModel searchModel)
	{
		if (searchModel == null)
			throw new ArgumentNullException(nameof(searchModel));

		searchModel.SetGridPageSize();

		return searchModel;
	}

    public virtual async Task<ProjectIntegrationListModel> PrepareProjectIntegrationListModelAsync(ProjectIntegrationSearchModel searchModel)
	{
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

		var projectIntegrations = await _projectIntegrationService.GetAllProjectIntegrationsAsync(pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new ProjectIntegrationListModel().PrepareToGridAsync(searchModel, projectIntegrations, () =>
        {
            return projectIntegrations.SelectAwait(async projectIntegration =>
            {
                var projectIntegrationModel = new ProjectIntegrationModel()
                {
                    Id = projectIntegration.Id,
                    IntegrationName = projectIntegration.IntegrationName,
                    SystemName = projectIntegration.SystemName,
                    IsActive = projectIntegration.IsActive,
                    DisplayOrder = projectIntegration.DisplayOrder
                };

                return projectIntegrationModel;

            });
        });

        return model;
    }

    public virtual async Task<ProjectIntegrationModel> PrepareProjectIntegrationModelAsync(ProjectIntegrationModel model, ProjectIntegration projectIntegration)
    {
        if (projectIntegration != null)
        {
            model = model ?? new ProjectIntegrationModel();

            model.Id = projectIntegration.Id;
            model.IntegrationName = projectIntegration.IntegrationName;
            model.SystemName = projectIntegration.SystemName;
            model.IsActive = projectIntegration.IsActive;
            model.DisplayOrder = projectIntegration.DisplayOrder;

            await PrepareProjectIntegrationMappingsSearchModelAsync(projectIntegration.Id, model.ProjectIntegrationMappingsSearchModel);

            var projects = await _projectsService.GetAllProjectsAsync(string.Empty);
            model.AvailableProjects = projects.Select(project => new SelectListItem
            {
                Text = project.ProjectTitle,
                Value = project.Id.ToString()
            }).ToList();
        }

        return model;
    }

    #endregion

    #region Project Integration Mapping Methods

    public virtual async Task<ProjectIntegrationMappingsListModel> PrepareProjectIntegrationMappingsListModelAsync(
        ProjectIntegrationMappingsSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var projectIntegrationMappings = await _projectIntegrationService.GetAllProjectIntegrationMappingsAsync(searchModel.ProjectIntegrationId,
            searchModel.Page - 1, searchModel.PageSize);

        var model = await new ProjectIntegrationMappingsListModel().PrepareToGridAsync(searchModel, projectIntegrationMappings, () =>
        {
            return projectIntegrationMappings.SelectAwait(async projectIntegrationMapping =>
            {
                var projectIntegrationMappingsModel = new ProjectIntegrationMappingsModel()
                {
                    Id = projectIntegrationMapping.Id,
                    ProjectName = (await _projectsService.GetProjectsByIdAsync(projectIntegrationMapping.ProjectId)).ProjectTitle,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(projectIntegrationMapping.CreatedOnUtc, DateTimeKind.Utc)
                };

                return projectIntegrationMappingsModel;
            });
        });

        return model;
    }


    #endregion

    #region Project Integration Settings Methods

    public virtual async Task<ProjectIntegrationSettingsSearchModel> PrepareProjectIntegrationSettingsSearchModelAsync(int projectIntegrationId,
        int projectIntegrationMappingId, ProjectIntegrationSettingsSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        searchModel.ProjectIntegrationId = projectIntegrationId;

        searchModel.ProjectIntegrationMappingId = projectIntegrationMappingId;

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<ProjectIntegrationSettingsListModel> PrepareProjectIntegrationSettingsListModelAsync(ProjectIntegrationSettingsSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var projectIntegrationSettings = await _projectIntegrationService.GetAllProjectIntegrationSettingsAsync(searchModel.ProjectIntegrationMappingId,
            searchModel.Page - 1, searchModel.PageSize);

        var model = await new ProjectIntegrationSettingsListModel().PrepareToGridAsync(searchModel, projectIntegrationSettings, () =>
        {
            return projectIntegrationSettings.SelectAwait(async projectIntegrationSetting =>
            {
                var projectIntegrationSettingsModel = new ProjectIntegrationSettingsModel()
                {
                    Id = projectIntegrationSetting.Id,
                    SettingsName = projectIntegrationSetting.KeyName,
                    Value = projectIntegrationSetting.KeyValue,
                };

                return projectIntegrationSettingsModel;

            });
        });

        return model;
    }

    #endregion
}
