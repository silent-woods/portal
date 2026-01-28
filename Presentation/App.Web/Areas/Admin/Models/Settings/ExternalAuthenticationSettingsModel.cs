using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents an external authentication settings model
    /// </summary>
    public partial record ExternalAuthenticationSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AllowCustomersToRemoveAssociations")]
        public bool AllowCustomersToRemoveAssociations { get; set; }

        #endregion
    }
}