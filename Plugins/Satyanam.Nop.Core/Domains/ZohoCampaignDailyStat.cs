using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    public partial class ZohoCampaignDailyStat : BaseEntity
    {
        #region Properties

        public string   CampaignKey  { get; set; }
        public DateTime StatDate     { get; set; }
        public int      OpensCount   { get; set; }
        public int      ClicksCount  { get; set; }
        public int      BouncesCount { get; set; }
        public DateTime FetchedUtc   { get; set; }

        #endregion
    }
}
