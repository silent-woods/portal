using App.Web.Framework.Models;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Models
{
    /// <summary>
    /// Represents GoogleAuthenticator list model
    /// </summary>
    public record GoogleAuthenticatorListModel : BasePagedListModel<GoogleAuthenticatorModel>
    {
    }
}
