using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Candidate service
    /// </summary>
    public partial class CandidatesService : ICandidatesService
    {
        /// <summary>
        /// Candidate service
        /// </summary>

        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<JobApplication> _jobApplicationRepository;

        public CandidatesService(IRepository<Candidate> candidateRepository,
            IRepository<JobApplication> jobApplicationRepository)
        {
            _candidateRepository = candidateRepository;
            _jobApplicationRepository = jobApplicationRepository;
        }

        #region Candidate

        public async Task<Candidate> GetCandidateByIdAsync(int id)
        {
            return await _candidateRepository.GetByIdAsync(id);
        }
        public virtual async Task<IList<Candidate>> GetCandidateByIdsAsync(int[] candidateIds)
        {
            return await _candidateRepository.GetByIdsAsync(candidateIds);
        }
        public async Task<IPagedList<Candidate>> GetAllCandidatesAsync(string name = "", string email = "", string positionApplied = "", int candidatetypeId = 0, int statusId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _candidateRepository.Table;

            // Name filter
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c =>
                    c.FirstName.Contains(name) ||
                    c.LastName.Contains(name) ||
                    (c.FirstName + " " + c.LastName).Contains(name));
            }

            // Email filter
            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(c => c.Email.Contains(email));
            }

            // CandidateType filter
            if (candidatetypeId > 0)
            {
                query = query.Where(c => c.CandidateTypeId == candidatetypeId);
            }

            // PositionApplied filter (CHECK LATEST APPLICATION ONLY)
            if (!string.IsNullOrWhiteSpace(positionApplied))
            {
                query = query.Where(c =>
                    _jobApplicationRepository.Table
                        .Where(j => j.CandidateId == c.Id)
                        .OrderByDescending(j => j.Id)
                        .Select(j => j.PositionApplied)
                        .FirstOrDefault()
                        .Contains(positionApplied));
            }

            // Status filter (CHECK LATEST APPLICATION ONLY)
            if (statusId > 0)
            {
                query = query.Where(c =>
                    _jobApplicationRepository.Table
                        .Where(j => j.CandidateId == c.Id)
                        .OrderByDescending(j => j.Id)
                        .Select(j => j.StatusId)
                        .FirstOrDefault() == statusId);
            }

            query = query.OrderByDescending(c => c.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertCandidateAsync(Candidate candidate)
        {
            await _candidateRepository.InsertAsync(candidate);
        }

        public async Task UpdateCandidateAsync(Candidate candidate)
        {
            await _candidateRepository.UpdateAsync(candidate);
        }

        public async Task DeleteCandidateAsync(Candidate candidate)
        {
            await _candidateRepository.DeleteAsync(candidate);
        }
        public async Task InsertJobApplicationAsync(JobApplication application)
        {
            var existing = await _jobApplicationRepository.Table
                .FirstOrDefaultAsync(x =>
                    x.CandidateId == application.CandidateId &&
                    x.JobPostingId == application.JobPostingId);

            if (existing != null)
                return;

            application.AppliedOnUtc = DateTime.UtcNow;
            application.UpdatedOnUtc = DateTime.UtcNow;

            await _jobApplicationRepository.InsertAsync(application);
        }
        public async Task<IPagedList<Candidate>> GetCandidatesByJobPostingIdNewAsync(int jobPostingId,int pageIndex = 0,int pageSize = int.MaxValue)
        {
            var query =
                from application in _jobApplicationRepository.Table
                join candidate in _candidateRepository.Table
                on application.CandidateId equals candidate.Id
                where application.JobPostingId == jobPostingId
                orderby application.AppliedOnUtc descending
                select candidate;

            var list = await query.ToListAsync();

            return new PagedList<Candidate>(list, pageIndex, pageSize);
        }
        public async Task UpdateJobApplicationStatusAsync(int candidateId,int jobPostingId,int statusId,string hrNotes)
        {
            var application = await GetJobApplicationAsync(candidateId, jobPostingId);

            if (application == null)
                return;

            application.StatusId = statusId;
            application.HrNotes = hrNotes;
            application.UpdatedOnUtc = DateTime.UtcNow;

            await _jobApplicationRepository.UpdateAsync(application);
        }
        public async Task<JobApplication> GetJobApplicationAsync(int candidateId,int jobPostingId)
        {
            return await _jobApplicationRepository.Table
                .FirstOrDefaultAsync(x =>
                    x.CandidateId == candidateId &&
                    x.JobPostingId == jobPostingId);
        }
        public async Task<IList<JobApplication>> GetJobApplicationsByCandidateIdAsync(int candidateId)
        {
            return await _jobApplicationRepository.Table
                .Where(x => x.CandidateId == candidateId)
                .OrderByDescending(x => x.AppliedOnUtc)
                .ToListAsync();
        }
        public async Task<JobApplication> GetJobApplicationByCandidateIdAsync(int candidateId, int jobPostingId)
        {
            if (candidateId <= 0 || jobPostingId <= 0)
                return null;

            return await _jobApplicationRepository.Table
                .Where(x => x.CandidateId == candidateId && x.JobPostingId == jobPostingId)
                .OrderByDescending(x => x.AppliedOnUtc)
                .FirstOrDefaultAsync();
        }
        public async Task<Candidate> GetCandidateByEmailAsync(string email)
        {
            return await _candidateRepository.Table
                .FirstOrDefaultAsync(x => x.Email == email);
        }
        public async Task UpdateJobApplicationAsync(JobApplication application)
        {
            if (application == null)
                throw new ArgumentNullException(nameof(application));

            application.UpdatedOnUtc = DateTime.UtcNow;

            await _jobApplicationRepository.UpdateAsync(application);
        }
        #endregion
    }
}