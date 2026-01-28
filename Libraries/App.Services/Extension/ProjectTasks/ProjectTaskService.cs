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
    /// <summary>
    /// Projects service
    /// </summary>
    public partial class ProjectTaskService : IProjectTaskService
    {
        #region Fields

        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IRepository<TimeSheet> _timesheetRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly MonthlyReportSetting _monthlyReportSetting;
        private readonly IDesignationService _designationService;
        private readonly IRepository<Employee> _employeeRepository;




        #endregion

        #region Ctor


        public ProjectTaskService(IRepository<ProjectTask> projectTaskRepository, IRepository<TimeSheet> timesheetRepository, MonthlyReportSetting monthlyReportSetting, IRepository<Project> projectRepository
, IDesignationService designationService, IRepository<Employee> employeeRepository)
        {
            _projectTaskRepository = projectTaskRepository;
            _timesheetRepository = timesheetRepository;
            _projectRepository = projectRepository;
            _monthlyReportSetting = monthlyReportSetting;
            _designationService = designationService;
            _employeeRepository = employeeRepository;
        }

        #endregion


        private  async Task<IList<int>> GetTimeSheetByTaskNameAsync(string taskName)
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

            // Get QA Employee IDs (we'll use it in DB-side NOT IN filter)
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();

            // DB-side SUM to avoid pulling unnecessary data
            var devMinutesQuery = _timesheetRepository.Table
                .Where(t => t.TaskId == taskId && !qaEmployeeIds.Contains(t.EmployeeId))
                .Select(t => (t.SpentHours * 60) + t.SpentMinutes);

            var totalDevMinutes = await LinqToDB.AsyncExtensions.SumAsync(devMinutesQuery);


            return (totalDevMinutes / 60, totalDevMinutes % 60);
        }



        #region Methods

        /// <summary>
        /// Get all project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(string projectTaskName,int projectId , int statusId,
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
            //paging
            return new PagedList<ProjectTask>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(int taskId,int taskTypeId, IList<int> employeeIds, IList<int> projectIds, string taskName, DateTime? from, DateTime? to,DateTime? dueDate, int SelectedStatusId, int processWorkflowId,
 int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int filterDeliveryOnTime =0)
        {
            var query = await _projectTaskRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => !c.IsDeleted);

                if (taskId != 0)
                {
                    query = query.Where(c => c.Id == taskId);
                }

                if(taskTypeId != 0)
                {
                    query = query.Where(c => c.Tasktypeid == taskTypeId);

                }
                // Filter by employee IDs if provided
                if (employeeIds != null && employeeIds.Any())
                {
                    query = query.Where(c => employeeIds.Contains(c.AssignedTo));
                }

                // Filter by project IDs if provided
                if (projectIds != null && projectIds.Any())
                {
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                }

                // Filter by task name if provided
                if (!string.IsNullOrWhiteSpace(taskName))
                {
                    var taskIds = await  GetTimeSheetByTaskNameAsync(taskName);
                    query = query.Where(c => c.TaskTitle.Contains(taskName));
                }

                if(SelectedStatusId != 0)
                {
                    query = query.Where(pr => pr.StatusId == SelectedStatusId);
                }
                if (processWorkflowId != 0)
                {
                    query = query.Where(pr => pr.ProcessWorkflowId == processWorkflowId);
                }
                
                if(filterDeliveryOnTime == 1)
                {
                    query = query.Where(pr => pr.DeliveryOnTime == true);
                }
                else  if (filterDeliveryOnTime == 2)
                {
                    query = query.Where(pr => pr.DeliveryOnTime == false);
                }
                // Filter by date range
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

            // Paging
            return new PagedList<ProjectTask>(query.ToList(), pageIndex, pageSize);
            //return new PagedList<TimeSheet>(query.Skip(pageIndex * pageSize).Take(pageSize).ToList(), pageIndex, pageSize);
        }

        public virtual async Task<IList<ProjectTask>> GetAllProjectTasksAsync(bool showHidden = false)
        {
            var query = await _projectTaskRepository.GetAllAsync(async query =>
            {
                if (!showHidden)
                    query = query.Where(c => !c.IsDeleted);
                return query.OrderByDescending(c => c.CreatedOnUtc);
            });
            return query.ToList();
        }

        public virtual async Task<IList<ProjectTask>> GetAllProjectTasksByDateAsync(DateTime from, DateTime to,bool showHidden = false)
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


        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<ProjectTask>> GetProjectsTasksByIdsAsync(int[] projectTaskIds)
        {
            return await _projectTaskRepository.GetByIdsAsync(projectTaskIds, cache => default, false);
        }

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task InsertProjectTaskAsync(ProjectTask projectTask)
        {
            if (projectTask.Tasktypeid == (int)TaskTypeEnum.Bug && projectTask.TaskTitle !=null)
                projectTask.TaskTitle = "Bug: " + projectTask.TaskTitle.Trim();
            else if (projectTask.Tasktypeid == (int)TaskTypeEnum.ChangeRequest && projectTask.TaskTitle !=null)
                projectTask.TaskTitle = "CR: " + projectTask.TaskTitle.Trim();

            if(projectTask.Tasktypeid != 3)
            {
                projectTask.WorkQuality = 100.00m;
            }
            await _projectTaskRepository.InsertAsync(projectTask);
                                                                                    

        }
                                                                                         
        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateProjectTaskAsync(ProjectTask projectTask)
        {
            if (projectTask == null)
                throw new ArgumentNullException(nameof(projectTask));

            var oldTask = await GetProjectTasksWithoutCacheByIdAsync(projectTask.Id);
            //for update parent task's work quality
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
           
                ProcessWorkflowId =  oldTask.ProcessWorkflowId,
                ParentTaskId = oldTask.ParentTaskId,
                IsSync = oldTask.IsSync,
                
            };

           
            await _projectTaskRepository.UpdateAsync(projectTask);
            await UpdateParentTaskWorkQualityAsync(oldTaskCopy, projectTask);
            //await UpdateDeliveryOnTimeAsync(oldTaskCopy, projectTask);
        }



        /// <summary>
        /// delete project by record
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
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
           
            // If projectId is not 0, fetch tasks from the database
            var tasks = await _projectTaskRepository.GetAllAsync(async query =>
            {
                if(!showHidden)
                    query = query.Where(pt => !pt.IsDeleted);
                query = query.Where(c => c.ProjectId == projectId);
                return query.OrderByDescending(c => c.CreatedOnUtc);
            });

            // If no tasks are fetched, return an empty list
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

            // Get QA Role Id
            var qaRoleId = await _designationService.GetQARoleId();

            // Get active QA employee IDs
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();

            // Get all bug-type child task IDs for the given parent task
            var bugTaskIds = await _projectTaskRepository.Table
                .Where(t => t.ParentTaskId == taskId && t.Tasktypeid == 3)
                .Select(t => t.Id)
                .ToListAsync();

            if (!bugTaskIds.Any())   
                return (0, 0);

            // Get time entries excluding QA employees
            var devBugTimeEntries = await _timesheetRepository.Table
                .Where(ts => bugTaskIds.Contains(ts.TaskId) && !qaEmployeeIds.Contains(ts.EmployeeId))
                .Select(ts => new { ts.SpentHours, ts.SpentMinutes })
                .ToListAsync();

            int totalHours = devBugTimeEntries.Sum(e => e.SpentHours);
            int totalMinutes = devBugTimeEntries.Sum(e => e.SpentMinutes);

            // Normalize minutes to hours
            totalHours += totalMinutes / 60;
            totalMinutes = totalMinutes % 60;
                      
            return (totalHours, totalMinutes);
        }
                                                 
        public async Task<decimal?> CalculateWorkQualityAsync(int projectTaskId)
        {
            // Get the task
            var task = await _projectTaskRepository.GetByIdAsync(projectTaskId);
            if (task == null || task.EstimatedTime <= 0)
                return null;

            // Get bug time (in hours)
            var bugTimeData = await GetBugTimeByTaskIdAsync(projectTaskId);
            
            // Convert to total hours
            var bugTimeInHours = bugTimeData.Hours + (bugTimeData.Minutes / 60m);

            decimal bufferPercentage = _monthlyReportSetting.AllowedVariations; 
            var adjustedEstimation = task.EstimatedTime * (1 + (bufferPercentage / 100m));


            if (adjustedEstimation == 0)
                return null;

            // Formula: WorkQuality = 1 - (BugTime / AdjustedEstimation)
            var ratio = 1 - (bugTimeInHours / adjustedEstimation);

            // Convert to percentage and clamp
            var workQualityPercent = Math.Clamp(ratio * 100, 0, 100);

            return Math.Round(workQualityPercent, 2);
        }


        //public virtual async Task UpdateParentTaskWorkQualityAsync(ProjectTask oldTask, ProjectTask newTask)
        //{
        //    var newIsBug = newTask.Tasktypeid == 3;
        //    var oldIsBug = oldTask?.Tasktypeid == 3;

        //    // CASE 1: New bug task (create)
        //    if (oldTask == null && newIsBug && newTask.ParentTaskId != 0)
        //    {
        //        var parent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
        //        var workQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
        //        if (workQuality != null)
        //        {
        //            parent.WorkQuality = workQuality.Value;
        //            await _projectTaskRepository.UpdateAsync(parent);
        //        }
        //        return;
        //    }

        //    // CASE 2: Bug → Non-bug (tasktype changed)
        //    if (oldIsBug && !newIsBug && oldTask.ParentTaskId != 0)
        //    {
        //        var oldParent = await GetProjectTasksWithoutCacheByIdAsync(oldTask.ParentTaskId);
        //        var oldWorkQuality = await CalculateWorkQualityAsync(oldTask.ParentTaskId);
        //        if (oldWorkQuality != null)
        //        {
        //            oldParent.WorkQuality = oldWorkQuality.Value;
        //            await _projectTaskRepository.UpdateAsync(oldParent);

        //            oldTask.WorkQuality = 100.00m;
        //            await _projectTaskRepository.UpdateAsync(oldTask);

        //        }
        //        return;
        //    }

        //    // CASE 3: Non-bug → Bug (tasktype changed)
        //    if (!oldIsBug && newIsBug && newTask.ParentTaskId != 0)
        //    {
        //        var newParent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
        //        var newWorkQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
        //        if (newWorkQuality != null)
        //        {
        //            newParent.WorkQuality = newWorkQuality.Value;
        //            await _projectTaskRepository.UpdateAsync(newParent);
        //        }
        //        return;
        //    }

        //    // CASE 4: Bug task, parent changed
        //    if (newIsBug && oldTask.ParentTaskId != newTask.ParentTaskId)
        //    {
        //        if (oldTask.ParentTaskId != 0)
        //        {
        //            var oldParent = await GetProjectTasksWithoutCacheByIdAsync(oldTask.ParentTaskId);
        //            var oldWorkQuality = await CalculateWorkQualityAsync(oldTask.ParentTaskId);
        //            if (oldWorkQuality != null)
        //            {
        //                oldParent.WorkQuality = oldWorkQuality.Value;
        //                await _projectTaskRepository.UpdateAsync(oldParent);
        //            }
        //        }

        //        if (newTask.ParentTaskId != 0)
        //        {
        //            var newParent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
        //            var newWorkQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
        //            if (newWorkQuality != null)
        //            {
        //                newParent.WorkQuality = newWorkQuality.Value;
        //                await _projectTaskRepository.UpdateAsync(newParent);
        //            }
        //        }
        //        return;
        //    }

        //    // CASE 5: Bug task, parent same, just recalc
        //    if (newIsBug && newTask.ParentTaskId != 0)
        //    {
        //        var parent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
        //        var workQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
        //        if (workQuality != null)
        //        {
        //            parent.WorkQuality = workQuality.Value;
        //            await _projectTaskRepository.UpdateAsync(parent);
        //        }
        //    }

        //    // CASE 6: Not a bug → not a bug: do nothing
        //}

        public virtual async Task UpdateParentTaskWorkQualityAsync(ProjectTask oldTask, ProjectTask newTask)
        {
            var newIsBug = newTask.Tasktypeid == 3;
            var oldIsBug = oldTask?.Tasktypeid == 3;

            // ========== CASE 1: New bug task (created with parent) ==========
            if (oldTask == null && newIsBug && newTask.ParentTaskId != 0)
            {
                // DOT Update
                await UpdateDOTForTaskAsync(newTask.ParentTaskId); //parent 

                await UpdateDOTForTaskAsync(newTask.Id);

               

                // WorkQuality Update
                var parent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                var workQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                if (workQuality != null)
                {
                    parent.WorkQuality = workQuality.Value;
                    await _projectTaskRepository.UpdateAsync(parent);
                }

                return;
            }

            // ========== CASE 2: Bug → Non-bug ==========
            if (oldIsBug && !newIsBug && oldTask.ParentTaskId != 0)
            {
                // DOT Update
                await UpdateDOTForTaskAsync(oldTask.ParentTaskId);
               
                await UpdateDOTForTaskAsync(oldTask.Id);

                await SetTaskDOTTo100Async(oldTask);

                // WorkQuality Update
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

            // ========== CASE 3: Non-bug → Bug ==========
            if (!oldIsBug && newIsBug && newTask.ParentTaskId != 0)
            {
                // DOT Update
                await UpdateDOTForTaskAsync(newTask.ParentTaskId);
                await UpdateDOTForTaskAsync(newTask.Id);
                //await SetTaskDOTToNullAsync(newTask);

                // WorkQuality Update
                var newParent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                var newWorkQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                if (newWorkQuality != null)
                {
                    newParent.WorkQuality = newWorkQuality.Value;
                    await _projectTaskRepository.UpdateAsync(newParent);
                }

                return;
            }

            // ========== CASE 4: Bug reassigned to different parent ==========
            if (newIsBug && oldTask.ParentTaskId != newTask.ParentTaskId)
            {
                // DOT + WorkQuality Update for old parent
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

                // DOT + WorkQuality Update for new parent
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

            // ========== CASE 5: Bug updated under same parent ==========
            if (newIsBug && newTask.ParentTaskId != 0)
            {
                // DOT Update
                await UpdateDOTForTaskAsync(newTask.ParentTaskId);
                await UpdateDOTForTaskAsync(newTask.Id);


                // WorkQuality Update
                var parent = await GetProjectTasksWithoutCacheByIdAsync(newTask.ParentTaskId);
                var workQuality = await CalculateWorkQualityAsync(newTask.ParentTaskId);
                if (workQuality != null)
                {
                    parent.WorkQuality = workQuality.Value;
                    await _projectTaskRepository.UpdateAsync(parent);
                }

                return;
            }

            // ========== CASE 6: ID Changed (e.g. spent time change only) ==========
            if (oldTask.Id != newTask.Id)
            {
                await UpdateDOTForTaskAsync(oldTask.Id);
                await UpdateDOTForTaskAsync(newTask.Id);

                // WorkQuality usually not needed here unless your logic depends on it.
            }
            else
            {
                await UpdateDOTForTaskAsync(newTask.Id);
            }

            // CASE 7: Non-bug → Non-bug: No WorkQuality update needed.
        }

        //public virtual async Task<IList<int>> GetProjectManagerByIdsAsync(int[] projectIds)
        //{
        //    var projectmanagerIds = new List<int>();
        //    foreach(var projectid in projectIds)
        //    {
        //       var project=  await GetProjectsByIdAsync(projectid);
        //        projectmanagerIds.Add(project.ProjectManagerId);

        //    }
        //    return projectmanagerIds.ToList();
        //}

        //public virtual async Task<int> GetProjectManagerIdByIdAsync(int projectId)
        //{

        //    var project = await GetProjectsByIdAsync(projectId);
        //    var projectManagerId = project.ProjectManagerId;



        //    return projectManagerId;
        //}


        //public async Task<decimal?> CalculateDeliveryPerformanceAsync(int projectTaskId)
        //{
        //    var task = await _projectTaskRepository.GetByIdAsync(projectTaskId);
        //    if (task == null || task.EstimatedTime <= 0)
        //        return null;

        //    var spentTimeData = await GetDevelopmentTimeByTaskId(projectTaskId);
        //    var spentTimeInHours = spentTimeData.SpentHours + (spentTimeData.SpentMinutes / 60m);

        //    if (spentTimeInHours <= 0)
        //        return null;

        //    decimal bufferPercentage = _monthlyReportSetting.AllowedVariations;
        //    var adjustedEstimation = task.EstimatedTime * (1 + bufferPercentage / 100m);

        //    if (adjustedEstimation == 0)
        //        return null;

        //    // Apply linear logic: DOT% = 100 if spent <= adjusted, 0 if spent >= 2×adjusted
        //    decimal deliveryPerformance;
        //    if (spentTimeInHours <= adjustedEstimation)
        //    {
        //        deliveryPerformance = 100;
        //    }
        //    else if (spentTimeInHours >= adjustedEstimation * 2)
        //    {
        //        deliveryPerformance = 0;
        //    }
        //    else
        //    {
        //        var overRatio = (spentTimeInHours - adjustedEstimation) / adjustedEstimation;
        //        deliveryPerformance = 100 - (overRatio * 100); // Linear drop from 100 to 0
        //    }

        //    return Math.Round(deliveryPerformance, 2);
        //}


        public async Task<decimal?> CalculateDeliveryPerformanceAsync(int projectTaskId)
        {
            var task = await _projectTaskRepository.GetByIdAsync(projectTaskId);
            if (task == null || task.EstimatedTime <= 0)
                return null;

            // Get parent task time
            var parentTime = await GetDevelopmentTimeByTaskId(projectTaskId);

            // Get child bug task IDs
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

            // Normalize minutes
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
                deliveryPerformance = 100 - (overRatio * 100); // Linear drop
            }

            deliveryPerformance = Math.Clamp(deliveryPerformance, 0, 100);
            return Math.Round(deliveryPerformance, 2);
        }



        //public virtual async Task UpdateDeliveryOnTimeAsync(ProjectTask oldTask, ProjectTask newTask)
        //{
            //bool newIsBug = newTask.Tasktypeid == 3;
            //bool oldIsBug = oldTask?.Tasktypeid == 3;

            //// CASE 1: On new bug creation (with parent)
            //if (newIsBug && newTask.ParentTaskId != 0)
            //{
            //    await UpdateDOTForTaskAsync(newTask.ParentTaskId);
            //    return;
            //}

            //// CASE 2: Bug → Non-bug (task type changed)
            //if (oldIsBug && !newIsBug && oldTask.ParentTaskId != 0)
            //{
            //    await UpdateDOTForTaskAsync(oldTask.ParentTaskId);
            //    await SetTaskDOTTo100Async(oldTask);
            //    return;
            //}

            //// CASE 3: Non-bug → Bug (task type changed)
            //if (!oldIsBug && newIsBug && newTask.ParentTaskId != 0)
            //{
            //    await UpdateDOTForTaskAsync(newTask.ParentTaskId);
            //    await SetTaskDOTToNullAsync(newTask); // No children bugs yet
            //    return;
            //}

            //// CASE 4: Bug reassigned to different parent
            //if (newIsBug && oldTask.ParentTaskId != newTask.ParentTaskId)
            //{
            //    if (oldTask.ParentTaskId != 0)
            //        await UpdateDOTForTaskAsync(oldTask.ParentTaskId);

            //    if (newTask.ParentTaskId != 0)
            //        await UpdateDOTForTaskAsync(newTask.ParentTaskId);

            //    return;
            //}

            //// CASE 5: Bug updated under same parent
            //if (newIsBug && newTask.ParentTaskId != 0)
            //{
            //    await UpdateDOTForTaskAsync(newTask.ParentTaskId);
            //    return;
            //}

            //if(oldTask.Id != newTask.Id)
            //{
            //    await UpdateDOTForTaskAsync(oldTask.Id);
            //    await UpdateDOTForTaskAsync(newTask.Id);


            //}
            //else
            //{
            //    await UpdateDOTForTaskAsync(newTask.Id);
            //}
            
        //}

        private async Task UpdateDOTForTaskAsync(int taskId)
        {
            var task = await GetProjectTasksWithoutCacheByIdAsync(taskId);
            if (task == null)
                return; // Task not found, skip

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