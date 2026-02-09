using App.Core;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace App.Web.Models.Dashboard
{
    public partial record PendingDashboardModel : BaseNopModel
    {
        public PendingDashboardModel()
        {
            NewTasks = new List<FollowUpTaskModel>();
            Overdue = new List<FollowUpTaskModel>();
            DueToday = new List<FollowUpTaskModel>();
            Upcoming = new List<FollowUpTaskModel>();
            CompletedList = new List<FollowUpTaskModel>();
            AvailableEmployees = new List<SelectListItem>();
            AvailableProjects = new List<SelectListItem>();
            AvailableSearchPeriods = new List<SelectListItem>();
            CodeReviewTasks=  new List<CodeReviewTaskModel>();
            ReadyToTestTasks = new List<ReadyToTestTaskModel>();
            OverdueTasks = new List<OverdueTaskModel>();
            AvailableStatus= new List<SelectListItem>();
            AvailableProcessWorkflow = new List<SelectListItem>();
            StatusFilters =  new List<StatusFilterModel>();
        }
        public int CurrentEmployeeId { get; set; }
        public string CurrentEmployeeName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int SearchPeriodId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public int ProjectId { get; set; }
        public int EmployeeId { get; set; }
        public string TaskName { get; set; }
        public int PendingCodeReviewCount { get; set; }
        public int PendingReadyToTestCount { get; set; }
        public int PendingOverdueCount { get; set; }
        public int SelectedAlertPercentage { get; set; }
        public int SearchProcessWorkflowId { get; set; }
        public int SearchStatusId { get; set; }
        public IList<StatusFilterModel> StatusFilters { get; set; }

        public int SelectedStatusId { get; set; }
        public IList<SelectListItem> AvailableProcessWorkflow { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }
        public IList<OverdueTaskModel> OverdueTasks { get; set; }
        public IList<SelectListItem> AvailableAlertPercentages { get; set; }
        public IList<CodeReviewTaskModel> CodeReviewTasks { get; set; }
        public IList<ReadyToTestTaskModel> ReadyToTestTasks { get; set; }       
        public IList<FollowUpTaskModel> NewTasks { get; set; }
        public IList<FollowUpTaskModel> Overdue { get; set; }
        public IList<FollowUpTaskModel> DueToday { get; set; }
        public IList<FollowUpTaskModel> Upcoming { get; set; }
        public IList<FollowUpTaskModel> CompletedList { get; set; }
        public IList<SelectListItem> AvailableProjects { get; set; }
        public IList<SelectListItem> AvailableEmployees { get; set; }
        public IList<SelectListItem> AvailableSearchPeriods { get; set; }
        public IPagedList<FollowUpTask> CompletedPagedList { get; set; }
    }

    public class StatusFilterModel
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string ColorCode { get; set; }
        public int Count { get; set; }

        public int ProcessWorkflowId { get; set; }
        public string ProcessWorkflowName { get; set; }
    }

}