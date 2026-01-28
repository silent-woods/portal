using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Core.Models.LeadSources
{
    public record LeadSourceSearchModel : BaseSearchModel
    {
        public LeadSourceSearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.LeadSources.LeadSourceSearchModel.SearchLeadSourceName")]
        public string SearchLeadSourceName { get; set; }

        #endregion
    }
}