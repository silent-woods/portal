using App.Data.Extensions;
using App.Services.Helpers;
using App.Web.Framework.Models.Extensions;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.LeadAPILog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Factories;

public partial class LeadAPIModelFactory : ILeadAPIModelFactory
{
    #region Fields

    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly ILeadAPIService _leadAPIService;

    #endregion

    #region Ctor

    public LeadAPIModelFactory(IDateTimeHelper dateTimeHelper,
        ILeadAPIService leadAPIService)
    {
        _dateTimeHelper = dateTimeHelper;
        _leadAPIService = leadAPIService;
    }

    #endregion

    #region Lead API Log Methods

    public virtual async Task<LeadAPILogSearchModel> PrepareLeadAPILogSearchModelAsync(LeadAPILogSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<LeadAPILogListModel> PrepareLeadAPILogListModelAsync(LeadAPILogSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var startDateValue = !searchModel.SearchStartDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchStartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = !searchModel.SearchEndDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchEndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        var leadAPILogs = await _leadAPIService.SearchLeadAPILogsAsync(createdFromUtc: startDateValue, createdToUtc: endDateValue, pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new LeadAPILogListModel().PrepareToGridAsync(searchModel, leadAPILogs, () =>
        {
            return leadAPILogs.SelectAwait(async leadAPILog =>
            {
                var leadAPILogModel = new LeadAPILogModel
                {
                    Id = leadAPILog.Id,
                    EndPoint = leadAPILog.EndPoint,
                    RequestJson = leadAPILog.RequestJson,
                    ResponseJson = leadAPILog.ResponseJson,
                    ResponseMessage = leadAPILog.ResponseMessage,
                    Success = leadAPILog.Success,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(leadAPILog.CreatedOnUtc, DateTimeKind.Utc)
                };

                return leadAPILogModel;
            });
        });

        return model;
    }

    #endregion
}
