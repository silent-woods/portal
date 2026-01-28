using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Extension.WeeklyQuestions
{
    public partial class WeeklyReports : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int DesignationId { get; set; }
        public string Qdata { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
