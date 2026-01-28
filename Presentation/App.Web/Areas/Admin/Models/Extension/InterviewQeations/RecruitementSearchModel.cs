using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.InterviewQeations.Models
{
    /// <summary>
    /// Represents a custom event search model
    /// </summary>
    public record RecruitementSearchModel : BaseSearchModel
    {
        #region Ctor
        #endregion

        #region Properties



        [NopResourceDisplayName("Plugins.Widgets.FacebookPixel.Configuration.CustomEvents.Search.WidgetZone")]
        public string Category { get; set; }

        public int Id { get; set; }




        #endregion
    }
}

