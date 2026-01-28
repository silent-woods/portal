using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;
namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record EmployeeAttendanceSettingsModel : BaseNopModel, ISettingsModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.EmployeeAttendance.OfficeTime_From")]

        public DateTime? OfficeTime_From { get; set; }
       

        public bool OfficeTime_From_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.EmployeeAttendance.OfficeTime_To")]
        public DateTime? OfficeTime_To { get; set; }

        public bool OfficeTime_To_OverrideForStore { get; set; }



    }
}
