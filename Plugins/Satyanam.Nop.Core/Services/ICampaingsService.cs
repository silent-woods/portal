using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Campaings service interface
    /// </summary>
    public partial interface ICampaingsService
    {
        Task<IPagedList<Campaings>> GetAllCampaingsAsync(string name, int? statusid,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<Campaings> GetCampaingsByIdAsync(int id);
        Task InsertCampaingsAsync(Campaings campaings);
        Task UpdateCampaingsAsync(Campaings campaings);
        Task DeleteCampaingsAsync(Campaings campaings);
        Task<IList<Campaings>> GetCampaingsByIdsAsync(int[] campaingsIds);
       
    }
}