using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Inquiries
{
    public record InquirySearchModel : BaseSearchModel
    {
        public InquirySearchModel()
        {
            AvailableSources = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Inquiry.InquirySearchModel.SearchName")]
        public string SearchName { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Inquiry.InquirySearchModel.SearchEmail")]
        public string SearchEmail { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Inquiry.InquirySearchModel.SearchContactNo")]
        public string SearchContactNo { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Inquiry.InquirySearchModel.SearchCompany")]
        public string SearchCompany { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.Inquiry.InquirySearchModel.SearchSourceId")]
        public int? SearchSourceId { get; set; }
        public IList<SelectListItem> AvailableSources { get; set; }

        #endregion
    }
}
