using App.Core.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.PerformanceMeasurements
{
    public partial class TeamPerformanceSettings : ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }
       
        public int FeedbackShowId { get; set; }

        public int StartReminderDate { get; set; }

        public int StartCCDate { get; set; }
       


    }
    public enum FeedBackShow
    {
        [Display(Name = "Not Show")]
        NotShow = 0,
        [Display(Name = "Not With Name")]
        ShowWithName = 1,
        [Display(Name = "Not Without Name")]
        ShowWithoutName = 2,


    }

    public enum WeekDay
    {
        Sunday = 0,
        Monday = 1,
        Tuesday = 2,
        Wednesday = 3,
        Thursday = 4,
        Friday = 5,
        Saturday = 6
    }

}
