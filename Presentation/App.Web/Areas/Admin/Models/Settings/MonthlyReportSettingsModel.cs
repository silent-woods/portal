using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record MonthlyReportSettingsModel : BaseNopModel, ISettingsModel
    {
        public MonthlyReportSettingsModel() {

            WeekDayList = new List<SelectListItem>();
            ProjectList = new List<SelectListItem>();
        }

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.AllowedVariations")]

        public decimal AllowedVariations { get; set; }

        public bool AllowedVariations_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.AllowedQABillableHours")]
        public decimal AllowedQABillableHours { get; set; }

        public bool AllowedQABillableHours_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.WeekDay")]

        public int WeekDay { get; set; }

        public bool WeekDay_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.DayTime_From")]

        public DateTime? DayTime_From { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.DayTime_To")]

        public DateTime? DayTime_To { get; set; }



        public bool DayTime_From_OverrideForStore { get; set; }

        public bool DayTime_To_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendReportToEmployee")]

        public bool SendReportToEmployee { get; set; }

        public bool SendReportToEmployee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendReportToProjectLeader")]

        public bool SendReportToProjectLeader { get; set; }
        public bool SendReportToProjectLeader_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendReportToManager")]

        public bool SendReportToManager { get; set; }

        public bool SendReportToManager_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.ShowOnlyNotDOT")]

        public bool ShowOnlyNotDOT { get; set; }

        public bool ShowOnlyNotDOT_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendReportToHR")]

        public bool SendReportToHR { get; set; }

        public bool SendReportToHR_OverrideForStore { get; set; }

        public IList<SelectListItem> WeekDayList { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.LearningProjectId")]

        public int LearningProjectId { get; set; }

        public bool LearningProjectId_OverrideForStore { get; set; }

        public IList<SelectListItem> ProjectList { get; set; }




        //for overdue email

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.OverDue_From")]

        public DateTime? OverDue_From { get; set; }

        public bool OverDue_From_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.OverDue_To")]

        public DateTime? OverDue_To { get; set; }

        public bool OverDue_To_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendOverdueEmailToEmployee")]

        public bool SendOverdueEmailToEmployee { get; set; }

        public bool SendOverdueEmailToEmployee_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendOverdueReportToProjectLeader")]

        public bool SendOverdueReportToProjectLeader { get; set; }

        public bool SendOverdueReportToProjectLeader_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendOverdueReportToManager")]

        public bool SendOverdueReportToManager { get; set; }
        public bool SendOverdueReportToManager_OverrideForStore { get; set; }



        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.SendOverdueReportToHR")]

        public bool SendOverdueReportToHR { get; set; }
        public bool SendOverdueReportToHR_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.OverdueCountCCThreshold")]

        public int OverdueCountCCThreshold { get; set; }
        public bool OverdueCountCCThreshold_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.IncludeProjectLeadersInCC")]

        public bool IncludeProjectLeadersInCC { get; set; }
        public bool IncludeProjectLeadersInCC_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.IncludeProjectManagerInCC")]

        public bool IncludeProjectManagerInCC { get; set; }
        public bool IncludeProjectManagerInCC_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.IncludeManagementInCC")]

        public bool IncludeManagementInCC { get; set; }
        public bool IncludeManagementInCC_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.IncludeHRInCC")]

        public bool IncludeHRInCC { get; set; }
        public bool IncludeHRInCC_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.MonthlyReportSetting.Fields.IncludeProjectCoordinatorInCC")]

        public bool IncludeProjectCoordinatorInCC { get; set; }
        public bool IncludeProjectCoordinatorInCC_OverrideForStore { get; set; }





    }
}
