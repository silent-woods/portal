using App.Core;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateTemplate service
    /// </summary>
    public partial class UpdateQuestionOptionService : IUpdateQuestionOptionService
    {
        #region Fields

        private readonly IRepository<UpdateQuestionOption> _optionRepository;

        #endregion

        #region Ctor

        public UpdateQuestionOptionService(IRepository<UpdateQuestionOption> optionRepository)
        {
            _optionRepository = optionRepository;
        }

        #endregion

        #region Methods

        #region UpdateQuestionOption

        public async Task<IList<UpdateQuestionOption>> GetByQuestionIdAsync(int questionId)
        {
            return await _optionRepository.Table
                .Where(x => x.UpdateTemplateQuestionId == questionId)
                .OrderBy(x => x.DisplayOrder)
                .ToListAsync();
        }
        public virtual async Task<UpdateQuestionOption> GetByIdAsync(int questionId)
        {
            return await _optionRepository.GetByIdAsync(questionId);
        }
        public async Task<IPagedList<UpdateQuestionOption>> GetOptionByQuestionIdAsync(int UpdateTemplateQuestionId, int pageIndex, int pageSize)
        {
            var query = _optionRepository.Table.Where(x => x.UpdateTemplateQuestionId == UpdateTemplateQuestionId);
            return await query.ToPagedListAsync(pageIndex, pageSize);
            
        }

        public async Task InsertAsync(UpdateQuestionOption option)
        {
            await _optionRepository.InsertAsync(option);
        }
        public virtual async Task UpdateAsync(UpdateQuestionOption option)
        {
            await _optionRepository.UpdateAsync(option);
        }
        public async Task DeleteAsync(UpdateQuestionOption option)
        {
            await _optionRepository.DeleteAsync(option);
        }

        #endregion

        #endregion
    }
}