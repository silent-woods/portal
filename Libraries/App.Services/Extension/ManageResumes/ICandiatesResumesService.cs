using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.result;
using Nop.Core.Domain.Catalog;

namespace App.Services.ManageResumes
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface ICandiatesResumesService
    {
        /// <summary>
        /// Gets all CandiatesResumes
        /// </summary>
        Task<IPagedList<CandidatesResumes>> GetAllCandiatesResumesAsync(string firstName, string lastName, string email, string mobileno, string degree, int appliedForId, int statusId, DateTime? TechnicalRoundDate, DateTime? PracticalRoundDate, int[] InterviewrIds = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="Candiatesid"></param>
        /// <returns></returns>
        Task<CandidatesResumes> GetCandiatesResumesByIdAsync(int Candiatesid);

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task InsertCandiatesResumesAsync(CandidatesResumes  candiatesResumes);

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task UpdateCandiatesResumesAsync(CandidatesResumes candiatesResumes);

        /// <summary>
        /// Delete project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task DeleteCandiatesResumesAsync(CandidatesResumes candiatesResumes);

        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="designationIds"></param>
        /// <returns></returns>
        Task<IList<CandidatesResumes>> GetCandiatesResumesByIdsAsync(int[] projectIds);
        Task<IList<Employee>> GetCustomerRoleBySystemNameAsync(CandidatesResumes employee);
        Task<int[]> GetEmployeeIdsAsync(CandidatesResumes customer, bool showHidden = false);
        Task AddCustomerRoleMappingAsync(CandidateInterviewerMapping roleMapping);
        Task RemoveInterviewerMappingAsync(CandidatesResumes customer, Employee role);
        IList<Questions> GetAllQuestions(int typeId);
        Task InsertCandiatesResultAsync(CandidatesResult candiatesResumes);
        Task<CandidatesResult> GetCandiatesresultByIdAsync(int candiatesId);
        Task<IList<CandidatesResult>> GetByConditionIdAsync(int conditionId);
        IList<CandidatesResult> GetAllResult();
        IList<Questions> GetAllScreeningQuestions(int typeId);
    }
}