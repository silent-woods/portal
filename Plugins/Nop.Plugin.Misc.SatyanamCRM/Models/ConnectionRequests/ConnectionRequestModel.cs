using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests
{
    public record ConnectionRequestModel : BaseNopEntityModel
    {
        public ConnectionRequestModel()
        {
            Status = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.FirstName")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.LastName")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.LinkedinUrl")]
        public string LinkedinUrl { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.Email")]
        public string Email { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.WebsiteUrl")]
        public string WebsiteUrl { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.Status")]
        public int StatusId { get; set; }
        public IList<SelectListItem> Status { get; set; }
        public string StatusText { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.CreatedOnUtc")]
        public DateTime CreatedOnUtc { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Model.ConnectionRequests.UpdatedOnUtc")]
        public DateTime UpdatedOnUtc { get; set; }
        #endregion
    }
}