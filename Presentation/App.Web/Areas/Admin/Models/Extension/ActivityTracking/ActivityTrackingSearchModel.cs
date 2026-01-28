using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.ActivityTracking
{
    public partial record ActivityTrackingSearchModel : BaseSearchModel
    {
        public ActivityTrackingSearchModel() {
            SelectedEmployeeIds = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            PeriodList = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.ActivityTracking.Fields.SearchDate")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please select Date")]
        public DateTime? SearchDate { get; set; }


        [NopResourceDisplayName("Admin.ActivityTracking.Fields.From")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please select From Date")]
        public DateTime? From { get; set; }


        [NopResourceDisplayName("Admin.ActivityTracking.Fields.To")]
        [UIHint("DateNullable")]
        [Required(ErrorMessage = "Please select To Date")]
        public DateTime? To { get; set; }


        [NopResourceDisplayName("Admin.ActivityTracking.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }

        [NopResourceDisplayName("Admin.ActivityTracking.Fields.SelectedEmployeeId")]
        public IList<int> SelectedEmployeeIds { get; set; }

        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.ActivityTracking.Fields.SearchPeriodId")]
        public int SearchPeriodId { get; set; }

        public IList<SelectListItem> PeriodList { get; set; }


        #endregion
    }
}
