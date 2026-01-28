using System;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Framework.Models.Extensions;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Holidays;
using App.Core.Domain.Holidays;
using App.Services.Holidays;
using App.Data.Extensions;

namespace App.Web.Areas.Admin.Factories
{
	/// <summary>
	/// Represents the holiday model factory implementation
	/// </summary>
	public partial class HolidayModelFactory : IHolidayModelFactory
    {
        #region Fields

        private readonly IHolidayService _holidayService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public HolidayModelFactory(
            IHolidayService holidayService,
            IDateTimeHelper dateTimeHelper)
        {
            _holidayService = holidayService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare holiday search model
        /// </summary>
        /// <param name="searchModel">Holiday search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday search model
        /// </returns>
        public virtual async Task<HolidaySearchModel> PrepareHolidaySearchModelAsync(HolidaySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged holiday list model
        /// </summary>
        /// <param name="searchModel">Holiday search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the holiday list model
        /// </returns>
        public virtual async Task<HolidayListModel> PrepareHolidayListModelAsync(HolidaySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get holidays
            var holidays = await _holidayService.GetAllHolidaysAsync(showHidden: true,
                holiday: searchModel.SearchHolidayName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new HolidayListModel().PrepareToGridAsync(searchModel, holidays, () =>
            {
                //fill in model values from the entity
                return holidays.SelectAwait(async holiday =>
                {
                    var holidayModel = holiday.ToModel<HolidayModel>();

                    holidayModel.HolidayDate = holiday.Date.ToString("MM-dd-yyyy");

                    //var holidayModel = new HolidayModel();
                    //holidayModel.Id = holiday.Id;
                    //holidayModel.Name = holiday.Name;
                    //holidayModel.Date = holiday.Date;
                    //holidayModel.WeekDay = holiday.WeekDay;
                    //holidayModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(department.CreatedOnUtc, DateTimeKind.Utc);
                    //holidayModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(department.UpdatedOnUtc, DateTimeKind.Utc);

                    return holidayModel;
                });
            });

            return model;
        }

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
        public virtual async Task<HolidayModel> PrepareHolidayModelAsync(HolidayModel model, Holiday holiday, bool excludeProperties = false)
        {
            if (holiday != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = holiday.ToModel<HolidayModel>();
                    //model.Id = holiday.Id;
                    //model.Name = holiday.Name;
                    //model.Date = holiday.Date;
                    //model.WeekDay = holiday.WeekDay;
                }
            }

            return model;
        }

        #endregion
    }
}
