using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals
{
    public record DealsContactSearchModel : BaseSearchModel
    {
        public DealsContactSearchModel()
        {

        }

        #region Properties
        public int ContactId { get; set; }

        #endregion
    }
}