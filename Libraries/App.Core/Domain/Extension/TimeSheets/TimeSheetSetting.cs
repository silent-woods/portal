using App.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.TimeSheets
{
    public partial class TimeSheetSetting: ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public DateTime? Reminder1_From { get; set; }
        public DateTime? Reminder1_To { get; set; }
        public DateTime? Reminder2_From { get; set; }
        public DateTime? Reminder2_To { get; set; }

        public string DepartmentIds { get; set; }


        public bool SendEmailToAllProjectLeaders { get; set; }
        public bool SendEmailToAllProjectManager { get; set; }
        public bool SendEmailToHr { get; set; }



        public string CommonEmails { get; set; }
        public bool SendEmailToEmployeeManager { get; set; }


        public int ConsiderBeforeDay { get; set; }

        public int SendWithCCAfterDay { get; set; }

    }
}
