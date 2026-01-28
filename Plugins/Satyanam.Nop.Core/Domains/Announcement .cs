using App.Core;
using System;

namespace Satyanam.Nop.Core.Domains
{
    /// <summary>
    /// Represents a  Lead
    /// </summary>
    public class Announcement : BaseEntity
    {
        /// <summary>
        /// Gets or sets the title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the message 
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the creation date (UTC)
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the scheduled publish date (UTC), if any
        /// </summary>
        public DateTime? ScheduledOnUtc { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of employee IDs who liked the announcement
        
        /// </summary>
        public string LikedEmployeeIds { get; set; }

        /// <summary>
        /// Gets or sets the attachment file path (relative/absolute)
        /// </summary>
        public string AttachmentPath { get; set; }

        /// <summary>
        /// Gets or sets the attachment file name
        /// </summary>
        public string AttachmentName { get; set; }

        /// <summary>
        /// Gets or sets the audience type (All, Project, Designation, Employee)
        /// </summary>
        public int AudienceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the reference Ids (comma-separated IDs depending on AudienceType)
        /// Example:
        ///  - Project IDs if AudienceType = Project
        ///  - Designation IDs if AudienceType = Designation
        ///  - Employee IDs if AudienceType = Employee
        /// </summary>
        public string ReferenceIds { get; set; }

        /// <summary>
        /// Gets or sets the actual employee Ids to whom the announcement was sent (comma-separated)
        /// </summary>
        public string SendEmployeeIds { get; set; }

        public bool IsSent { get; set; }

    }

    /// <summary>
    /// Audience type for announcements
    /// </summary>
    public enum AudienceType
    {
        All = 0,
        Project = 1,
        Designation = 2,
        Employee = 3
    }
}
