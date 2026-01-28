using App.Web.Framework.Models;

namespace Satyanam.Nop.Core.Models.Unsubscribes
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