using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Company
{
    public record CompanyDealsSearchModel : BaseSearchModel
    {
        public CompanyDealsSearchModel()
        {

        }

        #region Properties

        public int CompanyId { get; set; }



        #endregion
    }
}