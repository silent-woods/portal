using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models
{
    /// <summary>
    /// Represents authentication model
    /// </summary>
    public record AuthModel : BaseNopModel
    {
        public string SecretKey { get; set; }

        [NopResourceDisplayName("Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.VerificationToken")]
        public string Code { get; set; }

        public string QrCodeImageUrl { get; set; }

        [NopResourceDisplayName("Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.ManualSetupCode")]
        public string ManualEntryQrCode { get; set; }

        public string Account { get; set; }
    }
}
