using App.Data.Extensions;
using App.Services.Helpers;
using App.Services.ProjectTasks;
using App.Web.Framework.Models.Extensions;
using Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Models.AzureSyncLogs;
using Satyanam.Plugin.Misc.AzureIntegration.Services;
using System;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AzureIntegration.Areas.Admin.Factories;

public partial class AzureIntegrationModelFactory : IAzureIntegrationModelFactory
{
    #region Fields

    protected readonly IAzureIntegrationService _azureIntegrationService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IProjectTaskService _projectTaskService;

    #endregion

    #region Ctor

    public AzureIntegrationModelFactory(IAzureIntegrationService azureIntegrationService,
        IDateTimeHelper dateTimeHelper,
        IProjectTaskService projectTaskService)
    {
        _azureIntegrationService = azureIntegrationService;
        _dateTimeHelper = dateTimeHelper;
        _projectTaskService = projectTaskService;
    }

    #endregion

    #region Methods

    public virtual async Task<AzureSyncLogSearchModel> PrepareAzureSyncLogSearchModelAsync(AzureSyncLogSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<AzureSyncLogListModel> PrepareAzureSyncLogListModelAsync(AzureSyncLogSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var startDateValue = !searchModel.SearchStartDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchStartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = !searchModel.SearchEndDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchEndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        var azureSyncLogs = await _azureIntegrationService.GetAllAzureSyncLogsAsync(createdFromUtc: startDateValue, 
            createdToUtc: endDateValue, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new AzureSyncLogListModel().PrepareToGridAsync(searchModel, azureSyncLogs, () =>
        {
            return azureSyncLogs.SelectAwait(async azureSyncLog =>
            {
                var azureSyncLogModel = new AzureSyncLogModel
                {
                    Id = azureSyncLog.Id,
                    TaskName = (await _projectTaskService.GetProjectTasksByIdAsync(azureSyncLog.TaskId)).TaskTitle,
                    APIEndPoint = azureSyncLog.APIEndPoint,
                    Exception = azureSyncLog.Exception,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(azureSyncLog.CreatedOnUtc, DateTimeKind.Utc)
                };

                return azureSyncLogModel;
            });
        });

        return model;
    }

    #endregion
}
