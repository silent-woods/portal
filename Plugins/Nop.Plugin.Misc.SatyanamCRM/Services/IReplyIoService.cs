using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// ReplyIo service interface
    /// </summary>
    public partial interface IReplyIoService
    {
        Task<bool> CreateOrUpdateLeadAsync(ReplyLeadDto lead);
    }
}