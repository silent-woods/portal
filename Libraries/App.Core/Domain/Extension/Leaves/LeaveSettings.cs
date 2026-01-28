using App.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.Leaves
{
    public partial class LeaveSettings : ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }
       
        public string HrEmail { get; set; }

        public bool SendEmailToAllProjectLeaders { get; set; }
        public bool SendEmailToAllProjectManager { get; set; }


        public string CommonEmails { get; set; }

        public bool SendEmailToEmployeeManager { get; set; }

        public int SeletedLeaveTypeId { get; set; }

        public DateTime LeaveTestDate { get; set; }

        public string LastUpdateBalance { get; set; }

        public int AddMonthlyLeaveDay { get; set; }

    }
}
