using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Company
{
    public record CompanyContactSearchModel : BaseSearchModel
    {
        public CompanyContactSearchModel()
        {

        }

        #region Properties

        public int CompanyId { get; set; }



        #endregion
    }
}