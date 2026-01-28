using Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// ConnectionRequest Export service interface
    /// </summary>
    public partial interface IConnectionRequestExportService
    {
        Task<byte[]> ExportConnectionRequestToExcelAsync(List<ConnectionRequestDto> items);
    }
}