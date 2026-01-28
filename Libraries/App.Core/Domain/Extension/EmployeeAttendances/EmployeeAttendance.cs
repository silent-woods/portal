using System;
namespace App.Core.Domain.EmployeeAttendances
{
    /// <summary>
    /// Represents a EmployeeAttendance
    /// </summary>
    public partial class EmployeeAttendance : BaseEntity
    {
        public int EmployeeId { get; set; }
        //public DateTime Date { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }

        public int StatusId { get; set; }
        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
    }
}