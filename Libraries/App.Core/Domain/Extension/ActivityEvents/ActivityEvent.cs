using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.ActivityEvents
{
    public partial class ActivityEvent : BaseEntity
    {
        public int TimesheetId { get; set; }

        public int KeyboardHits { get; set; }

        public int MouseHits { get; set; }

        public int EmployeeId { get; set; }

        public string JsonString { get; set; }

        public DateTime CreateOnUtc { get; set; }

    }
}
