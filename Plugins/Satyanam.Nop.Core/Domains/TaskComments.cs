using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Categorys
    /// </summary>
    public class TaskComments : BaseEntity
    {
       
        public int TaskId { get; set; }

        public int StatusId { get; set; }

        public string Description { get; set; }

        public int EmployeeId { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
