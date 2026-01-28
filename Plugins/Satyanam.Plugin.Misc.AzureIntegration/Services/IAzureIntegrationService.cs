using App.Core;
using Satyanam.Nop.Core.Domains;
using Satyanam.Plugin.Misc.AzureIntegration.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AzureIntegration.Services;

public partial interface IAzureIntegrationService
{
	#region Methods

	Task InsertIAzureSyncLogAsync(AzureSyncLog azureSyncLog);

    Task<IPagedList<AzureSyncLog>> GetAllAzureSyncLogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, 
        int pageIndex = 0, int pageSize = int.MaxValue);

    Task DeleteAzureSyncLogsAsync(IList<AzureSyncLog> azureSyncLogs);

    Task<IList<AzureSyncLog>> GetAzureSyncLogByIdsAsync(int[] azureSyncLogIds);

    Task ClearAzureSyncLogAsync();

    Task<IList<TaskComments>> GetAllTaskCommentsByTaskIdAsync(int taskId = 0);

    #endregion
}
