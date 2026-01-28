using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Models.Titles
{
    public record TitleModel : BaseNopEntityModel
    {
        public TitleModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Titles.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        #endregion
    }
}