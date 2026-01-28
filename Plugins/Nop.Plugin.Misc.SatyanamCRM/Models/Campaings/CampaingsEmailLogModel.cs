using App.Web.Framework.Models;
using System;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Campaings
{
    public record CampaingsEmailLogModel : BaseNopEntityModel
    {
        public CampaingsEmailLogModel()
        {

        }

        #region Properties

        public string Email { get; set; }
        public bool IsOpened { get; set; }
        public int OpenCount { get; set; }
        public bool IsClicked { get; set; }
        public int ClickCount { get; set; }
        public bool IsUnsubscribed { get; set; }
        public DateTime? OpenedOnUtc { get; set; }

        #endregion
    }
}