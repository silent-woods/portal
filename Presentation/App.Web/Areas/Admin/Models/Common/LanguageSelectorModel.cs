using System.Collections.Generic;
using App.Web.Areas.Admin.Models.Localization;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    /// <summary>
    /// Represents an admin language selector model
    /// </summary>
    public partial record LanguageSelectorModel : BaseNopModel
    {
        #region Ctor

        public LanguageSelectorModel()
        {
            AvailableLanguages = new List<LanguageModel>();
        }

        #endregion

        #region Properties

        public IList<LanguageModel> AvailableLanguages { get; set; }

        public LanguageModel CurrentLanguage { get; set; }

        #endregion
    }
}