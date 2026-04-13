using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    public partial class ZohoCampaignStat : BaseEntity
    {
        #region Properties

        public string   CampaignKey           { get; set; }
        public string   CampaignName          { get; set; }
        public string   EmailSubject          { get; set; }
        public string   EmailFrom             { get; set; }
        public DateTime? SentTime             { get; set; }
        public DateTime? CreatedTime          { get; set; }

        public int      EmailsSentCount       { get; set; }
        public int      DeliveredCount        { get; set; }
        public decimal  DeliveredPercent      { get; set; }

        public int      OpensCount            { get; set; }
        public decimal  OpenPercent           { get; set; }
        public int      UnopenedCount         { get; set; }

        public int      UniqueClicksCount     { get; set; }
        public decimal  UniqueClickedPercent  { get; set; }
        public decimal  ClicksPerOpenRate     { get; set; }

        public int      BouncesCount          { get; set; }
        public int      HardBounceCount       { get; set; }
        public int      SoftBounceCount       { get; set; }
        public decimal  BouncePercent         { get; set; }

        public int      UnsubCount            { get; set; }
        public decimal  UnsubscribePercent    { get; set; }
        public int      SpamsCount            { get; set; }
        public int      ComplaintsCount       { get; set; }
        public int      ForwardsCount         { get; set; }
        public int      AutoreplyCount        { get; set; }

        public DateTime LastSyncedUtc         { get; set; }

        #endregion
    }
}
