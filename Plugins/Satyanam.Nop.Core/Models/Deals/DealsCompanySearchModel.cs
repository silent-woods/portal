using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Deals
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