using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Localization
{
    /// <summary>
    /// Represents a locale resource model
    /// </summary>
    public partial record LocaleResourceModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.Name")]
        public string ResourceName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Languages.Resources.Fields.Value")]
        public string ResourceValue { get; set; }

        public int LanguageId { get; set; }

        #endregion
    }
}