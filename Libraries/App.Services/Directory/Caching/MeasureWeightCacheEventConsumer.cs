using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Caching;

namespace App.Services.Directory.Caching
{
    /// <summary>
    /// Represents a measure weight cache event consumer
    /// </summary>
    public partial class MeasureWeightCacheEventConsumer : CacheEventConsumer<MeasureWeight>
    {
    }
}
