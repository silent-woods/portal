namespace App.Core.Domain.ManageResumes
{
    /// <summary>
    /// Represents a customer-customer role mapping class
    /// </summary>
    public partial class CandidateInterviewerMapping : BaseEntity
    {
        /// <summary>
        /// Gets or sets the customer identifier
        /// </summary>
        public int CandidatesId { get; set; }

        /// <summary>
        /// Gets or sets the customer role identifier
        /// </summary>
        public int EmployeeId { get; set; }
    }
}