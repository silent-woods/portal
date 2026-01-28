using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Models
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

        #endregion
    }
}