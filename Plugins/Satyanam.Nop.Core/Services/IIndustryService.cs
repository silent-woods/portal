using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Industry service interface
    /// </summary>
    public partial interface IIndustryService
    {
        Task<IPagedList<Industry>> GetAllIndustryAsync(string name, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);
        Task<Industry> GetIndustryByIdAsync(int id);
        Task InsertIndustryAsync(Industry industry);
        Task UpdateIndustryAsync(Industry industry);
        Task DeleteIndustryAsync(Industry industry);
        Task<IList<Industry>> GetIndustryByIdsAsync(int[] industryIds);
        Task<IPagedList<Industry>> GetAllIndustryByNameAsync(string industryName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<Industry> GetOrCreateIndustryByNameAsync(string industryName);
    }
}