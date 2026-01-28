using App.Core.Domain.ProjectIntegrations;
using App.Web.Areas.Admin.Models.ProjectIntegrations;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories;

public partial interface IProjectIntegrationModelFactory
{
    #region Project Integration Methods

    Task<ProjectIntegrationSearchModel> PrepareProjectIntegrationSearchModelAsync(ProjectIntegrationSearchModel searchModel);
    
    Task<ProjectIntegrationListModel> PrepareProjectIntegrationListModelAsync(ProjectIntegrationSearchModel searchModel);

    Task<ProjectIntegrationModel> PrepareProjectIntegrationModelAsync(ProjectIntegrationModel model, ProjectIntegration projectIntegration);

    #endregion

    #region Project Integration Mapping Methods

    Task<ProjectIntegrationMappingsListModel> PrepareProjectIntegrationMappingsListModelAsync(
        ProjectIntegrationMappingsSearchModel searchModel);

    #endregion

    #region Project Integration Settings Methods

    Task<ProjectIntegrationSettingsSearchModel> PrepareProjectIntegrationSettingsSearchModelAsync(int projectIntegrationId,
        int projectIntegrationMappingId, ProjectIntegrationSettingsSearchModel searchModel);

    Task<ProjectIntegrationSettingsListModel> PrepareProjectIntegrationSettingsListModelAsync(ProjectIntegrationSettingsSearchModel searchModel);

    #endregion
}
