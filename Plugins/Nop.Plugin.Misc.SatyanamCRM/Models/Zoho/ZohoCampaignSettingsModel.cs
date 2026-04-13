using System;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Models.Zoho
{
    public partial class ZohoCampaignSettingsModel
    {
        public string    ClientId              { get; set; }
        public string    ClientSecret          { get; set; }
        public string    RefreshToken          { get; set; }
        public bool      IsEnabled             { get; set; }
        public int       CampaignFetchLimit    { get; set; } = 10;
        public DateTime? LastSyncedUtc         { get; set; }
    }
}
