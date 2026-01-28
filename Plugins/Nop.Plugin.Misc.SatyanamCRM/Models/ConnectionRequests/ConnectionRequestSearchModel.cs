using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests
{
    public record ConnectionRequestSearchModel : BaseSearchModel
    {
        public ConnectionRequestSearchModel()
        {
            Status = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.ConnectionRequests.SearchFirstName")]
        public string SearchFirstName { get; set; }                 
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.ConnectionRequests.SearchLastName")]
        public string SearchLastName { get; set; }                  
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.ConnectionRequests.SearchLinkedinUrl")]
        public string SearchLinkedinUrl { get; set; }               
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.ConnectionRequests.SearchEmail")]
        public string SearchEmail { get; set; }                     
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.ConnectionRequests.SearchWebsiteUrl")]
        public string SearchWebsiteUrl { get; set; }                
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.ConnectionRequests.SearchStatus")]
        public int? SearchStatus { get; set; }
        public IList<SelectListItem> Status { get; set; }

        #endregion
    }
}