using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a setting search model
    /// </summary>
    public partial record SettingSearchModel : BaseSearchModel
    {
        #region Ctor

        public SettingSearchModel()
        {
            AddSetting = new SettingModel();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Configuration.Settings.AllSettings.SearchSettingName")]
        public string SearchSettingName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.AllSettings.SearchSettingValue")]
        public string SearchSettingValue { get; set; }

        public SettingModel AddSetting { get; set; }

        #endregion
    }
}