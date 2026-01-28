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
        }
        public int CuurentEmployeeId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int SearchPeriodId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

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
}