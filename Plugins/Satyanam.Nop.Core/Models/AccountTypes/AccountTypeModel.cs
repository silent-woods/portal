using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Models.AccountTypes
{
    public record AccountTypeModel : BaseNopEntityModel
    {
        public AccountTypeModel()
        {
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.AccountTypes.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }

        #endregion
    }
}