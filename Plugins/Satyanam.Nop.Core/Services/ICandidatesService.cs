using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// ICandidates service interface
    /// </summary>
    public partial interface ICandidatesService
    {
        #region Candidate

        /// <summary>
        /// Get candidate by id
        /// </summary>
        Task<Candidate> GetCandidateByIdAsync(int id);
        Task<IList<Candidate>> GetCandidateByIdsAsync(int[] candidateIds);
        /// <summary>
        /// Get all candidates (paged)
        /// </summary>
        Task<IPagedList<Candidate>> GetAllCandidatesAsync(string name = "", string email = "", string positionApplied = "", int candidatetypeId = 0, int statusId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    
        /// <summary>
        /// Insert candidate
        /// </summary>
        Task InsertCandidateAsync(Candidate candidate);

        /// <summary>
        /// Update candidate
        /// </summary>
        Task UpdateCandidateAsync(Candidate candidate);

        /// <summary>
        /// Delete candidate (soft delete)
        /// </summary>
        Task DeleteCandidateAsync(Candidate candidate);

        Task InsertJobApplicationAsync(JobApplication application);
        Task<IPagedList<Candidate>> GetCandidatesByJobPostingIdNewAsync(int jobPostingId, int pageIndex = 0, int pageSize = int.MaxValue);
        Task UpdateJobApplicationStatusAsync(int candidateId, int jobPostingId, int statusId, string HrNotes);
        Task<JobApplication> GetJobApplicationAsync(int candidateId, int jobPostingId);
        Task<IList<JobApplication>> GetJobApplicationsByCandidateIdAsync(int candidateId);
        Task<Candidate> GetCandidateByEmailAsync(string email);
        Task UpdateJobApplicationAsync(JobApplication application);
        Task<JobApplication> GetJobApplicationByCandidateIdAsync(int candidateId);
        #endregion
    }
}