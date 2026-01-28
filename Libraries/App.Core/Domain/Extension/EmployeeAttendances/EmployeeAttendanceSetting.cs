using App.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.EmployeeAttendanceSetting
{
    public partial class EmployeeAttendanceSetting: ISettings
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        public DateTime? OfficeTime_From { get; set; }
        public DateTime? OfficeTime_To { get; set; }
      

    }
}
