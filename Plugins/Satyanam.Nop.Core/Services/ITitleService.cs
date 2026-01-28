using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service interface
    /// </summary>
    public partial interface ITitleService
    {
        Task<IPagedList<Title>> GetAllTitleAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Title> GetTitleByIdAsync(int id);
        Task InsertTitleAsync(Title title);
        Task UpdateTitleAsync(Title title);
        Task DeleteTitleAsync(Title title);
        Task<IList<Title>> GetTitleByIdsAsync(int[] titleIds);
        Task<IPagedList<Title>> GetAllTitleByNameAsync(string titleName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<Title> GetOrCreateTitleByNameAsync(string titleName);
        Task<bool> TitleExistsAsync(string name, int excludeId = 0);
    }
}