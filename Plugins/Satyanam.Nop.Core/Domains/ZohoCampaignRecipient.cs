using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    public partial class ZohoCampaignRecipient : BaseEntity
    {
        #region Properties

        public string    CampaignKey    { get; set; }
        public string    RecipientEmail { get; set; }
        public int?      LeadId         { get; set; }
        public int?      ContactId      { get; set; }
        public bool      HasOpened      { get; set; }
        public bool      HasClicked     { get; set; }
        public bool      HasBounced       { get; set; }
        public bool      HasUnsubscribed  { get; set; }
        public int       OpenCount        { get; set; }
        public int       ClickCount       { get; set; }
        public int       BounceCount      { get; set; }
        public string    BounceType       { get; set; }
        public DateTime? FirstOpenedAt    { get; set; }
        public DateTime? LastOpenedAt     { get; set; }
        public DateTime? BouncedAt        { get; set; }
        public DateTime? UnsubscribedAt   { get; set; }
        public DateTime  SyncedUtc      { get; set; }

        #endregion
    }
}
