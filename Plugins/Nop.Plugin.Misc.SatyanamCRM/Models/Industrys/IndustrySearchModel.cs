using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Industrys
{
    public record IndustrySearchModel : BaseSearchModel
    {
        public IndustrySearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Industrys.IndustrySearchModel.SearchIndustryName")]
        public string SearchIndustryName { get; set; }

        #endregion
    }
}