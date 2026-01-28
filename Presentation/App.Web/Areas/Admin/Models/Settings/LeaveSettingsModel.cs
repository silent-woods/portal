using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record LeaveSettingsModel : BaseNopModel, ISettingsModel
    {

        public LeaveSettingsModel()
        {
            AvailableLeaveType = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.Email.HrEmail")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string HrEmail { get; set; }

        public bool HrEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.Email.SendEmailToAllProjectLeaders")]

        public bool SendEmailToAllProjectLeaders { get; set; }
        public bool SendEmailToAllProjectLeaders_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.Email.SendEmailToAllManager")]



        public bool SendEmailToAllProjectManager { get; set; }
        public bool SendEmailToAllProjectManager_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.Email.SendEmailToEmployeeManager")]
        public bool SendEmailToEmployeeManager { get; set; }
        public bool SendEmailToEmployeeManager_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.Email.CommonEmails")]
        [RegularExpression(@"^([\w\.\-]+@[\w\-]+\.[a-zA-Z]{2,5};)*[\w\.\-]+@[\w\-]+\.[a-zA-Z]{2,5}$", ErrorMessage = "Invalid Email Address Format. Please use semicolons to separate multiple emails.")]
        public string CommonEmails { get; set; }

        public bool CommonEmails_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.Email.SeletedLeaveTypeId")]

        public int SeletedLeaveTypeId { get; set; }

        public bool SeletedLeaveTypeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Leave.monthlyLeave.AddMonthlyLeaveDay")]

        public int AddMonthlyLeaveDay { get; set; }

        public bool AddMonthlyLeaveDay_OverrideForStore { get; set; }


        public string LastUpdateBalance { get; set; }
        
        public bool LastUpdateBalance_OverrideForStore { get; set; }
        [UIHint("DateNullable")]
		[NopResourceDisplayName("Admin.Configuration.Settings.Leave.monthlyLeave.LeaveTestDate")]

		public DateTime? LeaveTestDate { get; set; }
        public bool LeaveTestDate_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableLeaveType { get; set; }



    }
}
