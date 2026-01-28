using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Extension.WeeklyQuestions;
using App.Core.Domain.TimeSheets;

namespace App.Services.TimeSheets
{
    /// <summary>
    /// TimeSheet service interface
    /// </summary>
    public partial interface IWeeklyUpdateService
    {
        /// <summary>
        /// Gets all TimeSheet
        /// </summary>
        //Task<IPagedList<TimeSheet>> GetWeeklyUpdateSheetAsync(string employeeName, string projectName,
        //    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get TimeSheet by id
        /// </summary>
        /// <param name="timesheetId"></param>
        /// <returns></returns>
        Task<WeeklyReports> GetWeeklyUpdateByIdAsync(int id);

        /// <summary>
        /// Insert timeSheet
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        Task InsertWeeklyUpdateAsync(WeeklyReports weeklyReports);
        Task<IList<WeeklyReports>> GetByreportIdAsync(int Eid);

        /// <summary>
        /// Update timeSheet
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        Task UpdateWeeklyUpdateAsync(WeeklyReports weeklyReports);

        /// <summary>
        /// Delete timeSheet
        /// </summary>
        /// <param name="timeSheet"></param>
        /// <returns></returns>
        Task<IPagedList<WeeklyReports>> GetAllWeeklyUpdateAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
    }
}