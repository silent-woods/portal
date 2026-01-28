using System;

namespace App.Core.Domain.Security
{
    /// <summary>
    /// Represents a permission record-customer role mapping class
    /// </summary>
    public partial class PermissionRecordCustomerRoleMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the permission record identifier
        /// </summary>
        public int PermissionRecordId { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
        public int CustomerRoleId { get; set; }

        public bool Add { get; set; }

        public bool Edit { get; set; }

        public bool Delete { get; set; }

        public bool View { get; set; }

        public bool Full { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}