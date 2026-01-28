using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// LinkedInFollowupsImport service interface
    /// </summary>
    public partial interface ILinkedInFollowupsImportService
    {
        Task<string> ImportLinkedInFollowupsFromExcelAsync(IFormFile importFile);
    }
}