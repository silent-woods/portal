using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Plugin.Misc.EmailVerification.Models
{
    /// <summary>
    /// Represents a configuration model
    /// </summary>
    public record ConfigurationModel : BaseNopModel
    {
        #region Ctor


        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Misc.EmailVerification.Fields.ApiKey")]
        public string ApiKey { get; set; }


        [NopResourceDisplayName("Plugins.Misc.EmailVerification.Fields.ApiUrl")]
        public string ApiUrl { get; set; }
        [NopResourceDisplayName("Plugins.Misc.EmailVerification.Fields.DisconnectOnUninstall")]
        public bool DisconnectOnUninstall { get; set; }
        [NopResourceDisplayName("Plugins.Misc.EmailVerification.Fields.Registartionpage")]
        public bool Registartionpage { get; set; }
        [NopResourceDisplayName("Plugins.Misc.EmailVerification.Fields.ContactUspage")]
        public bool ContactUspage { get; set; }

        [NopResourceDisplayName("Plugins.Misc.EmailVerification.Fields.EBookspage")]
        public bool EBookspage { get; set; }

        #endregion
    }
}