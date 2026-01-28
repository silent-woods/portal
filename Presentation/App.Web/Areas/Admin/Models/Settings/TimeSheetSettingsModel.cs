using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record TimeSheetSettingsModel : BaseNopModel, ISettingsModel
    {
        public TimeSheetSettingsModel() {
            SelectedDepartmentIds = new List<int>();
            AvailableDepartments = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Reminder1_From")]

        public DateTime? Reminder1_From { get; set; }


        public bool Reminder1_From_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Reminder1_To")]
        public DateTime? Reminder1_To { get; set; }

        public bool Reminder1_To_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Reminder2_From")]
        public DateTime? Reminder2_From { get; set; }

        public bool Reminder2_From_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Reminder2_To")]
        public DateTime? Reminder2_To { get; set; }

        public bool Reminder2_To_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.SendEmailToAllProjectLeaders")]

        public bool SendEmailToAllProjectLeaders { get; set; }
        public bool SendEmailToAllProjectLeaders_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.SendEmailToHr")]
        public bool SendEmailToHr { get; set; }
        public bool SendEmailToHr_OverrideForStore { get; set; }
       

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.SendEmailToAllManager")]
        public bool SendEmailToAllProjectManager { get; set; }
        public bool SendEmailToAllProjectManager_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.SendEmailToEmployeeManager")]
        public bool SendEmailToEmployeeManager { get; set; }
        public bool SendEmailToEmployeeManager_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.CommonEmails")]
        [RegularExpression(@"^([\w\.\-]+@[\w\-]+\.[a-zA-Z]{2,5};)*[\w\.\-]+@[\w\-]+\.[a-zA-Z]{2,5}$", ErrorMessage = "Invalid Email Address Format. Please use semicolons to separate multiple emails.")]
        public string CommonEmails { get; set; }

        public bool CommonEmails_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.SelectedDepartmentIds")]
        public IList<int> SelectedDepartmentIds { get; set; }

        public bool SelectedDepartmentIds_OverrideForStore { get; set; }

        public string DepartmentIds { get; set; }

        public bool DepartmentIds_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.ConsiderBeforeDay")]
        public int ConsiderBeforeDay { get; set; }

        public bool ConsiderBeforeDay_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TimeSheet.Email.SendWithCCAfterDay")]
        public int SendWithCCAfterDay { get; set; }

        public bool SendWithCCAfterDay_OverrideForStore { get; set; }


        public IList<SelectListItem> AvailableDepartments { get; set; }





    }
}
