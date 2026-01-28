using App.Core;
using App.Core.Domain.ProjectIntegrations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.ProjectIntegrations;

public partial interface IProjectIntegrationService
{
    #region Project Integration Methods

    Task InsertProjectIntegrationAsync(ProjectIntegration projectIntegration);

    Task UpdateProjectIntegrationAsync(ProjectIntegration projectIntegration);

    Task DeleteProjectIntegrationAsync(ProjectIntegration projectIntegration);

    Task<ProjectIntegration> GetProjectIntegrationByIdAsync(int id = 0);

    Task<IPagedList<ProjectIntegration>> GetAllProjectIntegrationsAsync(int pageIndex = 0, int pageSize = int.MaxValue, 
        bool showHidden = false);

    #endregion

    #region Project Integration Mapping Methods

    Task InsertProjectIntegrationMappingsAsync(ProjectIntegrationMappings projectIntegrationMappings);

    Task DeleteProjectIntegrationMappingsAsync(ProjectIntegrationMappings projectIntegrationMappings);

    Task<ProjectIntegrationMappings> GetProjectIntegrationMappingsByIdAsync(int id = 0);

    Task<ProjectIntegrationMappings> GetProjectIntegrationMappingsByProjectAndIntegrationIdAsync(int integrationId = 0,
        int projectId = 0);

    Task<IPagedList<ProjectIntegrationMappings>> GetAllProjectIntegrationMappingsAsync(int integrationId = 0, 
        int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

    #endregion

    #region Project Integration Settings Methods

    Task InsertProjectIntegrationSettingsAsync(ProjectIntegrationSettings projectIntegrationSettings);

    Task UpdateProjectIntegrationSettingsAsync(ProjectIntegrationSettings projectIntegrationSettings);

    Task DeleteProjectIntegrationSettingsAsync(ProjectIntegrationSettings projectIntegrationSettings);

    Task<ProjectIntegrationSettings> GetProjectIntegrationSettingsByIdAsync(int id = 0);

    Task<IPagedList<ProjectIntegrationSettings>> GetAllProjectIntegrationSettingsAsync(
        int projectIntegrationMappingId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

    #endregion

    #region Sync Project Integration Methods

    Task<IList<ProjectIntegrationSettings>> GetProjectIntegrationSettingsByProjectIdAsync(int projectId = 0);

    #endregion
}
