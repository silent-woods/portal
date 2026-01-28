using App.Core;
using Humanizer;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Categorys
    /// </summary>
    public class TaskChangeLog : BaseEntity
    {
       
        public int TaskId { get; set; }

        public int StatusId { get; set; }

        public int AssignedTo { get; set; }

        public int EmployeeId { get; set; }

        public int LogTypeId { get; set; }

        public string Notes { get; set; }


        public DateTime CreatedOn { get; set; }

 
    }
}
