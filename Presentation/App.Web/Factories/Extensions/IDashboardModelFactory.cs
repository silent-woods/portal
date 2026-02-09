using App.Core.Domain.ProjectTasks;
using App.Web.Models.Dashboard;
using App.Web.Models.Extensions.ProjectTasks;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial interface IDashboardModelFactory
    {
        Task<PendingDashboardModel> PrepareFollowUpDashboardModelAsync(string taskName = null, int statusType = 0, int currEmployeeId = 0, int projectId = 0, int employeeId = 0, int page = 1, int pageSize = int.MaxValue, IList<int> managedProjectIds = null, IList<int> visibleProjectIds = null, bool showOnlyNotOnTrack = false, string sourceType = null, DateTime? from = null, DateTime? to = null, int percentageFilter = 0, int processWorkflow = 0, int statusId = 0);
        Task<PendingDashboardModel> PrepareCodeReviewDashboardModelAsync(int currEmployeeId = 0, int projectId = 0, int employeeId = 0, string taskName = null, int statusId = 0);
        Task<PendingDashboardModel> PrepareReadyToTestDashboardModelAsync(int currEmployeeId = 0, int projectId = 0, int employeeId = 0, string taskName = null, int statusId = 0);
        Task<PendingDashboardModel> PrepareOverdueDashboardModelAsync(int currEmployeeId = 0,int projectId = 0,int employeeId = 0,string taskName = null);
    }
}
