using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.LeadStatus
{
    public record LeadStatusModel : BaseNopEntityModel
    {
        public LeadStatusModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.LeadStatus.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        #endregion
    }
}