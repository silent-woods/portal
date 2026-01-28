using App.Core.Domain.Holidays;
using App.Web.Areas.Admin.Models.Holidays;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the holiday model factory
    /// </summary>
    public partial interface IHolidayModelFactory
    {
        /// <summary>
        /// Prepare holiday search model
        /// </summary>
        /// <param name="searchModel">Holiday search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday search model
        /// </returns>
        Task<HolidaySearchModel> PrepareHolidaySearchModelAsync(HolidaySearchModel searchModel);

        /// <summary>
        /// Prepare paged holiday list model
        /// </summary>
        /// <param name="searchModel">Holiday search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday list model
        /// </returns>
        Task<HolidayListModel> PrepareHolidayListModelAsync(HolidaySearchModel searchModel);

        /// <summary>
        /// Prepare holiday model
        /// </summary>
        /// <param name="model">Holiday model</param>
        /// <param name="holiday">Holiday</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday model
        /// </returns>
        Task<HolidayModel> PrepareHolidayModelAsync(HolidayModel model, Holiday holiday, bool excludeProperties = false);

    }
}