using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App.Core.Domain.Activities
{
    public partial class Activity : BaseEntity
    {
     
        public int EmployeeId { get; set; }

        public int TaskId { get; set; }

        public string ActivityName { get; set; }

        public int SpentHours { get; set; }

        public int SpentMinutes { get; set; }


        public DateTime CreateOnUtc { get; set; }

        public DateTime UpdateOnUtc { get; set; }



    }
}
