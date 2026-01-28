using App.Core.Caching;

namespace App.Web.Areas.Admin.Infrastructure.Cache
{
    public static partial class NopModelCacheDefaults
    {
        /// <summary>
        /// Key for nopCommerce.com news cache
        /// </summary>
        public static CacheKey OfficialNewsModelKey => new("Nop.pres.admin.official.news");
    }
}