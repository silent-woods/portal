using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Core.Models.Titles
{
    public record TitleSearchModel : BaseSearchModel
    {
        public TitleSearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Titles.TitleSearchModel.SearchTitleName")]
        public string SearchTitleName { get; set; }

        #endregion
    }
}