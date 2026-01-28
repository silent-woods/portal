using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Customers;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace App.Services.Helpers
{
    /// <summary>
    /// Represents a datetime helper
    /// </summary>
    public partial class DateTimeHelper : IDateTimeHelper
    {
        #region Fields

        private readonly DateTimeSettings _dateTimeSettings;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public DateTimeHelper(DateTimeSettings dateTimeSettings,
            IWorkContext workContext)
        {
            _dateTimeSettings = dateTimeSettings;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Retrieves a System.TimeZoneInfo object from the registry based on its identifier.
        /// </summary>
        /// <param name="id">The time zone identifier, which corresponds to the System.TimeZoneInfo.Id property.</param>
        /// <returns>A System.TimeZoneInfo object whose identifier is the value of the id parameter.</returns>
        protected virtual TimeZoneInfo FindTimeZoneById(string id)
        {
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a sorted collection of all the time zones
        /// </summary>
        /// <returns>A read-only collection of System.TimeZoneInfo objects.</returns>
        public virtual ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones();
        }

        /// <summary>
        /// Converts the date and time to current user date and time
        /// </summary>
        /// <param name="dt">The date and time (represents local system time or UTC time) to convert.</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a DateTime value that represents time that corresponds to the dateTime parameter in customer time zone.
        /// </returns>
        public virtual async Task<DateTime> ConvertToUserTimeAsync(DateTime dt)
        {

            return await ConvertToUserTimeAsync(dt, dt.Kind);
        }
        public virtual async Task<DateTime> GetIndianTimeAsync()
        {
            //        var dateUTC = DateTime.UtcNow; // Get UTC time
            //    var localdate = DateTime.Now; // Get server's local time
            //    var date = dateUTC;

            //    // Calculate the difference between local time and UTC time
            //    var difference = localdate - dateUTC;

            //        // Check if server's local time is UTC-6 (6 hours behind UTC)
            //        if (Math.Abs(difference.TotalHours) >= 5.9 && Math.Abs(difference.TotalHours) <= 6.1)
            //        {
            //            // If the time difference is within the range of UTC-6, adjust for IST manually
            //            date = localdate.AddHours(5).AddMinutes(30);
            //}
            //        else
            //        {
            //            // Convert UTC to IST if it's not UTC-6
            //            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            //date = TimeZoneInfo.ConvertTimeFromUtc(dateUTC, istTimeZone);
            //        }
            //        return date;


            var dateUTC = DateTime.UtcNow;
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
          var indianTime = TimeZoneInfo.ConvertTimeFromUtc(dateUTC, istTimeZone);

            return indianTime;
        }

        public virtual async Task<DateTime> GetUTCAsync()
        {
            // var indianTime = await GetIndianTimeAsync();


            //var utc = indianTime.AddHours(-5).AddMinutes(-30);

            // return utc;

            var dateUTC = DateTime.UtcNow;

            return dateUTC;
        }

        /// <summary>
        /// Converts the date and time to current user date and time
        /// </summary>
        /// <param name="dt">The date and time (represents local system time or UTC time) to convert.</param>
        /// <param name="sourceDateTimeKind">The source datetimekind</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains a DateTime value that represents time that corresponds to the dateTime parameter in customer time zone.
        /// </returns>
        public virtual async Task<DateTime> ConvertToUserTimeAsync(DateTime dt, DateTimeKind sourceDateTimeKind)
        {
            dt = DateTime.SpecifyKind(dt, sourceDateTimeKind);
            if (sourceDateTimeKind == DateTimeKind.Local && TimeZoneInfo.Local.IsInvalidTime(dt))
                return dt;

            var currentUserTimeZoneInfo = await GetCurrentTimeZoneAsync();
            return TimeZoneInfo.ConvertTime(dt, currentUserTimeZoneInfo);
        }

        /// <summary>
        /// Converts the date and time to current user date and time
        /// </summary>
        /// <param name="dt">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of dateTime.</param>
        /// <param name="destinationTimeZone">The time zone to convert dateTime to.</param>
        /// <returns>A DateTime value that represents time that corresponds to the dateTime parameter in customer time zone.</returns>
        public virtual DateTime ConvertToUserTime(DateTime dt, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone)
        {
            if (sourceTimeZone.IsInvalidTime(dt))
                return dt;

            return TimeZoneInfo.ConvertTime(dt, sourceTimeZone, destinationTimeZone);
        }

        /// <summary>
        /// Converts the date and time to Coordinated Universal SpentHours (UTC)
        /// </summary>
        /// <param name="dt">The date and time (represents local system time or UTC time) to convert.</param>
        /// <returns>A DateTime value that represents the Coordinated Universal SpentHours (UTC) that corresponds to the dateTime parameter. The DateTime value's Kind property is always set to DateTimeKind.Utc.</returns>
        public virtual DateTime ConvertToUtcTime(DateTime dt)
        {
            return ConvertToUtcTime(dt, dt.Kind);
        }

        /// <summary>
        /// Converts the date and time to Coordinated Universal SpentHours (UTC)
        /// </summary>
        /// <param name="dt">The date and time (represents local system time or UTC time) to convert.</param>
        /// <param name="sourceDateTimeKind">The source datetimekind</param>
        /// <returns>A DateTime value that represents the Coordinated Universal SpentHours (UTC) that corresponds to the dateTime parameter. The DateTime value's Kind property is always set to DateTimeKind.Utc.</returns>
        public virtual DateTime ConvertToUtcTime(DateTime dt, DateTimeKind sourceDateTimeKind)
        {
            dt = DateTime.SpecifyKind(dt, sourceDateTimeKind);
            if (sourceDateTimeKind == DateTimeKind.Local && TimeZoneInfo.Local.IsInvalidTime(dt))
                return dt;

            return TimeZoneInfo.ConvertTimeToUtc(dt);
        }

        /// <summary>
        /// Converts the date and time to Coordinated Universal SpentHours (UTC)
        /// </summary>
        /// <param name="dt">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of dateTime.</param>
        /// <returns>A DateTime value that represents the Coordinated Universal SpentHours (UTC) that corresponds to the dateTime parameter. The DateTime value's Kind property is always set to DateTimeKind.Utc.</returns>
        public virtual DateTime ConvertToUtcTime(DateTime dt, TimeZoneInfo sourceTimeZone)
        {
            if (sourceTimeZone.IsInvalidTime(dt))
            {
                //could not convert
                return dt;
            }

            return TimeZoneInfo.ConvertTimeToUtc(dt, sourceTimeZone);
        }

        /// <summary>
        /// Gets a customer time zone
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer time zone; if customer is null, then default store time zone
        /// </returns>
        public virtual Task<TimeZoneInfo> GetCustomerTimeZoneAsync(Customer customer)
        {
            if (!_dateTimeSettings.AllowCustomersToSetTimeZone)
                return Task.FromResult(DefaultStoreTimeZone);

            TimeZoneInfo timeZoneInfo = null;

            var timeZoneId = string.Empty;
            if (customer != null)
                timeZoneId = customer.TimeZoneId;

            try
            {
                if (!string.IsNullOrEmpty(timeZoneId))
                    timeZoneInfo = FindTimeZoneById(timeZoneId);
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return Task.FromResult(timeZoneInfo ?? DefaultStoreTimeZone);
        }

        /// <summary>
        /// Gets the current user time zone
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the current user time zone
        /// </returns>
        public virtual async Task<TimeZoneInfo> GetCurrentTimeZoneAsync()
        {
           return await GetCustomerTimeZoneAsync(await _workContext.GetCurrentCustomerAsync());
        }

        /// <summary>
        /// Gets or sets a default store time zone
        /// </summary>
        public virtual TimeZoneInfo DefaultStoreTimeZone
        {
            get
            {
                TimeZoneInfo timeZoneInfo = null;
                try
                {
                    if (!string.IsNullOrEmpty(_dateTimeSettings.DefaultStoreTimeZoneId))
                        timeZoneInfo = FindTimeZoneById(_dateTimeSettings.DefaultStoreTimeZoneId);
                }
                catch (Exception exc)
                {
                    Debug.Write(exc.ToString());
                }

                return timeZoneInfo ?? TimeZoneInfo.Local;
            }
        }

        #endregion
    }
}