using Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Models.LeadAPILog;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.LeadAPI.Areas.Admin.Factories;

public partial interface ILeadAPIModelFactory
{
	#region Lead API Log Methods

	Task<LeadAPILogSearchModel> PrepareLeadAPILogSearchModelAsync(LeadAPILogSearchModel searchModel);

	Task<LeadAPILogListModel> PrepareLeadAPILogListModelAsync(LeadAPILogSearchModel searchModel);

    #endregion
}
