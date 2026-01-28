using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record EmailSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.EmailSettings.Fields.FirstMailVariation")]
        public decimal FirstMailVariation { get; set; }

        public bool FirstMailVariation_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.EmailSettings.Fields.SecondMailVariation")]
        public decimal SecondMailVariation { get; set; }

        public bool SecondMailVariation_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.EmailSettings.Fields.ThirdMailVariation")]
        public decimal ThirdMailVariation { get; set; }

        public bool ThirdMailVariation_OverrideForStore { get; set; }

        #endregion
    }
}
