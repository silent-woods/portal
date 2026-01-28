using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// LeadImport service interface
    /// </summary>
    public partial interface ILeadImportService
    {
        Task ImportLeadsFromExcelAsync(IFormFile importFile);
        Task ImportLeadsFromExcelReplyAsync(IFormFile importFile);
    }
}