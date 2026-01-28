using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// UpdateQuestionOption service interface
    /// </summary>
    public partial interface IUpdateQuestionOptionService
    {
        Task<IList<UpdateQuestionOption>> GetByQuestionIdAsync(int questionId);
        Task<IPagedList<UpdateQuestionOption>> GetOptionByQuestionIdAsync(int UpdateTemplateQuestionId, int pageIndex, int pageSize);
        Task InsertAsync(UpdateQuestionOption option);
        Task UpdateAsync(UpdateQuestionOption option);
        Task DeleteAsync(UpdateQuestionOption option);
        Task<UpdateQuestionOption> GetByIdAsync(int questionId);
    }
}