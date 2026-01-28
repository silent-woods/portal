using App.Core.Caching;
using App.Core.Domain.ProjectIntegrations;

namespace App.Services.Extension.ProjectIntegrations
{
    public static partial class NopProjectIntegrationDefaults
    {
        #region Static System Names

        public static CacheKey ProjectIntegrationSettingsByProjectIdCacheKey => new("Nop.projectIntegration.settings.byprojectid.{0}", ProjectIntegrationSettingsByProjectIdPrefix);

        public static string ProjectIntegrationSettingsByProjectIdPrefix => "Nop.projectIntegration.settings.byprojectid.";

        #endregion
    }
}
