using App.Core.Domain.Messages;
using App.Services.Caching;
using System.Threading.Tasks;

namespace App.Services.Messages.Caching
{
    /// <summary>
    /// Represents a message template cache event consumer
    /// </summary>
    public partial class MessageTemplateCacheEventConsumer : CacheEventConsumer<MessageTemplate>
    {
        /// <summary>
        /// Clear cache data
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task ClearCacheAsync(MessageTemplate entity)
        {
            await RemoveByPrefixAsync(NopMessageDefaults.MessageTemplatesByNamePrefix, entity.Name);
        }
    }
}
