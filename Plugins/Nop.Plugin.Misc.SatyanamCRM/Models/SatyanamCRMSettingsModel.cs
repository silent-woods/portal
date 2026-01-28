using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models
{
    public record SatyanamCRMSettingsModel : BaseNopEntityModel
    {
        public SatyanamCRMSettingsModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.LinkedInUrl")]
        public string LinkedInUrl { get; set; }

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.WebsiteUrl")]
        public string WebsiteUrl { get; set; }

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.FooterText")]
        public string FooterText { get; set; }

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.APIKey")]
        public string APIKey { get; set; }

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.APISecret")]
        public string APISecret { get; set; }

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.AllowedDomains")]
        public string AllowedDomains { get; set; }

        [NopResourceDisplayName("SatyanamCRM.Admin.Settings.InquiryEmailSendTo")]
        public string InquiryEmailSendTo { get; set; }

        #endregion
    }
}