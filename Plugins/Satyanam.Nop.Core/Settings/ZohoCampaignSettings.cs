using App.Core.Configuration;
using System;

namespace Satyanam.Nop.Core.Settings
{
    public partial class ZohoCampaignSettings : ISettings
    {
        #region Properties

        public string    ClientId               { get; set; }
        public string    ClientSecret           { get; set; }
        public string    RefreshToken           { get; set; }
        public string    AccessToken            { get; set; }
        public DateTime? AccessTokenExpiresUtc  { get; set; }
        public bool      IsEnabled              { get; set; }
        public DateTime? LastSyncedUtc          { get; set; }
        public int       CampaignFetchLimit     { get; set; } = 10;

        #endregion
    }
}
