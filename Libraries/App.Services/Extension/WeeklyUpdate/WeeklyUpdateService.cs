using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.Projects;
using App.Core.Domain.result;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;

namespace App.Services.TimeSheets
{
    /// <summary>
    /// TimeSheet service
    /// </summary>
    public partial class WeeklyUpdateService : IWeeklyUpdateService
    {
        #region Fields

        private readonly IRepository<WeeklyReports> _Weeklyupdaterepository;
        #endregion

        #region Ctor

        public WeeklyUpdateService(IRepository<WeeklyReports> weeklyupdaterepository)
        {
            _Weeklyupdaterepository = weeklyupdaterepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all timeSheet
        /// </summary>
        /// <param name="timeSheetName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<WeeklyReports>> GetAllWeeklyUpdateAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _Weeklyupdaterepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<WeeklyReports>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get timeSheet by id
        /// </summary>
        /// <param name="timeSheetId"></param>
        /// <returns></returns>
        public virtual async Task<WeeklyReports> GetWeeklyUpdateByIdAsync(int id)
        {
            return await _Weeklyupdaterepository.GetByIdAsync(id, cache => default);
        }
        public async Task<IList<WeeklyReports>> GetByreportIdAsync(int Eid)
        {
            return await _Weeklyupdaterepository.Table
                                   .Where(c => c.EmployeeId == Eid)
                                   .ToListAsync();
        }

        /// <summary>
        /// Get timeSheet by ids
        /// </summary>
        /// <param name="timeSheetIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<WeeklyReports>> GetWeeklyUpdateByIdsAsync(int[] ints)
        {
            return await _Weeklyupdaterepository.GetByIdsAsync(ints, cache => default, false);
        }

        /// <summary>
        /// Insert timeSheet
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        public virtual async Task InsertWeeklyUpdateAsync(WeeklyReports weeklyReports)
        {
            await _Weeklyupdaterepository.InsertAsync(weeklyReports);
        }

        /// <summary>
        /// Update timeSheet
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateWeeklyUpdateAsync(WeeklyReports weeklyReports)
        {
            if (weeklyReports == null)
                throw new ArgumentNullException(nameof(weeklyReports));

            await _Weeklyupdaterepository.UpdateAsync(weeklyReports);
        }

        /// <summary>
        /// delete timeSheet by record
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        public virtual async Task DeleteWeeklyUpdateAsync(WeeklyReports weeklyReports)
        {
            await _Weeklyupdaterepository.DeleteAsync(weeklyReports, false);
        }
        #endregion
    }
}