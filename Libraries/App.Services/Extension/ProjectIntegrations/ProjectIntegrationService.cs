using App.Core;
using App.Core.Caching;
using App.Core.Domain.ProjectIntegrations;
using App.Core.Domain.Projects;
using App.Data;
using App.Data.Extensions;
using App.Services.Extension.ProjectIntegrations;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.ProjectIntegrations;

public partial class ProjectIntegrationService : IProjectIntegrationService
{
    #region Fields

    protected readonly IRepository<Project> _projectRepository;
    protected readonly IRepository<ProjectIntegration> _projectIntegrationRepository;
    protected readonly IRepository<ProjectIntegrationMappings> _projectIntegrationMappingsRepository;
    protected readonly IRepository<ProjectIntegrationSettings> _projectIntegrationSettingsRepository;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public ProjectIntegrationService(IRepository<Project> projectRepository,
        IRepository<ProjectIntegration> projectIntegrationRepository,
        IRepository<ProjectIntegrationMappings> projectIntegrationMappingsRepository,
        IRepository<ProjectIntegrationSettings> projectIntegrationSettingsRepository,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext)
    {
        _projectRepository = projectRepository;
        _projectIntegrationRepository = projectIntegrationRepository;
        _projectIntegrationMappingsRepository = projectIntegrationMappingsRepository;
        _projectIntegrationSettingsRepository = projectIntegrationSettingsRepository;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
    }

    #endregion

    #region Project Integration Methods

    public virtual async Task InsertProjectIntegrationAsync(ProjectIntegration projectIntegration)
    {
        if (projectIntegration == null)
            throw new ArgumentNullException(nameof(projectIntegration));

        await _projectIntegrationRepository.InsertAsync(projectIntegration);
    }

    public virtual async Task UpdateProjectIntegrationAsync(ProjectIntegration projectIntegration)
    {
        if (projectIntegration == null)
            throw new ArgumentNullException(nameof(projectIntegration));

        await _projectIntegrationRepository.UpdateAsync(projectIntegration);
    }

    public virtual async Task DeleteProjectIntegrationAsync(ProjectIntegration projectIntegration)
    {
        if (projectIntegration == null)
            throw new ArgumentNullException(nameof(projectIntegration));

        var projectIntegrationMappings = await GetAllProjectIntegrationMappingsAsync(projectIntegration.Id);
        foreach (var projectIntegrationMapping in projectIntegrationMappings)
        {
            var projectIntegrationSettings = await GetAllProjectIntegrationSettingsAsync(projectIntegrationMapping.Id);
            foreach (var projectIntegrationSetting in projectIntegrationSettings)
                await DeleteProjectIntegrationSettingsAsync(projectIntegrationSetting);

            await DeleteProjectIntegrationMappingsAsync(projectIntegrationMapping);
        }

        await _projectIntegrationRepository.DeleteAsync(projectIntegration);
    }

    public virtual async Task<ProjectIntegration> GetProjectIntegrationByIdAsync(int id = 0)
    {
        if (id == 0)
            throw new ArgumentNullException(nameof(id));

        return await _projectIntegrationRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<ProjectIntegration>> GetAllProjectIntegrationsAsync(int pageIndex = 0, int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        var projectIntegrations = from pi in _projectIntegrationRepository.Table
                                  select pi;

        if (showHidden)
            projectIntegrations = projectIntegrations.Where(pi => pi.IsActive);

        projectIntegrations = projectIntegrations.OrderByDescending(pi => pi.DisplayOrder);

        return await projectIntegrations.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Project Integration Mapping Methods

    public virtual async Task InsertProjectIntegrationMappingsAsync(ProjectIntegrationMappings projectIntegrationMappings)
    {
        if (projectIntegrationMappings == null)
            throw new ArgumentNullException(nameof(projectIntegrationMappings));

        await _projectIntegrationMappingsRepository.InsertAsync(projectIntegrationMappings);
    }

    public virtual async Task DeleteProjectIntegrationMappingsAsync(ProjectIntegrationMappings projectIntegrationMappings)
    {
        if (projectIntegrationMappings == null)
            throw new ArgumentNullException(nameof(projectIntegrationMappings));

        var projectIntegrationSettings = await GetAllProjectIntegrationSettingsAsync(projectIntegrationMappings.Id);
        foreach (var projectIntegrationSetting in projectIntegrationSettings)
            await DeleteProjectIntegrationSettingsAsync(projectIntegrationSetting);

        await _projectIntegrationMappingsRepository.DeleteAsync(projectIntegrationMappings);
    }

    public virtual async Task<ProjectIntegrationMappings> GetProjectIntegrationMappingsByIdAsync(int id = 0)
    {
        if (id == 0)
            throw new ArgumentNullException(nameof(id));

        return await _projectIntegrationMappingsRepository.GetByIdAsync(id);
    }

    public virtual async Task<ProjectIntegrationMappings> GetProjectIntegrationMappingsByProjectAndIntegrationIdAsync(int integrationId = 0,
        int projectId = 0)
    {
        var projectIntegrationMapping = from pim in _projectIntegrationMappingsRepository.Table
                                        where pim.IntegrationId == integrationId && pim.ProjectId == projectId
                                        select pim;

        return await projectIntegrationMapping.FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<ProjectIntegrationMappings>> GetAllProjectIntegrationMappingsAsync(int integrationId = 0,
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var projectIntegrationMappings = from pim in _projectIntegrationMappingsRepository.Table
                                         join p in _projectRepository.Table on pim.ProjectId equals p.Id
                                         select pim;

        if (integrationId > 0)
            projectIntegrationMappings = projectIntegrationMappings.Where(pim => pim.IntegrationId == integrationId);

        projectIntegrationMappings = projectIntegrationMappings.OrderByDescending(pi => pi.Id);

        return await projectIntegrationMappings.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Project Integration Settings Methods

    public virtual async Task InsertProjectIntegrationSettingsAsync(ProjectIntegrationSettings projectIntegrationSettings)
    {
        if (projectIntegrationSettings == null)
            throw new ArgumentNullException(nameof(projectIntegrationSettings));

        await _projectIntegrationSettingsRepository.InsertAsync(projectIntegrationSettings);
    }

    public virtual async Task UpdateProjectIntegrationSettingsAsync(ProjectIntegrationSettings projectIntegrationSettings)
    {
        if (projectIntegrationSettings == null)
            throw new ArgumentNullException(nameof(projectIntegrationSettings));

        await _projectIntegrationSettingsRepository.UpdateAsync(projectIntegrationSettings);
    }

    public virtual async Task DeleteProjectIntegrationSettingsAsync(ProjectIntegrationSettings projectIntegrationSettings)
    {
        if (projectIntegrationSettings == null)
            throw new ArgumentNullException(nameof(projectIntegrationSettings));

        await _projectIntegrationSettingsRepository.DeleteAsync(projectIntegrationSettings);
    }

    public virtual async Task<ProjectIntegrationSettings> GetProjectIntegrationSettingsByIdAsync(int id = 0)
    {
        if (id == 0)
            throw new ArgumentNullException(nameof(id));

        return await _projectIntegrationSettingsRepository.GetByIdAsync(id);
    }

    public virtual async Task<IPagedList<ProjectIntegrationSettings>> GetAllProjectIntegrationSettingsAsync(
        int projectIntegrationMappingId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var projectIntegrationSettings = from pis in _projectIntegrationSettingsRepository.Table
                                         where pis.ProjectIntegrationMappingId == projectIntegrationMappingId
                                         select pis;

        return await projectIntegrationSettings.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Sync Project Integration Methods

    public virtual async Task<IList<ProjectIntegrationSettings>> GetProjectIntegrationSettingsByProjectIdAsync(int projectId = 0)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
            NopProjectIntegrationDefaults.ProjectIntegrationSettingsByProjectIdCacheKey, projectId);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var projectIntegrationSettings = from pis in _projectIntegrationSettingsRepository.Table
                                             join pim in _projectIntegrationMappingsRepository.Table on pis.ProjectIntegrationMappingId equals pim.Id
                                             join pi in _projectIntegrationRepository.Table on pim.IntegrationId equals pi.Id
                                             where pim.ProjectId == projectId && pi.IsActive
                                             select pis;

            return await projectIntegrationSettings.ToListAsync();
        });
    }

    #endregion
}
