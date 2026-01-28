using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Core.Models.Company
{
    public record CompanySearchModel : BaseSearchModel
    {
        public CompanySearchModel()
        {

        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Company.ComapnySearchModel.CompanyName")]
        public string CompanyName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Company.ComapnySearchModel.SearchWebsiteUrl")]
        public string SearchWebsiteUrl { get; set; }

        #endregion
    }
}