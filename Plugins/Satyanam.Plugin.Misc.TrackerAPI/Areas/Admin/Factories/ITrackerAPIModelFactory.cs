using Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Models.TrackerAPILog;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Areas.Admin.Factories;

public partial interface ITrackerAPIModelFactory
{
	#region Tracker API Log Methods

	Task<TrackerAPILogSearchModel> PrepareTrackerAPILogSearchModelAsync(TrackerAPILogSearchModel searchModel);

	Task<TrackerAPILogListModel> PrepareTrackerAPILogListModelAsync(TrackerAPILogSearchModel searchModel);

    #endregion
}
