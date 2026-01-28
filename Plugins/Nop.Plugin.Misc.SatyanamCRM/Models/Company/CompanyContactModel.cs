using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Company
{
    public record CompanyContactModel : BaseNopEntityModel
    {
        public CompanyContactModel()
        {
            

        }

        #region Properties
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.FirstName")]
        [Required(ErrorMessage = "Please enter a firstname.")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.LastName")]
        [Required(ErrorMessage = "Please enter a lastname.")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.Contacts.TitleId")]
        public int TitleId { get; set; }
        public string TitleName { get; set; }
        public IList<SelectListItem> Titles { get; set; }   
        #endregion
    }
}