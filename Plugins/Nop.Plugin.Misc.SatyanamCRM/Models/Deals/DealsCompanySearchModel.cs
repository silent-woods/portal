using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals
{
    public record DealsCompanySearchModel : BaseSearchModel
    {
        public DealsCompanySearchModel()
        {

        }

        #region Properties
        public int CompanyId { get; set; }

        #endregion
    }
}