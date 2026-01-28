using Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Models.AzureSyncLogs;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Factories;

public partial interface IAzureIntegrationModelFactory
{
    #region Methods

    Task<AzureSyncLogSearchModel> PrepareAzureSyncLogSearchModelAsync(AzureSyncLogSearchModel searchModel);

    Task<AzureSyncLogListModel> PrepareAzureSyncLogListModelAsync(AzureSyncLogSearchModel searchModel);

    #endregion
}
