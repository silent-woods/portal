using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateSubmissionComment service interface
    /// </summary>
    public partial interface IUpdateSubmissionCommentService
    {
        Task InsertCommentAsync(UpdateSubmissionComment comment);
        Task<IList<UpdateSubmissionComment>> GetCommentsBySubmissionIdAsync(int submissionId);
    }
}