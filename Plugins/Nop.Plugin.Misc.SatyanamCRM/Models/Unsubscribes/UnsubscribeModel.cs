using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Unsubscribes
{
    public record UnsubscribeModel : BaseNopEntityModel
    {
        public UnsubscribeModel()
        {
        }

        #region Properties
        public string Email { get; set; }
        public string Message { get; set; }
        #endregion
    }
}