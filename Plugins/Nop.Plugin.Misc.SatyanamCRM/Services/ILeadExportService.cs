using Microsoft.AspNetCore.Http;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// LeadExport service interface
    /// </summary>
    public partial interface ILeadExportService
    {
        Task<byte[]> ExportLeadsToExcelAsync(List<LeadDto> leads);
        Task<byte[]> ExportLeadsToExcelReplyAsync(List<LeadDto> leads);
    }
}