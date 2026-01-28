using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Data;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Employees;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateSubmission service
    /// </summary>
    public partial class UpdateSubmissionService : IUpdateSubmissionService
    {
        #region Fields

        private readonly IRepository<UpdateSubmission> _submissionRepository;
        private readonly IRepository<UpdateSubmissionAnswer> _answerRepository;
        private readonly IEmployeeService _employeeService;
        private readonly IDesignationService _designationService;
        private readonly IRepository<UpdateTemplateQuestion> _questionRepository;
        private readonly IRepository<UpdateSubmissionComment> _updateSubmissionCommentRepository;
        #endregion

        #region Ctor

        public UpdateSubmissionService(IRepository<UpdateSubmission> submissionRepository, IRepository<UpdateSubmissionAnswer> answerRepository, IEmployeeService employeeService, IDesignationService designationService, IRepository<UpdateTemplateQuestion> questionRepository, IRepository<UpdateSubmissionComment> updateSubmissionCommentRepository)
        {
            _submissionRepository = submissionRepository;
            _answerRepository = answerRepository;
            _employeeService = employeeService;
            _designationService = designationService;
            _questionRepository = questionRepository;
            _updateSubmissionCommentRepository = updateSubmissionCommentRepository;
        }

        #endregion

        #region Methods

        #region UpdateSubmissionService

        public async Task InsertSubmissionAsync(UpdateSubmission submission, IList<UpdateSubmissionAnswer> answers)
        {
            if (submission == null)
                throw new ArgumentNullException(nameof(submission));

            await _submissionRepository.InsertAsync(submission);

            foreach (var answer in answers)
            {
                answer.UpdateSubmissionId = submission.Id;
                await _answerRepository.InsertAsync(answer);
            }
        }

        public async Task<UpdateSubmission> GetSubmissionByIdAsync(int id)
        {
            return await _submissionRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IList<UpdateSubmission>> GetSubmissionsByCustomerIdAsync(int customerId)
        {
            return await _submissionRepository.GetAllAsync(q => q.Where(x => x.SubmittedByCustomerId == customerId));
        }
        public async Task UpdateSubmissionAsync(UpdateSubmission submission, IList<UpdateSubmissionAnswer> answers)
        {
            if (submission == null)
                throw new ArgumentNullException(nameof(submission));

            await _submissionRepository.UpdateAsync(submission);

            // Delete old answers
            var existingAnswers = await _answerRepository.GetAllAsync(q => q.Where(a => a.UpdateSubmissionId == submission.Id));
            foreach (var existing in existingAnswers)
                await _answerRepository.DeleteAsync(existing);

            // Insert new answers
            foreach (var answer in answers)
            {
                answer.UpdateSubmissionId = submission.Id;
                await _answerRepository.InsertAsync(answer);
            }
        }

        public async Task<IList<(DateTime StartDate, DateTime EndDate)>> GetDistinctSubmissionDates(int updateTemplateId)
        {
            var submissions = await _submissionRepository.GetAllAsync(query =>
                query.Where(s => s.UpdateTemplateId == updateTemplateId)
            );

            var dates = submissions
                .Select(s => s.SubmittedOnUtc.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();

            var result = dates
                .Select(d => (StartDate: d, EndDate: d))
                .ToList();

            return result;
        }

        public async Task<IList<Employee>> GetEmployeesAllowedToViewAsync(int currentEmployeeId)
        {
            var currentEmployee = await _employeeService.GetEmployeeByIdAsync(currentEmployeeId);
            if (currentEmployee == null)
                return new List<Employee>();

            var designation = await _designationService.GetDesignationByIdAsync(currentEmployee.DesignationId);
            var designationName = designation?.Name?.Trim().ToLower() ?? "";

            var elevatedDesignations = new[]
            {
        "admin", "human resource", "qa", "senior developer", "project manager", "project leader"
    };

            var allEmployees = await _employeeService.GetAllEmployeesAsync();

            if (elevatedDesignations.Contains(designationName))
                return allEmployees.OrderBy(e => e.FirstName + " " + e.LastName).ToList();

            var managedEmployees = allEmployees
                .Where(e => e.ManagerId == currentEmployeeId)
                .OrderBy(e => e.FirstName+" "+e.LastName)
                .ToList();

            if (managedEmployees.Any())
                return managedEmployees;

            return new List<Employee> { currentEmployee };
        }


        public async Task<IList<UpdateSubmission>> GetSubmissionsAsync(int updateTemplateId,int? selectedSubmitterId,
         DateTime? fromDate,DateTime? toDate)
        {
            return await _submissionRepository.GetAllAsync(query =>
            {
                // Base query
                query = query.Where(s => s.UpdateTemplateId == updateTemplateId);

                // Submitter filter
                if (selectedSubmitterId.HasValue && selectedSubmitterId.Value > 0)
                {
                    query = query.Where(s => s.SubmittedByCustomerId == selectedSubmitterId.Value);
                }

                // Date range filter
                if (fromDate.HasValue)
                {
                    query = query.Where(s => s.SubmittedOnUtc >= fromDate.Value.Date);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(s => s.SubmittedOnUtc <= toDate.Value.Date.AddDays(1).AddTicks(-1)); // inclusive end of day
                }

                // Order by recent submissions
                return query.OrderByDescending(s => s.SubmittedOnUtc);
            });
        }

        public async Task<IList<UpdateTemplateQuestion>> GetQuestionsByTemplateIdAsync(int templateId)
        {
            var questions = await _questionRepository.GetAllAsync(q =>
                q.Where(x => x.UpdateTemplateId == templateId)
                 .OrderBy(x => x.DisplayOrder));

            return questions ?? new List<UpdateTemplateQuestion>();
        }

        public async Task<IList<UpdateSubmissionComment>> GetCommentsBySubmissionIdAsync(int submissionId)
        {
            if (submissionId <= 0)
                return new List<UpdateSubmissionComment>();

            return await _updateSubmissionCommentRepository.Table
                .Where(c => c.UpdateSubmissionId == submissionId) // ✅ FIXED
                .OrderBy(c => c.CreatedOnUtc)
                .ToListAsync();
        }


        public async Task<UpdateSubmissionComment> GetCommentByIdAsync(int commentId)
        {
            if (commentId <= 0)
                return null;

            return await _updateSubmissionCommentRepository.GetByIdAsync(commentId);
        }

        #endregion

        #endregion
    }
}