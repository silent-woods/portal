using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Holidays

{
    /// <summary>
    /// Represents a holiday search model
    /// </summary>
    public partial record HolidaySearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Holiday.Fields.SearchHolidayName")]
        public string SearchHolidayName { get; set; }

        #endregion
    }
}