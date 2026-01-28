using App.Core;
using App.Data;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// ConnectionRequest service
    /// </summary>
    public partial class ConnectionRequestService : IConnectionRequestService
    {
        #region Fields

        private readonly IRepository<ConnectionRequest> _connectionRequestRepository;

        #endregion

        #region Ctor

        public ConnectionRequestService(IRepository<ConnectionRequest> connectionRequestRepository)
        {
            _connectionRequestRepository = connectionRequestRepository;
        }

        #endregion

        #region Methods

        #region ConnectionRequest

        /// <summary>
        /// Gets all ConnectionRequest
        /// </summary>
        /// <param name="name">name</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="connectionRequest">Filter by connectionRequest name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the connectionRequest
        /// </returns>

        public virtual async Task<IPagedList<ConnectionRequest>> GetAllConnectionRequestAsync(string firstname, string lastname,
         string email, string linkedinUrl, string website, int? statusId = null, int pageIndex = 0, int pageSize = int.MaxValue,
         bool showHidden = false, bool? isSyncedToReply = null)
        {
            var query = await _connectionRequestRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(firstname))
                    query = query.Where(c => c.FirstName.Contains(firstname));

                if (!string.IsNullOrWhiteSpace(firstname))
                    query = query.Where(c => c.LastName.Contains(lastname));

                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(c => c.Email.Contains(email));

                if (!string.IsNullOrWhiteSpace(website))
                    query = query.Where(c => c.WebsiteUrl.Contains(website));

                if (!string.IsNullOrWhiteSpace(linkedinUrl))
                    query = query.Where(c => c.LinkedinUrl.Contains(linkedinUrl));

                if (statusId.HasValue)
                    query = query.Where(c => c.StatusId == statusId.Value);

                return query.OrderBy(c => c.Id);
            });
            //paging
            return new PagedList<ConnectionRequest>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Gets a ConnectionRequest
        /// </summary>
        /// <param name="connectionRequestId">connectionRequest identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the connectionRequest
        /// </returns>
        public virtual async Task<ConnectionRequest> GetConnectionRequestByIdAsync(int connectionRequestId)
        {
            return await _connectionRequestRepository.GetByIdAsync(connectionRequestId);
        }

        public virtual async Task<IList<ConnectionRequest>> GetConnectionRequestByIdsAsync(int[] connectionRequestIds)
        {
            return await _connectionRequestRepository.GetByIdsAsync(connectionRequestIds);
        }

        /// <summary>
        /// Inserts a ConnectionRequest
        /// </summary>
        /// <param name="connectionRequest">ConnectionRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertConnectionRequestAsync(ConnectionRequest connectionRequest)
        {
            await _connectionRequestRepository.InsertAsync(connectionRequest);
        }

        /// <summary>
        /// Updates the ConnectionRequest
        /// </summary>
        /// <param name="connectionRequest">ConnectionRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateConnectionRequestAsync(ConnectionRequest connectionRequest)
        {
            await _connectionRequestRepository.UpdateAsync(connectionRequest);
        }

        /// <summary>
        /// Deletes a ConnectionRequest
        /// </summary>
        /// <param name="connectionRequest">ConnectionRequest</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteConnectionRequestAsync(ConnectionRequest connectionRequest)
        {
            await _connectionRequestRepository.DeleteAsync(connectionRequest);
        }

        public async Task<IList<ConnectionRequest>> GetConnectionRequestsByIdsAsync(List<int> ids)
        {
            var list = new List<ConnectionRequest>();
            foreach (var id in ids)
            {
                var item = await GetConnectionRequestByIdAsync(id); // existing method
                if (item != null) list.Add(item);
            }
            return list;
        }

        #endregion

        #endregion
    }
}