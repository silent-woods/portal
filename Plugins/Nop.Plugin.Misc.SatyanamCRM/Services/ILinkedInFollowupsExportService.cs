using Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services
{
    /// <summary>
    /// LinkedInFollowups Export service interface
    /// </summary>
    public partial interface ILinkedInFollowupsExportService
    {
        Task<byte[]> ExportLinkedInFollowupsToExcelAsync(List<LinkedInFollowupsDto> items);
    }
}