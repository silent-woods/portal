using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Campaings
{
    public record CampaingsEmailLogSearchModel : BaseSearchModel
    {
        public CampaingsEmailLogSearchModel()
        {
        }

        #region Properties
        public int CampaignId { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.Email")]
        public string Email { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.IsOpened")]
        public bool? IsOpened { get; set; }                         
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.IsClicked")]
        public bool? IsClicked { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.OpenCountMin")]
        public int? OpenCountMin { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.OpenCountMax")]
        public int? OpenCountMax { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.ClickCountMin")]
        public int? ClickCountMin { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.ClickCountMax")]
        public int? ClickCountMax { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Misc.SatyanamCRM.Models.CampaingsEmailLogSearchModel.IsUnsubscribed")]
        public bool? IsUnsubscribed { get; set; }

        #endregion
    }
}