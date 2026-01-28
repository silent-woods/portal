using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Leaves;
using App.Data;
using App.Data.Extensions;
using StackExchange.Redis;

namespace App.Services.Leaves
{
    /// <summary>
    /// Leavetype service
    /// </summary>
    public partial class LeaveTypeService : ILeaveTypeService
    {
        #region Fields

        private readonly IRepository<Leave> _leaveTypeRepository;

        #endregion

        #region Ctor

        public LeaveTypeService(IRepository<Leave> leaveTypeRepository
           )
        {
            _leaveTypeRepository = leaveTypeRepository;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<Leave>> GetAllLeaveTypeAsync(string leaveName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _leaveTypeRepository.GetAllAsync(async query =>
            {
                if (!string.IsNullOrWhiteSpace(leaveName))
                    query = query.Where(c => c.Type.Contains(leaveName));

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<Leave>(query.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<Leave> GetLeaveTypeByIdAsync(int leaveTypeId)
        {
            return await _leaveTypeRepository.GetByIdAsync(leaveTypeId, cache => default);
        }

        public virtual async Task<IList<Leave>> GetLeaveTypesByIdsAsync(int[] leaveTypeIds)
        {
            return await _leaveTypeRepository.GetByIdsAsync(leaveTypeIds, cache => default, false);
        }

        public virtual async Task InsertLeaveTypeAsync(Leave leavesType)
        {
            await _leaveTypeRepository.InsertAsync(leavesType);
        }

        public virtual async Task UpdateLeaveTypeAsync(Leave leavesType)
        {
            if (leavesType == null)
                throw new ArgumentNullException(nameof(leavesType));

            await _leaveTypeRepository.UpdateAsync(leavesType);
        }

        public virtual async Task DeleteLeaveTypeAsync(Leave leavesType)
        {
            await _leaveTypeRepository.DeleteAsync(leavesType, false);
        }

        public async Task<IList<Leave>> GetLeaveTypesAsync()
        {
            return await _leaveTypeRepository.Table.ToListAsync();
        }

        #endregion
    }
}