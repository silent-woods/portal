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
    public partial class UpdateTemplateQuestionService : IUpdateTemplateQuestionService
    {
        #region Fields

        private readonly IRepository<UpdateTemplateQuestion> _updateTemplateQuestionRepository;

        #endregion

        #region Ctor

        public UpdateTemplateQuestionService(IRepository<UpdateTemplateQuestion> updateTemplateQuestionRepository)
        {
            _updateTemplateQuestionRepository = updateTemplateQuestionRepository;
        }

        #endregion

        #region Methods

        #region UpdateTemplateQuestion

        public async Task InsertAsync(UpdateTemplateQuestion updateTemplateQuestion)
        {
            await _updateTemplateQuestionRepository.InsertAsync(updateTemplateQuestion);
        }

        public async Task UpdateAsync(UpdateTemplateQuestion updateTemplateQuestion)
        {
            await _updateTemplateQuestionRepository.UpdateAsync(updateTemplateQuestion);
        }

        public async Task DeleteAsync(UpdateTemplateQuestion updateTemplateQuestion)
        {
            await _updateTemplateQuestionRepository.DeleteAsync(updateTemplateQuestion);
        }
        public virtual async Task<IList<UpdateTemplateQuestion>> GetByIdsAsync(int[] updateTemplateQuestionIds)
        {
            return await _updateTemplateQuestionRepository.GetByIdsAsync(updateTemplateQuestionIds);
        }
        public async Task<UpdateTemplateQuestion> GetByIdAsync(int id)
        {
            return await _updateTemplateQuestionRepository.GetByIdAsync(id);
        }

        public virtual async Task<IPagedList<UpdateTemplateQuestion>> GetAllUpdateTemplateQuestionAsync(int UpdateTemplateId,string question = null,
    bool? isRequired = null,int? controlTypeId = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _updateTemplateQuestionRepository.Table
        .Where(x => x.UpdateTemplateId == UpdateTemplateId);

            if (!string.IsNullOrEmpty(question))
                query = query.Where(x => x.QuestionText.Contains(question));

            if (isRequired.HasValue)
                query = query.Where(x => x.IsRequired == isRequired.Value);

            if (controlTypeId.HasValue && controlTypeId > 0)
                query = query.Where(x => x.ControlTypeId == controlTypeId.Value);

            query = query.OrderBy(x => x.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);

        }
        

        #endregion

        #endregion
    }
}