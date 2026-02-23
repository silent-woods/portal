using App.Core;
using App.Core.Domain.ProjectTasks;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface IFollowUpTaskService
    {
        Task<IPagedList<FollowUpTask>> GetAllFollowUpTasksAsync(
      int taskId = 0,
      int reviewerId = 0,
      IList<int> projectIds = null,
      IList<int> employeeIds = null,
      string comment = null,
      string type = null,
      string taskName = null,
      int statusType = 0,
      int pageIndex = 0,
      int pageSize = int.MaxValue,
      int currEmployeeId = 0,
      bool showOnlyNotOnTrack = false,
      string sourceType = null,
       DateTime? from = null,
       DateTime? to = null,
       int percentageFilter = 0,
       int processWorkflow = 0,
       int statusId = 0,
      IList<int> visibleProjectIds = null,
      IList<int> managedProjectIds = null);
        Task<FollowUpTask> GetFollowUpTaskByIdAsync(int id);
        Task<IList<FollowUpTask>> GetFollowUpTasksByIdsAsync(int[] ids);
        Task<FollowUpTask> GetFollowUpTaskByTaskIdAsync(int taskId);
        Task InsertFollowUpTaskAsync(FollowUpTask entity);
        Task UpdateFollowUpTaskAsync(FollowUpTask entity);
        Task DeleteFollowUpTaskAsync(FollowUpTask entity);
        Task InsertFollowupTaskByTask(ProjectTask entity);
        Task<bool> CheckIfManaualFollowupExistsAsync(int taskId);
        DateTime AdjustToOfficeHours(DateTime startTime, int minutesToAdd, TimeZoneInfo officeTimeZone);
    }
}
