using App.Core.Domain.Extension.TimeSheets;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.ActivityTracking
{
    public partial record ActivityTrackingModel : BaseNopEntityModel
    {

    
       public ActivityTrackingModel()
        {

        }

        public int ProjectId { get; set; }

        public string ProjectName { get; set; }


        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public int ActivityId { get; set; }

        public string ActivityName { get; set; }

        public int ActivityStatusId { get; set; }

        public string ActivityStatusName { get; set; }
            
  
        
        public string EmployeeName { get; set; }

        public string PresentStatus { get; set; }

        public string Time { get; set; }

        public string SpentTime { get; set; }

        public string LeaveInfo { get; set; } 
        public bool ShowLeaveInfo { get; set; }


        //ActivityTracking Report
        public int EmployeeId { get; set; }

        public int ActiveDuration { get; set; }

        public string ActiveDurationHHMM { get; set; }


        public int AwayDuration { get; set; }

        public string AwayDurationHHMM { get; set; }


        public int OfflineDuration { get; set; }

        public string OfflineDurationHHMM { get; set; }


        public int StoppedDuration { get; set; }

        public string StoppedDurationHHMM { get; set; }


        public int TotalDuration { get; set; }

        public string TotalDurationHHMM { get; set; }


        public string JsonString { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Date { get; set; }

        public string Label { get; set; }
        public ShowByEnum ShowBy { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public DateTime TodayDate { get; set; }


    }
}
