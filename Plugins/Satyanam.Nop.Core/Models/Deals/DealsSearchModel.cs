using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Nop.Core.Models.Deals
{
    public record DealsSearchModel : BaseSearchModel
    {
        public DealsSearchModel()
        {
            Stages = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Deals.DealsSearchModel.DealName")]
        public string DealName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Deals.DealsSearchModel.Amount")]
        public int Amount { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Deals.DealsSearchModel.StageId")]
        public int StageId { get; set; }
        public IList<SelectListItem> Stages { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Deals.DealsSearchModel.ClosingDate")]
        [UIHint("DateNullable")]
        public DateTime? ClosingDate { get; set; }
        

        #endregion
    }
}