using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Holidays

{
    /// <summary>
    /// Represents a holiday model
    /// </summary>
    public partial record HolidayModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Holiday.Fields.Name")]
        [Required(ErrorMessage = "Please enter a holiday name.")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Holiday.Fields.Date")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please enter a holiday date.")]
        public DateTime? Date { get; set; }

        [NopResourceDisplayName("Admin.Holiday.Fields.HolidayDate")]
        public string HolidayDate { get; set; }

        [NopResourceDisplayName("Admin.Holiday.Fields.WeekDay")]
        [Required(ErrorMessage = "Please enter a holiday weekday.")]
        public string WeekDay { get; set; }

        #endregion
    }
}