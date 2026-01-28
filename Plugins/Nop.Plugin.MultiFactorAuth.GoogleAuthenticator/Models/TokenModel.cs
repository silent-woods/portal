using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Models
{
    /// <summary>
    /// Represents verification model
    /// </summary>
    public record TokenModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.VerificationToken")]
        public string Token { get; set; }
    }
}
