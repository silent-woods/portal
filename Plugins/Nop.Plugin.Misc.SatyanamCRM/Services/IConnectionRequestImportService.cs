using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// ConnectionRequestImport service interface
    /// </summary>
    public partial interface IConnectionRequestImportService
    {
        Task<string> ImportConnectionRequestFromExcelAsync(IFormFile importFile);
    }
}