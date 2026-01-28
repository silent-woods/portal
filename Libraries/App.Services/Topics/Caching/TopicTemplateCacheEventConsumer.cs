using System.Threading.Tasks;
using App.Core.Domain.Topics;
using App.Services.Caching;

namespace App.Services.Topics.Caching
{
    /// <summary>
    /// Represents a topic template cache event consumer
    /// </summary>
    public partial class TopicTemplateCacheEventConsumer : CacheEventConsumer<TopicTemplate>
    {
    }
}
