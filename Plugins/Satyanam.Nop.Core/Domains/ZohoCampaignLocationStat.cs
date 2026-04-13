using App.Core;

namespace Satyanam.Nop.Core.Domains
{
    public partial class ZohoCampaignLocationStat : BaseEntity
    {
        #region Properties

        public string CampaignKey { get; set; }
        public string Country     { get; set; }
        public int    OpensCount  { get; set; }

        #endregion
    }
}
