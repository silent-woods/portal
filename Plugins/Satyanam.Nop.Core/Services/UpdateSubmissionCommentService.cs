using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateSubmissionComment service
    /// </summary>
    public partial class UpdateSubmissionCommentService : IUpdateSubmissionCommentService
    {
        #region Fields

        private readonly IRepository<UpdateSubmissionComment> _commentRepo;

        #endregion

        #region Ctor

        public UpdateSubmissionCommentService(IRepository<UpdateSubmissionComment> commentRepo)
        {
            _commentRepo = commentRepo;
        }

        #endregion

        #region Methods

        #region UpdateSubmissionComment

        public async Task InsertCommentAsync(UpdateSubmissionComment comment)
        {
            await _commentRepo.InsertAsync(comment);
        }

        public async Task<IList<UpdateSubmissionComment>> GetCommentsBySubmissionIdAsync(int submissionId)
        {
            return await _commentRepo.Table
                .Where(c => c.UpdateSubmissionId == submissionId)
                .OrderBy(c => c.CreatedOnUtc)
                .ToListAsync();
        }

        #endregion

        #endregion
    }
}