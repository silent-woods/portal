using System;
namespace App.Core.Domain.ProjectEmployeeMappings
{
    /// <summary>
    /// Represents a ProjectEmployeeMapping
    /// </summary>
    public partial class ProjectEmployeeMapping : BaseEntity
    {
        public int ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public int RoleId { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreateOnUtc { get; set; }
    }
}