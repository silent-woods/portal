using App.Core.Configuration;
using System;

namespace App.Core.Domain.Extension.TimeSheets
{
    public partial class MonthlyReportSetting : ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        public decimal AllowedVariations { get; set; }

        public decimal AllowedQABillableHours { get; set; }


        //for weekly report email
        public int WeekDay { get; set; }

        public DateTime? DayTime_From { get; set; }

        public DateTime? DayTime_To { get; set; }

       

        public bool SendReportToEmployee { get; set; }

        public bool SendReportToProjectLeader { get; set; }
        
        public bool SendReportToManager { get; set; }

        public bool ShowOnlyNotDOT { get; set; }

       public bool SendReportToHR { get; set; }


        public int LearningProjectId { get; set; }


        //for overdue reminder email


        public DateTime? OverDue_From { get; set; }

        public DateTime? OverDue_To { get; set; }
        public bool SendOverdueEmailToEmployee { get; set; }

        public bool SendOverdueReportToProjectLeader { get; set; }

        public bool SendOverdueReportToManager { get; set; }

        public bool SendOverdueReportToHR { get; set; }

        public int OverdueCountCCThreshold { get; set; }

        public bool IncludeProjectLeadersInCC { get; set; }

        public bool IncludeProjectManagerInCC { get; set; }

        public bool IncludeManagementInCC { get; set; }

        public bool IncludeProjectCoordinatorInCC { get; set; }


        public bool IncludeHRInCC { get; set; }









    }
}
