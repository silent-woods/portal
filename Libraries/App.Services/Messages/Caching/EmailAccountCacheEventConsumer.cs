using App.Core.Domain.Messages;
using App.Services.Caching;
using System.Threading.Tasks;

namespace App.Services.Messages.Caching
{
    /// <summary>
    /// Represents an email account cache event consumer
    /// </summary>
    public partial class EmailAccountCacheEventConsumer : CacheEventConsumer<EmailAccount>
    {
    }
}
