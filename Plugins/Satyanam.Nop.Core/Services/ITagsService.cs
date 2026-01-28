using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Tags service interface
    /// </summary>
    public partial interface ITagsService
    {
        Task<IPagedList<Tags>> GetAllTagsAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Tags> GetTagsByIdAsync(int id);
        Task InsertTagsAsync(Tags tags);
        Task UpdateTagsAsync(Tags tags);
        Task DeleteTagsAsync(Tags tags);
        Task<IList<Tags>> GetTagsByIdsAsync(int[] tagsIds);
        Task<Tags> GetOrCreateTagsByNameAsync(string tagsName);
    }
}