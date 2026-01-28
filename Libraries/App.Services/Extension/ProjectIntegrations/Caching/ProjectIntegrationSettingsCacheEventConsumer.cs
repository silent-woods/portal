using App.Core.Domain.ProjectIntegrations;
using App.Services.Caching;
using System.Threading.Tasks;

namespace App.Services.Extension.ProjectIntegrations.Caching
{
    public partial class ProjectIntegrationSettingsCacheEventConsumer : CacheEventConsumer<ProjectIntegrationSettings>
    {
        #region Methods

        protected override async Task ClearCacheAsync(ProjectIntegrationSettings entity)
        {
            await RemoveByPrefixAsync(NopProjectIntegrationDefaults.ProjectIntegrationSettingsByProjectIdPrefix);
        }

        #endregion
    }
}
