namespace App.Core.Domain.ManageResumes
{
    /// <summary>
    /// Represents a InterviewStatus
    /// </summary>
    public enum InterviewStatusEnum
    {
        Select = 0,
        New = 1,
        Reviewed = 2,
        ApprovedForTechnical = 3,
        ScheduledTechnical = 4,
        ScheduledPractical = 5,
        ScheduledHR = 6,
        Hired = 7,
        Rejected = 8,
        Cancelled = 9,
    }
}