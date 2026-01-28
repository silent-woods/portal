using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.TimeSheets
{
    public partial class TimeSheetReport : BaseEntity
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }

        public int ProjectId { get; set; }
        public string ProjectName { get; set; }

        public bool IsHoliday { get; set; }
       public bool IsWeekend { get; set; }

        public bool IsLeave { get; set; }


        public bool IsHalfLeave { get; set; }

        public int TaskId { get; set; }

        public string TaskName { get; set; }
        public DateTime SpentDate { get; set; }
        public decimal EstimatedHours { get; set; }
        //public decimal SpentHours { get; set; }
        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }
        public int TotalMinutes { get; set; }
        public int BillableMinutes { get; set; }

        public List<int> EmployeeIds { get; set; }

        public string EmployeeNames { get; set; }


        public decimal TotalSpentHours { get; set; }
        public int TotalSpentMinutes { get; set; }


        public string SpentDateFormat { get; set; }
        public bool Billable { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }


    }
}
