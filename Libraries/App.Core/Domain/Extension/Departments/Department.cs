using System;

namespace App.Core.Domain.Departments
{
    /// <summary>
    /// Department
    /// </summary>
    public partial class Department : BaseEntity
    {
        /// <summary>
        /// Gets or sets the department name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance Updation
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }
    }
}
