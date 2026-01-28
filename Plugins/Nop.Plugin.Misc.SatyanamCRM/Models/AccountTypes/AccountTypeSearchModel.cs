using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.SatyanamCRM.Models.AccountTypes
{
    public record AccountTypeSearchModel : BaseSearchModel
    {
        public AccountTypeSearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.AccountTypes.AccountTypeSearchModel.SearchAccountTypeName")]
        public string SearchAccountTypeName { get; set; }

        #endregion
    }
}