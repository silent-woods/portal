using System;
namespace App.Core.Domain.ManageResumes
{
    /// <summary>
    /// Represents a TimeSheet
    /// </summary>
    public partial class CandidatesResumes : BaseEntity
    {

        public string Gender { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime? DateofBirth { get; set; }

        public string Email { get; set; }

        public string CollegeName { get; set; }

        public string Degree { get; set; }

        public string CompletedYear { get; set; }

        public decimal Percentage { get; set; }

        public string ResumeUrl { get; set; }

        public string Skill { get; set; }

        public string MobileNo { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string ZipPostalCode { get; set; }

        public string City { get; set; }

        public string AboutYourSelf { get; set; }

        public DateTime? DateForInterview { get; set; }
        public DateTime? CreatedOnUTC { get; set; }

        public string PersonInCharge { get; set; }

        public string Venue { get; set; }

        public int DownloadId { get; set; }

        public int AppliedForId { get; set; }
        public int StatusId { get; set; }
        public int EmployeeId { get; set; } 
        public int TraineeId { get; set; } 
        public DateTime TechnicalRoundDate { get; set; }
        public DateTime PracticalRoundDate { get; set; }

        
    }
}