using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.LeadSources
{
    public record LeadSourceModel : BaseNopEntityModel
    {
        public LeadSourceModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LeadSources.LeadSourceModel.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        #endregion
    }
}