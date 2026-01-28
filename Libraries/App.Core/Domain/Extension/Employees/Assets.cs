using System;

namespace App.Core.Domain.Employees
{
    /// <summary>
    /// Represents a Assets
    /// </summary>
    public partial class Assets : BaseEntity
    {
       
        public int EmployeeID { get; set; }
        public int TypeId { get; set; }
        public string Name { get; set; }
        public int DocumentId { get; set; }
        public string Description { get; set; }

 

    }
}