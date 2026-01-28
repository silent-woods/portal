using System;

namespace App.Core.Domain.Employees
{
    /// <summary>
    /// Represents a Experience
    /// </summary>
    public partial class Experience : BaseEntity
    {
       
        public int EmployeeID { get; set; }
        public string PreviousCompanyName { get; set; }
        public string Designation { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

    }
}