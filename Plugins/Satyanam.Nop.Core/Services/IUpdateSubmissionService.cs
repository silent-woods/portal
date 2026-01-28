using App.Core.Domain.Employees;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateSubmission service interface
    /// </summary>
    public partial interface IUpdateSubmissionService
    {
        Task InsertSubmissionAsync(UpdateSubmission submission, IList<UpdateSubmissionAnswer> answers);
        Task<UpdateSubmission> GetSubmissionByIdAsync(int id);
        Task<IList<UpdateSubmission>> GetSubmissionsByCustomerIdAsync(int customerId);
        Task UpdateSubmissionAsync(UpdateSubmission submission, IList<UpdateSubmissionAnswer> answers);

        Task<IList<(DateTime StartDate, DateTime EndDate)>> GetDistinctSubmissionDates(int updateTemplateId);
        Task<IList<Employee>> GetEmployeesAllowedToViewAsync(int currentEmployeeId);
        Task<IList<UpdateSubmission>> GetSubmissionsAsync(int updateTemplateId, int? selectedSubmitterId,
         DateTime? fromDate, DateTime? toDate);

        Task<IList<UpdateTemplateQuestion>> GetQuestionsByTemplateIdAsync(int updateTemplateId);
        Task<IList<UpdateSubmissionComment>> GetCommentsBySubmissionIdAsync(int submissionId);
        Task<UpdateSubmissionComment> GetCommentByIdAsync(int commentId);

    }
}