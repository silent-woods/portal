using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// ConnectionRequest service interface
    /// </summary>
    public partial interface IConnectionRequestService
    {
        Task<IPagedList<ConnectionRequest>> GetAllConnectionRequestAsync(string firstname, string lastname,
         string email, string linkedinUrl, string website,int? statusId = null, int pageIndex = 0, int pageSize = int.MaxValue,
         bool showHidden = false, bool? isSyncedToReply = null);
        Task<ConnectionRequest> GetConnectionRequestByIdAsync(int id);
        Task<IList<ConnectionRequest>> GetConnectionRequestByIdsAsync(int[] connectionRequestIds);
        Task InsertConnectionRequestAsync(ConnectionRequest connectionRequest);
        Task UpdateConnectionRequestAsync(ConnectionRequest connectionRequest);
        Task DeleteConnectionRequestAsync(ConnectionRequest connectionRequest);
        Task<IList<ConnectionRequest>> GetConnectionRequestsByIdsAsync(List<int> ids);

    }
}