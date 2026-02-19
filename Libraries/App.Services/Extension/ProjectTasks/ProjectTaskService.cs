using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Localization;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Humanizer;
using Pipelines.Sockets.Unofficial.Arenas;
using StackExchange.Profiling.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace App.Services.ProjectTasks
{
    public partial class ProjectTaskService : IProjectTaskService
    {
        #region Fields
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<TimeSheet> _timesheetRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly MonthlyReportSetting _monthlyReportSetting;
        private readonly IDesignationService _designationService;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor
        public ProjectTaskService(IRepository<ProjectTask> projectTaskRepository, IRepository<TimeSheet> timesheetRepository, MonthlyReportSetting monthlyReportSetting, IRepository<Project> projectRepository
, IDesignationService designationService, IRepository<Employee> employeeRepository, ILocalizationService localizationService)
        {
            _projectTaskRepository = projectTaskRepository;
            _timesheetRepository = timesheetRepository;
            _projectRepository = projectRepository;
            _monthlyReportSetting = monthlyReportSetting;
            _designationService = designationService;
            _employeeRepository = employeeRepository;
            _localizationService = localizationService;
        }
        #endregion

        #region Utilities
        private async Task<IList<int>> GetTimeSheetByTaskNameAsync(string taskName)
        {
            var querys = (from t1 in _projectTaskRepository.Table
                          join t2 in _timesheetRepository.Table
                          on t1.Id equals t2.TaskId
                          where t1.TaskTitle.Contains(taskName)
                          select t1.Id).Distinct().ToList();
            return querys;
        }

        private async Task<(int SpentHours, int SpentMinutes)> GetDevelopmentTimeByTaskId(int taskId)
        {
            if (taskId <= 0)
                throw new ArgumentException("Invalid Task ID");

            var qaRoleId = await _designationService.GetQARoleId();
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();
            var devMinutesQuery = _timesheetRepository.Table
                .Where(t => t.TaskId == taskId && !qaEmployeeIds.Contains(t.EmployeeId))
                .Select(t => (t.SpentHours * 60) + t.SpentMinutes);
            var totalDevMinutes = await LinqToDB.AsyncExtensions.SumAsync(devMinutesQuery);
            return (totalDevMinutes / 60, totalDevMinutes % 60);
        }
        #endregion

        #region Methods
        public virtual async Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(string projectTaskName, int projectId, int statusId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime = 0)
        {
            var query = await _projectTaskRepository.GetAllAsync(q =>
            {
                if (!showHidden)
                    q = q.Where(c => !c.IsDeleted);

                if (!string.IsNullOrWhiteSpace(projectTaskName))
                    q = q.Where(c =>
                        c.TaskTitle.Contains(projectTaskName) ||
                        c.Description.Contains(projectTaskName));
                if (projectId != 0)
                    q = q.Where(c => c.ProjectId == projectId);
                if (statusId != 0)
                    q = q.Where(c => c.StatusId == statusId);
                if (filterDeliveryOnTime == 1)
                    q = q.Where(pr => pr.DeliveryOnTime == true);
                else if (filterDeliveryOnTime == 2)
                    q = q.Where(pr => pr.DeliveryOnTime == false);

                return q.OrderByDescending(c => c.CreatedOnUtc);
            });
            return new PagedList<ProjectTask>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(int taskId = 0, int taskTypeId = 0, IList<int> employeeIds = null, IList<int> projectIds = null, string taskName = null, DateTime? from = null, DateTime? to = null, DateTime? dueDate = null, int SelectedStatusId = 0, int processWorkflowId = 0,
 int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime = 0)
        {
            var query = await _projectTaskRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => !c.IsDeleted);

                if (taskId != 0)
                {
                    query = query.Where(c => c.Id == taskId);
                }

                if (taskTypeId != 0)
                {
                    query = query.Where(c => c.Tasktypeid == taskTypeId);

                }
                if (employeeIds != null && employeeIds.Any())
                {
                    query = query.Where(c => employeeIds.Contains(c.AssignedTo));
                }

                if (projectIds != null && projectIds.Any())
                {
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                }

                if (!string.IsNullOrWhiteSpace(taskName))
                {
                    var normalizedTaskName = taskName.Trim().ToLower();

                    query = query.Where(c =>
                        c.TaskTitle != null &&
                        c.TaskTitle.Trim().ToLower().Contains(normalizedTaskName));
                }

                if (SelectedStatusId != 0)
                {
                    query = query.Where(pr => pr.StatusId == SelectedStatusId);
                }
                if (processWorkflowId != 0)
                {
                    query = query.Where(pr => pr.ProcessWorkflowId == processWorkflowId);
                }

                if (filterDeliveryOnTime == 1)
                {
                    query = query.Where(pr => pr.DeliveryOnTime == true);
                }
                else if (filterDeliveryOnTime == 2)
                {
                    query = query.Where(pr => pr.DeliveryOnTime == false);
                }
                if (from.HasValue)
                    query = query.Where(pr => pr.CreatedOnUtc >= from.Value);
                if (to.HasValue)
                    query = query.Where(pr => pr.CreatedOnUtc <= to.Value);

                if (dueDate.HasValue)
                {
                    query = query.Where(pr => pr.DueDate.Value <= dueDate.Value);
                    return query.OrderByDescending(c => c.DueDate);
                }
                return query.OrderByDescending(c => c.CreatedOnUtc);
            });
            return new PagedList<ProjectTask>(query.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<IList<ProjectTask>> GetAllProjectTasksByDateAsync(DateTime from, DateTime to, bool showHidden = false)
        {
            var projectTasks = await _projectTaskRepository.GetAllAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(pt => !pt.IsDeleted);
                query = query.Join(_timesheetRepository.Table,
                                   pt => pt.Id,
                                   ts => ts.TaskId,
                                   (pt, ts) => new { pt, ts })
                                 .Where(joined => joined.ts.SpentDate >= from && joined.ts.SpentDate <= to)
                                 .Select(joined => joined.pt)
                                 .Distinct();

                return query.OrderByDescending(c => c.CreatedOnUtc);
            });
            return projectTasks.ToList();
        }

        public virtual async Task<ProjectTask> GetProjectTasksByIdAsync(int projectTaskId, bool showHidden = false)
        {
            var task = await _projectTaskRepository.GetByIdAsync(projectTaskId, cache => default);
            if (task == null)
                return null;
            if (!showHidden && task.IsDeleted)
                return null;

            return task;
        }

        public virtual async Task<ProjectTask> GetProjectTasksWithoutCacheByIdAsync(int projectTaskId, bool showHidden = false)
        {
            var task = await _projectTaskRepository.GetByIdAsync(projectTaskId);
            if (task == null)
                return null;
            if (!showHidden && task.IsDeleted)
                return null;

            return task;
        }
        public virtual async Task<IList<ProjectTask>> GetProjectsTasksByIdsAsync(int[] projectTaskIds)
        {
            return await _projectTaskRepository.GetByIdsAsync(projectTaskIds, cache => default, false);
        }

        public virtual async Task InsertProjectTaskAsync(ProjectTask projectTask)
        {
            if (!string.IsNullOrWhiteSpace(projectTask.TaskTitle))
            {
                string prefix = string.Empty;

                if (projectTask.Tasktypeid == (int)TaskTypeEnum.Bug)
                    prefix = await _localizationService.GetResourceAsync("ProjectTask.Prefix.Bug");

                else if (projectTask.Tasktypeid == (int)TaskTypeEnum.ChangeRequest)
                    prefix = await _localizationService.GetResourceAsync("ProjectTask.Prefix.ChangeRequest");

                else if (projectTask.Tasktypeid == (int)TaskTypeEnum.UserStory)
                    prefix = await _localizationService.GetResourceAsync("ProjectTask.Prefix.UserStory");

                if (!string.IsNullOrWhiteSpace(prefix))
                    projectTask.TaskTitle = $"{prefix} {projectTask.TaskTitle.Trim()}";
            }
            if (projectTask.Tasktypeid != 3)
            {
                projectTask.WorkQuality = 100.00m;
            }
            await _projectTaskRepository.InsertAsync(projectTask);
        }

        public virtual async Task UpdateProjectTaskAsync(ProjectTask projectTask)
        {
            if (projectTask == null)
                throw new ArgumentNullException(nameof(projectTask));

            var oldTask = await GetProjectTasksWithoutCacheByIdAsync(projectTask.Id);
            var oldTaskCopy = new ProjectTask
            {
                TaskTitle = oldTask.TaskTitle,

                EstimatedTime = oldTask.EstimatedTime,
                ProjectId = oldTask.ProjectId,
                Description = oldTask.Description,
                StatusId = oldTask.StatusId,
                AssignedTo = oldTask.AssignedTo,
                DueDate = oldTask.DueDate,
                Tasktypeid = oldTask.Tasktypeid,
                ProcessWorkflowId = oldTask.ProcessWorkflowId,
                ParentTaskId = oldTask.ParentTaskId,
                IsSync = oldTask.IsSync,
            };
            await _projectTaskRepository.UpdateAsync(projectTask);
            await UpdateParentTaskWorkQualityAsync(oldTaskCopy, projectTask);
        }

        public virtual async Task DeleteProjectTaskAsync(ProjectTask projectTask)
        {
            projectTask.IsDeleted = true;
            await _projectTaskRepository.UpdateAsync(projectTask, false);
        }

        public virtual async Task<IList<ProjectTask>> GetProjectTasksByProjectId(int projectId, bool showHidden = false)
        {
            var result = new List<ProjectTask>();

            result.Add(new ProjectTask
            {
                Id = 0,
                TaskTitle = "All"
            });
            if (projectId == 0)
                return result;

            var tasks = await _projectTaskRepository.GetAllAsync(query =>
            {
                if (!showHidden)
                    query = query.Where(pt => !pt.IsDeleted);
                query = query.Where(c => c.ProjectId == projectId);
                return query.OrderByDescending(c => c.CreatedOnUtc);
            });
            result.AddRange(tasks);

            return result;
        }

        public virtual async Task<IList<ProjectTask>> GetProjectTasksByProjectIdForTimeSheet(int projectId, bool showHidden = false)
        {
            var tasks = await _projectTaskRepository.GetAllAsync(async query =>
            {
                query = query.Where(pt =>
                    pt.ProjectId == projectId &&
                    pt.Tasktypeid != (int)TaskTypeEnum.UserStory);

                if (!showHidden)
                    query = query.Where(pt => !pt.IsDeleted);

                return query.OrderByDescending(pt => pt.CreatedOnUtc);
            });
            return tasks;
        }

        public async Task<ProjectTask> GetProjectTaskByTitleAndProjectAsync(string taskTitle, int projectId, bool showHidden = false)
        {
            if (string.IsNullOrWhiteSpace(taskTitle))
                throw new ArgumentException("Task title cannot be null or empty.", nameof(taskTitle));

            var query = _projectTaskRepository.Table
        .Where(pt => pt.TaskTitle == taskTitle && pt.ProjectId == projectId);
            if (!showHidden)
                query = query.Where(pt => !pt.IsDeleted);

            return await query.FirstOrDefaultAsync();
        }
        public async Task<Project> GetProjectByTaskIdAsync(int taskId)
        {
            var task = await GetProjectTasksByIdAsync(taskId);
            Project project = new Project();
            if (task != null)
                project = await _projectRepository.Table.Where(pr => pr.Id == task.ProjectId).FirstOrDefaultAsync();

            return project;
        }
        public async Task<List<ProjectTask>> GetProjectTasksByIdsAsync(List<int> taskIds)
        {
            if (taskIds == null || !taskIds.Any())
                return new List<ProjectTask>();
            return await _projectTaskRepository.Table
                .Where(task => taskIds.Contains(task.Id))
                .ToListAsync();
        }

        public async Task<IList<ProjectTask>> GetParentTasksByProjectIdAsync(int projectId, bool showHidden = false)
        {
            return await _projectTaskRepository.Table
                .Where(t => showHidden || !t.IsDeleted)
                .Where(t =>
                    (projectId == 0 ||
                     (t.ProjectId == projectId && t.ParentTaskId == 0))
                     ||
                    (t.Tasktypeid == 4 && t.ProjectId == projectId))
                .OrderBy(t => t.CreatedOnUtc)
                .ToListAsync();
        }

        public async Task<IList<ProjectTask>> GetBugChildTasksByParentTaskIdAsync(int parentTaskId, bool showHidden = false)
        {
            return await _projectTaskRepository.Table
                .Where(t => showHidden || !t.IsDeleted)
                .Where(t => t.ParentTaskId == parentTaskId && t.Tasktypeid == (int)TaskTypeEnum.Bug)
                .ToListAsync();
        }

        public async Task<IList<ProjectTask>> GetProjectTasksByParentIdAsync(int parentTaskId, bool showHidden = false)
        {
            if (parentTaskId <= 0)
                return new List<ProjectTask>();

            return await _projectTaskRepository.Table
                .Where(t => showHidden || !t.IsDeleted)
                .Where(t => t.ParentTaskId == parentTaskId)
                .ToListAsync();
        }

        public async Task<(int Hours, int Minutes)> GetBugTimeByTaskIdAsync(int taskId)
        {
            if (taskId <= 0)
                return (0, 0);

            var qaRoleId = await _designationService.GetQARoleId();
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();
            var bugTaskIds = await _projectTaskRepository.Table
                .Where(t => t.ParentTaskId == taskId && t.Tasktypeid == 3)
                .Select(t => t.Id)
                .ToListAsync();
            if (!bugTaskIds.Any())
                return (0, 0);
            var devBugTimeEntries = await _timesheetRepository.Table
                .Where(ts => bugTaskIds.Contains(ts.TaskId) && !qaEmployeeIds.Contains(ts.EmployeeId))
                .Select(ts => new { ts.SpentHours, ts.SpentMinutes })
                .ToListAsync();
            int totalHours = devBugTimeEntries.Sum(e => e.SpentHours);
            int totalMinutes = devBugTimeEntries.Sum(e => e.SpentMinutes);
            totalHours += totalMinutes / 60;
            totalMinutes = totalMinutes % 60;
            return (totalHours, totalMinutes);
        }

        public async Task<decimal?> CalculateWorkQualityAsync(int projectTaskId)
        {
            var task = await _projectTaskRepository.GetByIdAsync(projectTaskId);
            if (task == null || task.EstimatedTime <= 0)
                return null;
            var bugTimeData = await GetBugTimeByTaskIdAsync(projectTaskId);
            var bugTimeInHours = bugTimeData.Hours + (bugTimeData.Minutes / 60m);
            decimal bufferPercentage = _monthlyReportSetting.AllowedVariations;
            var adjustedEstimation = task.EstimatedTime * (1 + (bufferPercentage / 100m));
            if (adjustedEstimation == 0)
                return null;
            var ratio = 1 - (bugTimeInHours / adjustedEstimation);
            var workQualityPercent = Math.Clamp(ratio * 100, 0, 100);
            return Math.Round(workQualityPercent, 2);
        }
        public virtual async Task UpdateParentTaskWorkQualityAsync(ProjectTask oldTask, ProjectTask newTask)
        {
            var newIsBug = newTask.Tasktypeid == 3;
            var oldIsBug = oldTask?.Tasktypeid == 3;
            if (oldTask == null && newIsBug && newTask.ParentTaskId != 0)
            {
                await UpdateDOTForTaskAsync(newTask.ParentTaskId); 
                await UpdateDOTForTaskAsync(newTask.Id);
                var parent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                var workQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                if (workQuality != null)
                {
                    parent.WorkQuality = workQuality.Value;
                    await _projectTaskRepository.UpdateAsync(parent);
                }
                return;
            }
            if (oldIsBug && !newIsBug && oldTask.ParentTaskId != 0)
            {
                await UpdateDOTForTaskAsync(oldTask.ParentTaskId);
                await UpdateDOTForTaskAsync(oldTask.Id);
                await SetTaskDOTTo100Async(oldTask);

                var oldParent = await GetProjectTasksWithoutCacheByIdAsync(oldTask.ParentTaskId);
                var oldWorkQuality = await CalculateWorkQualityAsync(oldTask.ParentTaskId);
                if (oldWorkQuality != null)
                {
                    oldParent.WorkQuality = oldWorkQuality.Value;
                    await _projectTaskRepository.UpdateAsync(oldParent);

                    oldTask.WorkQuality = 100.00m;
                    await _projectTaskRepository.UpdateAsync(oldTask);
                }
                return;
            }
            if (!oldIsBug && newIsBug && newTask.ParentTaskId != 0)
            {
                await UpdateDOTForTaskAsync(newTask.ParentTaskId);
                await UpdateDOTForTaskAsync(newTask.Id);
                var newParent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                var newWorkQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                if (newWorkQuality != null)
                {
                    newParent.WorkQuality = newWorkQuality.Value;
                    await _projectTaskRepository.UpdateAsync(newParent);
                }

                return;
            }
            if (newIsBug && oldTask.ParentTaskId != newTask.ParentTaskId)
            {
                if (oldTask.ParentTaskId != 0)
                {
                    await UpdateDOTForTaskAsync(oldTask.ParentTaskId);
                    await UpdateDOTForTaskAsync(oldTask.Id);
                    var oldParent = await GetProjectTasksWithoutCacheByIdAsync(oldTask.ParentTaskId);
                    var oldWorkQuality = await CalculateWorkQualityAsync(oldTask.ParentTaskId);
                    if (oldWorkQuality != null)
                    {
                        oldParent.WorkQuality = oldWorkQuality.Value;
                        await _projectTaskRepository.UpdateAsync(oldParent);
                    }
                }
                if (newTask.ParentTaskId != 0)
                {
                    await UpdateDOTForTaskAsync(newTask.ParentTaskId);
                    await UpdateDOTForTaskAsync(newTask.Id);
                    var newParent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                    var newWorkQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                    if (newWorkQuality != null)
                    {
                        newParent.WorkQuality = newWorkQuality.Value;
                        await _projectTaskRepository.UpdateAsync(newParent);
                    }
                }
                return;
            }
            if (newIsBug && newTask.ParentTaskId != 0)
            {
                await UpdateDOTForTaskAsync(newTask.ParentTaskId);
                await UpdateDOTForTaskAsync(newTask.Id);
                var parent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                var workQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                if (workQuality != null)
                {
                    parent.WorkQuality = workQuality.Value;
                    await _projectTaskRepository.UpdateAsync(parent);
                }

                return;
            }
            if (oldTask.Id != newTask.Id)
            {
                await UpdateDOTForTaskAsync(oldTask.Id);
                await UpdateDOTForTaskAsync(newTask.Id);
            }
            else
            {
                await UpdateDOTForTaskAsync(newTask.Id);
            }
        }
        public async Task<decimal?> CalculateDeliveryPerformanceAsync(int projectTaskId)
        {
            var task = await _projectTaskRepository.GetByIdAsync(projectTaskId);
            if (task == null || task.EstimatedTime <= 0)
                return null;
            var parentTime = await GetDevelopmentTimeByTaskId(projectTaskId);
            var childBugTaskIds = await _projectTaskRepository
                .Table
                .Where(x => x.ParentTaskId == projectTaskId && x.Tasktypeid == 3)
                .Select(x => x.Id)
                .ToListAsync();

            int totalHours = parentTime.SpentHours;
            int totalMinutes = parentTime.SpentMinutes;

            foreach (var bugTaskId in childBugTaskIds)
            {
                var bugTime = await GetDevelopmentTimeByTaskId(bugTaskId);
                totalHours += bugTime.SpentHours;
                totalMinutes += bugTime.SpentMinutes;
            }
            totalHours += totalMinutes / 60;
            totalMinutes = totalMinutes % 60;
            var spentTimeInHours = totalHours + (totalMinutes / 60m);
            decimal bufferPercentage = _monthlyReportSetting.AllowedVariations;
            var adjustedEstimation = task.EstimatedTime * (1 + bufferPercentage / 100m);

            if (adjustedEstimation == 0)
                return null;

            decimal deliveryPerformance;

            if (spentTimeInHours <= adjustedEstimation)
            {
                deliveryPerformance = 100;
            }
            else if (spentTimeInHours >= adjustedEstimation * 2)
            {
                deliveryPerformance = 0;
            }
            else
            {
                var overRatio = (spentTimeInHours - adjustedEstimation) / adjustedEstimation;
                deliveryPerformance = 100 - (overRatio * 100);
            }

            deliveryPerformance = Math.Clamp(deliveryPerformance, 0, 100);
            return Math.Round(deliveryPerformance, 2);
        }
        private async Task UpdateDOTForTaskAsync(int taskId)
        {
            var task = await GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task == null)
                return; 

            decimal? dot;

            if (task.IsManualDOT && task.DeliveryOnTime)
            {
                dot = 100m;
            }
            else
            {
                dot = await CalculateDeliveryPerformanceAsync(taskId);
            }
            if (dot != null)
            {
                task.DOTPercentage = dot;
                await _projectTaskRepository.UpdateAsync(task);
            }
        }

        private async Task SetTaskDOTTo100Async(ProjectTask task)
        {
            task.DOTPercentage = 100.00m;
            await _projectTaskRepository.UpdateAsync(task);
        }

        private async Task SetTaskDOTToNullAsync(ProjectTask task)
        {
            task.DOTPercentage = null;
            await _projectTaskRepository.UpdateAsync(task);
        }

        public async Task<bool> HasBugTasksAsync(int parentTaskId, bool showHidden = false)
        {
            return await _projectTaskRepository.Table
                .Where(t => showHidden || !t.IsDeleted)
                .AnyAsync(t => t.ParentTaskId == parentTaskId && t.Tasktypeid == 3);
        }
        #endregion
    }
}