using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.ActivityEvents;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Designations;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial class LeadAPIService : ILeadAPIService
    {
        #region Fields
        
        protected readonly IRepository<LeadAPILog> _leadAPILogRepository;

        #endregion

        #region Ctor

        public LeadAPIService(
            IRepository<LeadAPILog> leadAPILogRepository)
        {
            _leadAPILogRepository = leadAPILogRepository;
        }

        #endregion



        #region Lead API Log Methods

        public virtual async Task InsertLeadAPILogAsync(LeadAPILog leadAPILog)
        {
            ArgumentNullException.ThrowIfNull(nameof(leadAPILog));

            await _leadAPILogRepository.InsertAsync(leadAPILog);
        }

        public virtual async Task<IPagedList<LeadAPILog>> SearchLeadAPILogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var leadAPILogs = from tal in _leadAPILogRepository.Table
                              select tal;

            if (createdFromUtc.HasValue)
                leadAPILogs = leadAPILogs.Where(tal => createdFromUtc.Value <= tal.CreatedOnUtc);

            if (createdToUtc.HasValue)
                leadAPILogs = leadAPILogs.Where(tal => createdToUtc.Value >= tal.CreatedOnUtc);

            leadAPILogs = leadAPILogs.OrderByDescending(tal => tal.Id);

            return await leadAPILogs.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task DeleteLeadAPILogsAsync(IList<LeadAPILog> leadAPILogs)
        {
            await _leadAPILogRepository.DeleteAsync(leadAPILogs);
        }

        public virtual async Task<IList<LeadAPILog>> GetLeadAPILogByIdsAsync(int[] leadAPILogIds)
        {
            return await _leadAPILogRepository.GetByIdsAsync(leadAPILogIds);
        }

        public virtual async Task ClearLeadAPILogAsync()
        {
            await _leadAPILogRepository.TruncateAsync();
        }

        #endregion


    }
}
