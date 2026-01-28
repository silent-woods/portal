using System;

namespace App.Core.Domain.Employees
{
    /// <summary>
    /// Represents a Education
    /// </summary>
    public partial class Education : BaseEntity
    {
       
        public int EmployeeID { get; set; }
        public string Course { get; set; }
        public string InstitutionName { get; set; }
        public decimal MarksScored { get; set; }
        public string YearOfCompletion { get; set; }

    }
}