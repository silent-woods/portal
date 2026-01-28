using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Holidays;

namespace App.Services.Holidays
{
    /// <summary>
    /// Holiday service interface
    /// </summary>
    public partial interface IHolidayService
    {
        /// <summary>
        /// Deletes a Holiday
        /// </summary>
        /// <param name="holiday">Holiday</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteHolidayAsync(Holiday holiday);

        /// <summary>
        /// Gets a Holiday
        /// </summary>
        /// <param name="holidayId">holiday identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday
        /// </returns>
        Task<Holiday> GetHolidayByIdAsync(int holidayId);

        /// <summary>
        /// Gets all Holidays
        /// </summary>
        /// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="holiday">Filter by holiday name</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday
        /// </returns>

        Task<IPagedList<Holiday>> GetAllHolidaysAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string holiday = null);

        /// <summary>
        /// Inserts a holiday
        /// </summary>
        /// <param name="holiday">Holiday</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertHolidayAsync(Holiday holiday);

        /// <summary>
        /// Updates the holiday
        /// </summary>
        /// <param name="holiday">Holiday</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateHolidayAsync(Holiday holiday);

        Task<IList<Holiday>> GetHolidayByIdsAsync(int[] holidayIds);
        Task<bool> IsHolidayAsync(DateTime date);

        Task<IList<Holiday>> GetHolidaysBetweenAsync(DateTime fromDate, DateTime toDate);
    }
}