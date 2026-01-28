using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Models.Contacts
{
    public record ContactsSearchModel : BaseSearchModel
    {
        public ContactsSearchModel()
        {
            
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Contacts.ContactsSearchModel.SearchName")]
        public string SearchName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Contacts.ContactsSearchModel.SearchCompanyName")]
        public string SearchCompanytName { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Contacts.ContactsSearchModel.SearchWebsiteUrl")]
        public string SearchWebsiteUrl { get; set; }
        
        
        #endregion
    }
}