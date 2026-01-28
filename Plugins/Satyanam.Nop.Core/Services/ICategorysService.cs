using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Categorys service interface
    /// </summary>
    public partial interface ICategorysService
    {
        Task<IPagedList<Categorys>> GetAllCategorysAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Categorys> GetCategorysByIdAsync(int id);
        Task InsertCategorysAsync(Categorys categorys);
        Task UpdateCategorysAsync(Categorys categorys);
        Task DeleteCategorysAsync(Categorys categorys);
        Task<IList<Categorys>> GetCategorysByIdsAsync(int[] categorysIds);
        Task<IPagedList<Categorys>> GetAllCategorysByNameAsync(string categoryName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<Categorys> GetOrCreateCategorysByNameAsync(string categoryName);
    }
}