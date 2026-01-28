using System;
using System.Runtime.CompilerServices;

namespace App.Core.Domain.Leaves
{
    /// <summary>
    /// Represents a LeaveManagement
    /// </summary>
    public partial class LeaveTransactionLog : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int LeaveId { get; set; }      
        public int StatusId { get; set; }
        public int ApprovedId { get; set; }     

        public decimal LeaveBalance { get; set; }

        public decimal BalanceChange { get; set; }
        public decimal ManualBalanceChange { get; set; }

        public DateTime BalanceMonthYear { get; set; }

   
        public string Comment { get; set; }
        public DateTime CreatedOnUTC { get; set; }
    }
}