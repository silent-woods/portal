using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Models.Campaings
{
    public record CampaingsSearchModel : BaseSearchModel
    {
        public CampaingsSearchModel()
        {
            Status = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Categorys.CampaingsSearchModel.Name")]
        public string Name { get; set; }
        
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Categorys.CampaingsSearchModel.StatusId")]
        public int StatusId { get; set; }
        public IList<SelectListItem> Status { get; set; }
        #endregion
    }
}