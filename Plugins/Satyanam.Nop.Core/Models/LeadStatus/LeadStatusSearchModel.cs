using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Core.Models.LeadStatus
{
    public record LeadStatusSearchModel : BaseSearchModel
    {
        public LeadStatusSearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.LeadStatus.LeadStatusSearchModel.SearchLeadStatusName")]
        public string SearchLeadStatusName { get; set; }

        #endregion
    }
}