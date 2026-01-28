using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Helpers;
using App.Web.Framework.Models.Extensions;
using Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Models.TrackerAPILog;
using Satyanam.Plugin.Misc.TrackerAPI.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Factories;

public partial class TrackerAPIModelFactory : ITrackerAPIModelFactory
{
    #region Fields

	protected readonly IDateTimeHelper _dateTimeHelper;
	protected readonly ITrackerAPIService _trackerAPIService;

	#endregion

	#region Ctor

	public TrackerAPIModelFactory(IDateTimeHelper dateTimeHelper,
		ITrackerAPIService trackerAPIService)
	{
		_dateTimeHelper = dateTimeHelper;
		_trackerAPIService = trackerAPIService; 
	}

    #endregion

    #region Tracker API Log Methods

    public virtual async Task<TrackerAPILogSearchModel> PrepareTrackerAPILogSearchModelAsync(TrackerAPILogSearchModel searchModel)
	{
		ArgumentNullException.ThrowIfNull(nameof(searchModel));

		searchModel.SetGridPageSize();

		return searchModel;
	}

    public virtual async Task<TrackerAPILogListModel> PrepareTrackerAPILogListModelAsync(TrackerAPILogSearchModel searchModel)
	{
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var startDateValue = !searchModel.SearchStartDate.HasValue ? null
			: (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchStartDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());

        var endDateValue = !searchModel.SearchEndDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchEndDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);

        var trackerAPILogs = await _trackerAPIService.SearchTrackerAPILogsAsync(createdFromUtc: startDateValue, createdToUtc: endDateValue, pageIndex: searchModel.Page - 1, 
			pageSize: searchModel.PageSize);

        var model = await new TrackerAPILogListModel().PrepareToGridAsync(searchModel, trackerAPILogs, () =>
        {
            return trackerAPILogs.SelectAwait(async trackerAPILog =>
            {
                var trackerAPILogModel = new TrackerAPILogModel
                {
                    Id = trackerAPILog.Id,
                    EndPoint = trackerAPILog.EndPoint,
                    RequestJson = trackerAPILog.RequestJson,
                    ResponseJson = trackerAPILog.ResponseJson,
                    ResponseMessage = trackerAPILog.ResponseMessage,
                    Success = trackerAPILog.Success,
                    CreatedOn = trackerAPILog.CreatedOnUtc
                };

                if (trackerAPILog.EmployeeId > 0)
                {
                    var employee = await _trackerAPIService.GetEmployeeByIdAsync(trackerAPILog.EmployeeId ?? 0);
                    if (employee != null)
                        trackerAPILogModel.EmployeeEmail = employee.OfficialEmail;
                }

                return trackerAPILogModel;
            });
        });

        return model;
    }

    #endregion
}
