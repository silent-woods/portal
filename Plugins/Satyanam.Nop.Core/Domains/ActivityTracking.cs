using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    public partial class ActivityTracking : BaseEntity
    {
        #region Properties

        public int EmployeeId { get; set; }

        public int ActiveDuration { get; set; }

        public int AwayDuration { get; set; }

        public int OfflineDuration { get; set; }

        public int StoppedDuration { get; set; }

        public int TotalDuration { get; set; }

        public string JsonString { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        #endregion
    }

    public partial class DailyActivityTrackingModel
    {
        public DateTime Date { get; set; }
        public int ActiveDuration { get; set; }
        public int AwayDuration { get; set; }
        public int OfflineDuration { get; set; }
        public int StoppedDuration { get; set; }
        public int TotalDuration { get; set; }

        public string ActiveDurationHHMM { get; set; }
        public string AwayDurationHHMM { get; set; }
        public string OfflineDurationHHMM { get; set; }
        public string StoppedDurationHHMM { get; set; }
        public string TotalDurationHHMM { get; set; }
    }

}
