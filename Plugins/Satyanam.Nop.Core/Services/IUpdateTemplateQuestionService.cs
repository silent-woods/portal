using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateTemplateQuestion service interface
    /// </summary>
    public partial interface IUpdateTemplateQuestionService
    {
        Task InsertAsync(UpdateTemplateQuestion updateTemplateQuestion);
        Task UpdateAsync(UpdateTemplateQuestion updateTemplateQuestion);
        Task DeleteAsync(UpdateTemplateQuestion updateTemplateQuestion);
        Task<IList<UpdateTemplateQuestion>> GetByIdsAsync(int[] updateTemplateQuestionIds);
        Task<UpdateTemplateQuestion> GetByIdAsync(int id);
        Task<IPagedList<UpdateTemplateQuestion>> GetAllUpdateTemplateQuestionAsync(int UpdateTemplateId, string question = null,
    bool? isRequired = null, int? controlTypeId = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}