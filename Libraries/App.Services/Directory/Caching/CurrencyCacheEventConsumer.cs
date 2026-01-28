using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Caching;

namespace App.Services.Directory.Caching
{
    /// <summary>
    /// Represents a currency cache event consumer
    /// </summary>
    public partial class CurrencyCacheEventConsumer : CacheEventConsumer<Currency>
    {
    }
}
