using System;
namespace App.Core.Domain.result
{
    /// <summary>
    /// Represents a TimeSheet
    /// </summary>
    public partial class CandidatesResult : BaseEntity
    {
   //Candidate result//
        public int CandidateId { get; set; }
        public int ResultStatusId { get; set; }
        public string Feedback { get; set; }
        public string Communication { get; set; }
        public string ConfidentLevel { get; set; }
        public string Incorrect { get; set; }
        public string partially { get; set; }
        public string correct { get; set; }
       
        public string ResultData { get; set; }
        public string Marks { get; set; }

    }
}