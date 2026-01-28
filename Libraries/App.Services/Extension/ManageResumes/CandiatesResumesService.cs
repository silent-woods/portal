using App.Core;
using App.Core.Caching;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.result;
using App.Data;
using App.Data.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.ManageResumes
{
    /// <summary>
    /// Projects service
    /// </summary>
    public partial class CandiatesResumesService : ICandiatesResumesService
    {
        #region Fields

        private readonly IRepository<CandidatesResumes> _candiatesresumesrepository;
        private readonly IRepository<CandidateInterviewerMapping> _candidateinteviewermappingrepository;
        private readonly IRepository<Employee> _employeerepository;
        private readonly IRepository<Questions> _questionrepository;
        private readonly IRepository<CandidatesResult> _candidateresult;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public CandiatesResumesService(IRepository<CandidatesResumes> candiatesresumesrepository, IStaticCacheManager staticCacheManager, IRepository<Employee> employeerepository, IRepository<CandidateInterviewerMapping> candidateinteviewermappingrepository, IRepository<Questions> questionrepository, IRepository<CandidatesResult> candidateresult)
        {
            _candiatesresumesrepository = candiatesresumesrepository;
            _staticCacheManager = staticCacheManager;
            _employeerepository = employeerepository;
            _candidateinteviewermappingrepository = candidateinteviewermappingrepository;
            _questionrepository = questionrepository;
            _candidateresult = candidateresult;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<CandidatesResumes>> GetAllCandiatesResumesAsync(string firstName, string lastName, string email, string mobileno, string degree, int appliedForId, int statusId, DateTime? TechnicalRoundDate, DateTime? PracticalRoundDate, int[] InterviewrIds = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _candiatesresumesrepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(firstName))
                    query = query.Where(e => e.FirstName.Contains(firstName));
                if (!string.IsNullOrWhiteSpace(lastName))
                    query = query.Where(e => e.LastName.Contains(lastName));
                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(e => e.Email.Contains(email));
                if (!string.IsNullOrWhiteSpace(mobileno))
                    query = query.Where(e => e.MobileNo.Contains(mobileno));
                if (!string.IsNullOrWhiteSpace(degree))
                    query = query.Where(e => e.Degree.Contains(degree));
                if (appliedForId > 0)
                    query = query.Where(c => appliedForId == c.AppliedForId);
                if (statusId > 0)
                    query = query.Where(c => statusId == c.StatusId);
                //if (InterviewerId > 0)
                //    query = query.Where(c => InterviewerId == c.EmployeeId);

                if (InterviewrIds != null && InterviewrIds.Length > 0)
                {
                    query = query.Join(_candidateinteviewermappingrepository.Table, x => x.Id, y => y.CandidatesId,
                            (x, y) => new { Customer = x, Mapping = y })
                        .Where(z => InterviewrIds.Contains(z.Mapping.EmployeeId))
                        .Select(z => z.Customer)
                        .Distinct();
                }
                if (PracticalRoundDate.HasValue)
                    query = query.Where(c => PracticalRoundDate.Value <= c.PracticalRoundDate);
                if (TechnicalRoundDate.HasValue)
                    query = query.Where(c => TechnicalRoundDate.Value >= c.TechnicalRoundDate);

                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<CandidatesResumes>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="candiatesId"></param>
        /// <returns></returns>
        public virtual async Task<CandidatesResumes> GetCandiatesResumesByIdAsync(int candiatesId)
        {
            return await _candiatesresumesrepository.GetByIdAsync(candiatesId, cache => default);
        }
        public virtual async Task<CandidatesResult> GetCandiatesresultByIdAsync(int candiatesId)
        {
            return await _candidateresult.GetByIdAsync(candiatesId, cache => default);
        }

        public async Task<IList<CandidatesResult>> GetByConditionIdAsync(int conditionId)
        {
            return await _candidateresult.Table
                                   .Where(c => c.CandidateId == conditionId)
                                   .ToListAsync();
        }

        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="candiatesId"></param>
        /// <returns></returns>
        public virtual async Task<IList<CandidatesResumes>> GetCandiatesResumesByIdsAsync(int[] candiatesId)
        {
            return await _candiatesresumesrepository.GetByIdsAsync(candiatesId, cache => default, false);
        }

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task InsertCandiatesResumesAsync(CandidatesResumes candiatesResumes)
        {
            await _candiatesresumesrepository.InsertAsync(candiatesResumes);
        }

        public virtual async Task InsertCandiatesResultAsync(CandidatesResult candiatesResumes)
        {
            await _candidateresult.InsertAsync(candiatesResumes);
        }

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateCandiatesResumesAsync(CandidatesResumes candiatesResumes)
        {
            if (candiatesResumes == null)
                throw new ArgumentNullException(nameof(candiatesResumes));

            await _candiatesresumesrepository.UpdateAsync(candiatesResumes);
        }

        /// <summary>
        /// delete project by record
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task DeleteCandiatesResumesAsync(CandidatesResumes candiatesResumes)
        {
            await _candiatesresumesrepository.DeleteAsync(candiatesResumes, false);
        }


        public virtual async Task<IList<Employee>> GetCustomerRoleBySystemNameAsync(CandidatesResumes employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));
            return await _employeerepository.GetAllAsync(query =>
            {
                return from cr in query
                       join crm in _candidateinteviewermappingrepository.Table on cr.Id equals crm.EmployeeId
                       where crm.CandidatesId == employee.Id
                       select cr;

            });
        }

        public virtual async Task<int[]> GetEmployeeIdsAsync(CandidatesResumes customer, bool showHidden = false)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));
            var query = from cr in _employeerepository.Table
                        join crm in _candidateinteviewermappingrepository.Table on cr.Id equals crm.EmployeeId
                        where crm.CandidatesId == customer.Id
                        select cr.Id;

            return query.ToArray();

        }
        public async Task AddCustomerRoleMappingAsync(CandidateInterviewerMapping roleMapping)
        {
            await _candidateinteviewermappingrepository.InsertAsync(roleMapping);
        }

        public async Task RemoveInterviewerMappingAsync(CandidatesResumes customer, Employee role)
        {
            if (customer is null)
                throw new ArgumentNullException(nameof(customer));

            if (role is null)
                throw new ArgumentNullException(nameof(role));

            var mapping = await _candidateinteviewermappingrepository.Table
                .SingleOrDefaultAsync(ccrm => ccrm.CandidatesId == customer.Id && ccrm.EmployeeId == role.Id);

            if (mapping != null)
                await _candidateinteviewermappingrepository.DeleteAsync(mapping);
        }

        public IList<Questions> GetAllQuestions(int typeId)
        {
            return _questionrepository.Table.Where(q => q.QuestionTypeId == typeId).ToList();
        }

        public IList<CandidatesResult> GetAllResult()
        {
            return _candidateresult.Table.ToList();
        }
        public IList<Questions> GetAllScreeningQuestions(int typeId)
        {

            return _questionrepository.Table.Where(q => q.QuestionTypeId == typeId).ToList();

        }

        #endregion
    }

}