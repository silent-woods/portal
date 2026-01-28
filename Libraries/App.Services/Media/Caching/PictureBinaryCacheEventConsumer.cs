using App.Core.Domain.Media;
using App.Services.Caching;

namespace App.Services.Media.Caching
{
    /// <summary>
    /// Represents a picture binary cache event consumer
    /// </summary>
    public partial class PictureBinaryCacheEventConsumer : CacheEventConsumer<PictureBinary>
    {
    }
}
