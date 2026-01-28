using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// LeadExportManager service interface
    /// </summary>
    public partial interface ILeadExportManagerService
    {
        Task<byte[]> ExportLeadsToExcelAsync(List<LeadDto> leads);
    }
}