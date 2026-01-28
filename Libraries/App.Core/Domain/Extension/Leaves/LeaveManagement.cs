using System;

namespace App.Core.Domain.Leaves
{
    /// <summary>
    /// Represents a LeaveManagement
    /// </summary>
    public partial class LeaveManagement : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int LeaveTypeId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string ReasonForLeave { get; set; }
        public decimal NoOfDays { get; set; }
        public int StatusId { get; set; }
        public int ApprovedId { get; set; }

        public string SendMailIds { get; set; }
        public bool IsArchived { get; set; }
        public DateTime? ApprovedOnUTC { get; set; }
        public DateTime CreatedOnUTC { get; set; }
    }
}