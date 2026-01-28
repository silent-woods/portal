using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Holidays;
using App.Data;
using App.Data.Extensions;

namespace App.Services.Holidays
{
    /// <summary>
    /// Holiday service
    /// </summary>
    public partial class HolidayService : IHolidayService
    {
        #region Fields

        private readonly IRepository<Holiday> _holidayRepository;       
        //private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public HolidayService(
            IRepository<Holiday> holidayRepository)
        {
            _holidayRepository = holidayRepository;
        }

        #endregion

        #region Methods

        #region Holiday

        /// <summary>
        /// Deletes a Holiday
        /// </summary>
        /// <param name="holiday">Holiday</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteHolidayAsync(Holiday holiday)
        {
            await _holidayRepository.DeleteAsync(holiday);
        }

        /// <summary>
        /// Gets a Holiday
        /// </summary>
        /// <param name="holidayId">holiday identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday
        /// </returns>
        public virtual async Task<Holiday> GetHolidayByIdAsync(int holidayId)
        {
            return await _holidayRepository.GetByIdAsync(holidayId, cache => default);
        }

        public virtual async Task<IList<Holiday>> GetHolidayByIdsAsync(int[] holidayIds)
        {
            return await _holidayRepository.GetByIdsAsync(holidayIds, cache => default, false);
        }

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

        public virtual async Task<IPagedList<Holiday>> GetAllHolidaysAsync(int storeId = 0, int languageId = 0,
            DateTime? dateFrom = null, DateTime? dateTo = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, string holiday = null)
        {
            return await _holidayRepository.GetAllPagedAsync(async query =>
            {

                if (!string.IsNullOrEmpty(holiday))
                    query = query.Where(b => b.Name.Contains(holiday));

                return query;
            }, pageIndex, pageSize);
        }

        /// <summary>
        /// Inserts a holiday
        /// </summary>
        /// <param name="holiday">Holiday</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertHolidayAsync(Holiday holiday)
        {
            await _holidayRepository.InsertAsync(holiday);
        }

        /// <summary>
        /// Updates the holiday
        /// </summary>
        /// <param name="holiday">Holiday</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateHolidayAsync(Holiday holiday)
        {
            await _holidayRepository.UpdateAsync(holiday);
        }

        public async Task<bool> IsHolidayAsync(DateTime date)
        {
            // Fetch all holidays
            var holidays = await GetAllHolidaysAsync();

            // Check if the given date matches any of the holidays
            foreach (var holiday in holidays)
            {
                if (date.Date == holiday.Date.Date)
                {
                    return true; // It's a holiday
                }
            }

            return false; // It's not a holiday
        }
        public async Task<IList<Holiday>> GetHolidaysBetweenAsync(DateTime fromDate, DateTime toDate)
        {
            return await _holidayRepository.Table
                .Where(h => h.Date >= fromDate.Date && h.Date <= toDate.Date)
                .ToListAsync();
        }


        #endregion

        #endregion
    }
}