using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface ILeadAPIService
    {
        #region Lead API Log Methods

        Task InsertLeadAPILogAsync(LeadAPILog leadAPILog);

        Task<IPagedList<LeadAPILog>> SearchLeadAPILogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, int pageIndex = 0,
            int pageSize = int.MaxValue);

        Task DeleteLeadAPILogsAsync(IList<LeadAPILog> leadAPILogs);

        Task<IList<LeadAPILog>> GetLeadAPILogByIdsAsync(int[] leadAPILogIds);

        Task ClearLeadAPILogAsync();

        #endregion

    }
}