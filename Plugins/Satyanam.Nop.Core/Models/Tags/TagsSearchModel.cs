using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Core.Models.Tags
{
    public record TagsSearchModel : BaseSearchModel
    {
        public TagsSearchModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Tags.TagsSearchModel.SearchTagsName")]
        public string SearchTagsName { get; set; }

        #endregion
    }
}