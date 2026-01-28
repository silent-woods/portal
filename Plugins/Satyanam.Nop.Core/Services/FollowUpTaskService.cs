using App.Core;
using App.Core.Domain.ProjectTasks;
using App.Data;
using App.Data.Extensions;
using App.Services.Projects;
using MailKit;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial class FollowUpTaskService : IFollowUpTaskService
    {
        #region Fields
        private readonly IRepository<FollowUpTask> _followUpTaskRepository;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IProjectsService _projectsService;
        #endregion

        #region Ctor
        public FollowUpTaskService(IRepository<FollowUpTask> followUpTaskRepository, IProjectsService projectsService, IRepository<ProjectTask> projectTaskRepository)
        {
            _followUpTaskRepository = followUpTaskRepository;
            _projectsService = projectsService;
            _projectTaskRepository = projectTaskRepository;
        }
        #endregion
        private async Task<List<int>> GetFollowupByTaskNameAsync(string taskName)
        {
            var query = from t1 in _projectTaskRepository.Table
                        join t2 in _followUpTaskRepository.Table
                        on t1.Id equals t2.TaskId
                        where t1.TaskTitle.ToLower().Contains(taskName.ToLower())
                        select t1.Id;

            return await query.Distinct().ToListAsync();
        }
        #region Methods
        public virtual async Task<IPagedList<FollowUpTask>> GetAllFollowUpTasksAsync(
      int taskId = 0,
      int reviewerId = 0,
      int projectId = 0,
      int employeeId = 0,
      string comment = null,
      string type = null,       
      string taskName = null,   
      int statusType =0,
      int pageIndex = 0,
      int pageSize = int.MaxValue,
      int currEmployeeId = 0,
      bool showOnlyNotOnTrack =false,
      string sourceType = null,
       DateTime? from = null,  
        DateTime? to = null,
      IList<int> visibleProjectIds = null,
      IList<int> managedProjectIds = null)
        {
            var today = DateTime.UtcNow.Date;
            var query = _followUpTaskRepository.Table;
            if (taskId > 0)
                query = query.Where(f => f.TaskId == taskId);
            if (reviewerId > 0)
                query = query.Where(f => f.ReviewerId == reviewerId);
            if (!string.IsNullOrWhiteSpace(comment))
            {
                var trimmed = comment.Trim().ToLower();
                query = query.Where(f => f.LastComment != null &&
                                         f.LastComment.ToLower().Contains(trimmed));
            }
            if (visibleProjectIds != null && visibleProjectIds.Any())
            {
                query =
                    from f in query
                    join t in _projectTaskRepository.Table
                        on f.TaskId equals t.Id
                    where visibleProjectIds.Contains(t.ProjectId)
                    select f;
            }
            if (currEmployeeId > 0)
            {
                query =
                    from f in query
                    join t in _projectTaskRepository.Table
                        on f.TaskId equals t.Id
                    where
                        (managedProjectIds != null && managedProjectIds.Contains(t.ProjectId))
                        || t.AssignedTo == currEmployeeId
                    select f;
            }
            if(showOnlyNotOnTrack)
                query = query.Where(f =>
              f.OnTrack);

            if (statusType != 0)
            {
                if(statusType == 1)
                {
                    query = query.Where(f =>
               f.IsCompleted);
                }
                else if(statusType == 2)
                {
                    query = query.Where(f =>
                !f.IsCompleted);
                }
            }
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                var taskIds = await GetFollowupByTaskNameAsync(taskName);
                query = query.Where(c => taskIds.Contains(c.TaskId));
            }
            if (!string.IsNullOrWhiteSpace(sourceType))
            {
                if (sourceType == "manual")
                    query = query.Where(c => c.AlertId==0);
                if (sourceType == "auto")
                    query = query.Where(c => c.AlertId > 0);
            }
            if (projectId > 0)
            {
                query = from f in query
                        join t in _projectTaskRepository.Table
                            on f.TaskId equals t.Id
                        where t.ProjectId == projectId
                        select f;
            }
            if (from.HasValue)
            {
                var fromDate = from.Value.Date;
                query = query.Where(f =>
                    f.NextFollowupDateTime.HasValue &&
                    f.NextFollowupDateTime.Value.Date >= fromDate);
            }

            if (to.HasValue)
            {
                var toDate = to.Value.Date;
                query = query.Where(f =>
                    f.NextFollowupDateTime.HasValue &&
                    f.NextFollowupDateTime.Value.Date <= toDate);
            }

            if (employeeId > 0)
            {
                query = from f in query
                        join t in _projectTaskRepository.Table
                            on f.TaskId equals t.Id
                        where t.AssignedTo == employeeId
                        select f;
            }
            if (!string.IsNullOrWhiteSpace(type))
            {
                switch (type.ToLower())
                {
                    case "new":
                        query = query.Where(f => f.LastFollowupDateTime == null);
                        break;
                    case "overdue":
                        query = query.Where(f => f.NextFollowupDateTime < today);
                        break;
                    case "today":
                        query = query.Where(f => f.NextFollowupDateTime == today);
                        break;
                    case "upcoming":
                        query = query.Where(f => f.NextFollowupDateTime > today);
                        break;
                }
            }
            query = query.OrderByDescending(f => f.UpdatedOn);
            var list = await query.ToPagedListAsync(pageIndex, pageSize);
            return list;
        }

        public virtual async Task<FollowUpTask> GetFollowUpTaskByIdAsync(int id)
        {
            return await _followUpTaskRepository.GetByIdAsync(id);
        }

        public virtual async Task<IList<FollowUpTask>> GetFollowUpTasksByIdsAsync(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<FollowUpTask>();
            var query = _followUpTaskRepository.Table.Where(f => ids.Contains(f.Id));
            var list = query.ToList()
                .OrderBy(f => Array.IndexOf(ids, f.Id))
                .ToList();

            return await Task.FromResult(list);
        }

        public virtual async Task<FollowUpTask> GetFollowUpTaskByTaskIdAsync(int taskId)
        {
            ArgumentNullException.ThrowIfNull(nameof(taskId));
            return await _followUpTaskRepository.Table.Where(ft => ft.TaskId == taskId).FirstOrDefaultAsync();
        }

        public virtual async Task InsertFollowUpTaskAsync(FollowUpTask entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            entity.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);
            entity.UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);
            await _followUpTaskRepository.InsertAsync(entity);
        }

        public virtual async Task UpdateFollowUpTaskAsync(FollowUpTask entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            entity.UpdatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);
            await _followUpTaskRepository.UpdateAsync(entity);
        }

        public virtual async Task DeleteFollowUpTaskAsync(FollowUpTask entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            await _followUpTaskRepository.DeleteAsync(entity);
        }

        public virtual async Task InsertFollowupTaskByTask(ProjectTask entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            FollowUpTask followUpTask = new FollowUpTask();
            followUpTask.TaskId = entity.Id;
            followUpTask.AlertId = -1;
            followUpTask.ReviewerId = await _projectsService.GetReviewerIdByProjectIdAsync(entity.ProjectId);         
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            followUpTask.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istTimeZone);
            followUpTask.UpdatedOn = followUpTask.CreatedOn;
            await InsertFollowUpTaskAsync(followUpTask);            
        }

        public virtual async Task<bool> CheckIfManaualFollowupExistsAsync(int taskId)
        {
            ArgumentNullException.ThrowIfNull(nameof(taskId));

            var followupTask = from ft in _followUpTaskRepository.Table
                               where ft.TaskId == taskId && ft.AlertId == 0
                               select ft;

            return followupTask.Any();
        }
        #endregion
    }
}
