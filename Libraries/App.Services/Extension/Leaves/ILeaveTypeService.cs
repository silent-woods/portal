using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Leaves;
using StackExchange.Redis;

namespace App.Services.Leaves
{
    public partial interface ILeaveTypeService
    {
        /// <summary>
        /// Gets all LeavesType
        /// </summary>
        Task<IPagedList<Leave>> GetAllLeaveTypeAsync(string leaveName,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<Leave> GetLeaveTypeByIdAsync(int leaveTypeId);

        Task InsertLeaveTypeAsync(Leave leavesType);

        Task UpdateLeaveTypeAsync(Leave leavesType);

        Task DeleteLeaveTypeAsync(Leave leavesType);

        Task<IList<Leave>> GetLeaveTypesByIdsAsync(int[] leaveTypeIds);

        Task<IList<Leave>> GetLeaveTypesAsync();

    }
}