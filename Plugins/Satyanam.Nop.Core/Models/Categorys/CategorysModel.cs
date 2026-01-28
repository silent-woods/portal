using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Models.Categorys
{
    public record CategorysModel : BaseNopEntityModel
    {
        public CategorysModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Categorys.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        #endregion
    }
}