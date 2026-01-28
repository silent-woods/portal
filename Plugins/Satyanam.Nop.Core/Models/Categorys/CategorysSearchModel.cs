using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Core.Models.Categorys
{
    public record CategorysSearchModel : BaseSearchModel
    {
        public CategorysSearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Categorys.CategorysSearchModel.SearchCategorysName")]
        public string SearchCategorysName { get; set; }

        #endregion
    }
}