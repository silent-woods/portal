using App.Core;
using App.Core.Caching;
using App.Core.Domain.Activities;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.EmployeeAttendanceSetting;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Activities;
using App.Services.Configuration;
using App.Services.Designations;
using App.Services.EmployeeAttendances;
using App.Services.Helpers;
using App.Services.Holidays;
using App.Services.Leaves;
using App.Services.Projects;
using App.Services.ProjectTasks;
using LinqToDB;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Reflection.Methods.LinqToDB.Insert;

namespace App.Services.TimeSheets
{
    public partial class TimeSheetService : ITimeSheetsService
    {
        #region Fields
        private readonly IRepository<TimeSheet> _timeSheetRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Project> _projectRepository;
        private readonly IRepository<ProjectTask> _projectTaskRepository;
        private readonly IProjectTaskService _projectTaskService;
        private readonly ISettingService _settingService;
        private readonly MonthlyReportSetting _monthlyReportSettings;
        private readonly IProjectsService _projectsService;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IRepository<EmployeeAttendance> _employeeAttendanceRepository;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IActivityService _activityService;
        private readonly IDesignationService _designationService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly EmployeeAttendanceSetting _employeeAttendanceSetting;
        private readonly IHolidayService _holidayService;
        private readonly IRepository<Activity> _activityRepository;
        #endregion

        #region Ctor
        public TimeSheetService(IRepository<TimeSheet> timeSheetRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Project> projectRepository, IRepository<ProjectTask> projectTaskRepository, IProjectTaskService projectTaskService, ISettingService settingService, MonthlyReportSetting monthlyReportSetting, IProjectsService projectsService, IEmployeeAttendanceService employeeAttendanceService,
            IRepository<EmployeeAttendance> employeeAttendanceRepository,
            IDateTimeHelper dateTimeHelper,
            IActivityService activityService,
            IDesignationService designationService,
            IStaticCacheManager staticCacheManager,
            ILeaveManagementService leaveManagementService,
            EmployeeAttendanceSetting employeeAttendanceSetting,
            IHolidayService holidayService,
            IRepository<Activity> activityRepository
           )
        {
            _timeSheetRepository = timeSheetRepository;
            _employeeRepository = employeeRepository;
            _projectRepository = projectRepository;
            _projectTaskRepository = projectTaskRepository;
            _projectTaskService = projectTaskService;
            _settingService = settingService;
            _monthlyReportSettings = monthlyReportSetting;
            _projectsService = projectsService;
            _employeeAttendanceService = employeeAttendanceService;
            _employeeAttendanceRepository = employeeAttendanceRepository;
            _dateTimeHelper = dateTimeHelper;
            _activityService = activityService;
            _designationService = designationService;
            _staticCacheManager = staticCacheManager;
            _leaveManagementService = leaveManagementService;
            _employeeAttendanceSetting = employeeAttendanceSetting;
            _holidayService = holidayService;
            _activityRepository = activityRepository;
        }
        #endregion

        #region Methods
        public virtual async Task<IPagedList<TimeSheet>> GetAllTimeSheetAsync(IList<int> employeeIds =null, IList<int> projectIds=null, string taskName=null, DateTime? from =null, DateTime? to=null, int SelectedBillable=0, string activityName = null,
    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int taskId = 0)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                if (employeeIds != null && employeeIds.Any())
                {
                    query = query.Where(c => employeeIds.Contains(c.EmployeeId));
                }
                if (projectIds != null && projectIds.Any())
                {
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                }
                if (!string.IsNullOrWhiteSpace(taskName))
                {
                    var taskIds = await GetTimeSheetByTaskNameAsync(taskName);
                    query = query.Where(c => taskIds.Contains(c.TaskId));
                }
                if (!string.IsNullOrWhiteSpace(activityName))
                {
                    var activityIds = await GetTimeSheetByActivityNameAsync(activityName);
                    query = query.Where(c => activityIds.Contains(c.ActivityId));
                }
                if (SelectedBillable == 1)
                {
                    query = query.Where(c => c.Billable == true);
                }
                else if (SelectedBillable == 2)
                {
                    query = query.Where(c => c.Billable == false);
                }
                if (taskId > 0)
                {
                    query = query.Where(c => c.TaskId == taskId);
                }
                if (from.HasValue)
                    query = query.Where(pr => pr.SpentDate >= from.Value);
                if (to.HasValue)
                    query = query.Where(pr => pr.SpentDate <= to.Value);
                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            return new PagedList<TimeSheet>(query.ToList(), pageIndex, pageSize);
        }

        public async Task<int> GetTotalTimeSheetCountAsync(
            List<int> employeeIds,
            List<int> projectIds,
            string taskName,
            DateTime? from,
            DateTime? to,
            bool showHidden
        )
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                if (employeeIds != null && employeeIds.Count > 0)
                {
                    query = query.Where(c => employeeIds.Contains(c.EmployeeId));
                }
                if (projectIds != null && projectIds.Count > 0)
                {
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                }
                if (!string.IsNullOrWhiteSpace(taskName))
                {
                    var taskIds = await GetTimeSheetByTaskNameAsync(taskName);
                    query = query.Where(c => taskIds.Contains(c.TaskId));
                }
                if (from.HasValue)
                {
                    query = query.Where(pr => pr.SpentDate >= from.Value);
                }
                if (to.HasValue)
                {
                    query = query.Where(pr => pr.SpentDate <= to.Value);
                }
                return query;
            });
            return query.Count();
        }

        public async Task<IList<TimeSheet>> GetAllTimeSheetsByTaskIdsAsync(List<int> taskIds)
        {
            if (taskIds == null || !taskIds.Any())
                return new List<TimeSheet>();

            return await _timeSheetRepository.Table
                .Where(ts => taskIds.Contains(ts.TaskId))
                .ToListAsync();
        }

        public virtual async Task<IPagedList<MonthlyTimeSheetReport>> GetAllEmployeePerformanceReportAsync(int employeeId, DateTime? fromDate, DateTime? toDate, IList<int> projectIdList,
 int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, bool? isOnlyShowNotDOT = false)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.EmployeeId == employeeId);
                if (projectIdList != null && projectIdList.Count > 0 && !projectIdList.Contains(0))
                {
                    query = query.Where(c => projectIdList.Contains(c.ProjectId));
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.SpentDate >= fromDate);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(c => c.SpentDate <= toDate);
                }
                if (!showHidden)
                {
                    query = query.Where(ts =>
                        ts.TaskId == 0 ||
                        _projectTaskRepository.Table
                            .Any(t => t.Id == ts.TaskId && !t.IsDeleted));
                }
                if (isOnlyShowNotDOT == true)
                {
                    var taskIdsInTimesheet = await query
                        .Select(ts => ts.TaskId)
                        .Where(tid => tid != 0)
                        .Distinct()
                        .ToListAsync();
                    var notDOTTaskIds = await _projectTaskRepository.Table
                        .Where(task => taskIdsInTimesheet.Contains(task.Id) && task.DeliveryOnTime == false)
                        .Select(task => task.Id)
                        .ToListAsync();
                    query = query.Where(ts => notDOTTaskIds.Contains(ts.TaskId));
                }

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            query = query.ToList();
            var timeSheetReports = query.GroupBy(c => c.TaskId)
                .Select(g => new MonthlyTimeSheetReport
                {
                    EmployeeId = g.First().EmployeeId,
                    ProjectId = g.First().ProjectId,
                    TaskId = g.Key,
                    SpentDate = g.First().SpentDate,
                    Billable = g.First().Billable,
                    AllowedVariations = _monthlyReportSettings.AllowedVariations,
                    CreateOnUtc = g.First().CreateOnUtc,
                    UpdateOnUtc = g.First().UpdateOnUtc
                }).ToList();

            foreach (var timeSheetReport in timeSheetReports.ToList())
            {
                var project = await _projectsService.GetProjectsByIdAsync(timeSheetReport.ProjectId);
                var task = await _projectTaskService.GetProjectTasksByIdAsync(timeSheetReport.TaskId);                
                if (timeSheetReport == null ||
                   project == null ||
                    task == null)
                {
                    timeSheetReports.Remove(timeSheetReport);
                }
            }
            return new PagedList<MonthlyTimeSheetReport>(timeSheetReports.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<IPagedList<MonthlyTimeSheetReport>> GetAllEmployeePerformanceReportForParentTaskAsync(
      int employeeId,
      DateTime? fromDate,
      DateTime? toDate,
      IList<int> projectIdList,
      int pageIndex = 0,
      int pageSize = int.MaxValue,
      bool showHidden = false,
      bool? overridePublished = null,
      bool? isOnlyShowNotDOT = false)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.EmployeeId == employeeId);

                if (projectIdList != null && projectIdList.Count > 0 && !projectIdList.Contains(0))
                {
                    query = query.Where(c => projectIdList.Contains(c.ProjectId));
                }
                if (fromDate.HasValue)
                {
                    query = query.Where(c => c.SpentDate >= fromDate);
                }
                if (toDate.HasValue)
                {
                    query = query.Where(c => c.SpentDate <= toDate);
                }
                if (!showHidden)
                {
                    query = query.Where(ts =>
                        ts.TaskId == 0 ||
                        _projectTaskRepository.Table
                            .Any(t => t.Id == ts.TaskId && !t.IsDeleted));
                }
                if (isOnlyShowNotDOT == true)
                {
                    var taskIdsInTimesheet = await query
                        .Select(ts => ts.TaskId)
                        .Where(tid => tid != 0)
                        .Distinct()
                        .ToListAsync();
                    var notDOTTaskIds = await _projectTaskRepository.Table
                        .Where(task => taskIdsInTimesheet.Contains(task.Id) && task.DeliveryOnTime == false)
                        .Select(task => task.Id)
                        .ToListAsync();
                    query = query.Where(ts => notDOTTaskIds.Contains(ts.TaskId));
                }
                return query.OrderByDescending(c => c.CreateOnUtc);
            });

            query = query.ToList();

            var finalTimeSheetList = new List<TimeSheet>();

            foreach (var entry in query)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(entry.TaskId);
                if (task == null) continue;
                if (!showHidden && task.IsDeleted) continue;
                if (task.Tasktypeid != 3)
                {
                    finalTimeSheetList.Add(entry);
                }
                else if (task.Tasktypeid == 3 && task.ParentTaskId != 0)
                {
                    var parentTask = await _projectTaskService.GetProjectTasksByIdAsync(task.ParentTaskId);
                    if (parentTask != null && parentTask.Tasktypeid != 3)
                    {
                        finalTimeSheetList.Add(new TimeSheet
                        {
                            Id = entry.Id,
                            EmployeeId = entry.EmployeeId,
                            ProjectId = parentTask.ProjectId,
                            TaskId = parentTask.Id,
                            SpentDate = entry.SpentDate,
                            SpentHours = entry.SpentHours,
                            Billable = entry.Billable,
                            CreateOnUtc = entry.CreateOnUtc,
                            UpdateOnUtc = entry.UpdateOnUtc,
                            ActivityId = entry.ActivityId,
                            StartTime = entry.StartTime,
                            EndTime = entry.EndTime,
                            IsManualEntry = entry.IsManualEntry,
                            SpentMinutes = entry.SpentMinutes
                        });
                    }
                }
            }
            var timeSheetReports = finalTimeSheetList
                .GroupBy(c => c.TaskId)
                .Select(g => new MonthlyTimeSheetReport
                {
                    EmployeeId = g.First().EmployeeId,
                    ProjectId = g.First().ProjectId,
                    TaskId = g.Key,
                    SpentDate = g.First().SpentDate,
                    Billable = g.First().Billable,
                    AllowedVariations = _monthlyReportSettings.AllowedVariations,
                    CreateOnUtc = g.First().CreateOnUtc,
                    UpdateOnUtc = g.First().UpdateOnUtc
                }).ToList();
            foreach (var report in timeSheetReports.ToList())
            {
                var project = await _projectsService.GetProjectsByIdAsync(report.ProjectId);
                var task = await _projectTaskService.GetProjectTasksByIdAsync(report.TaskId);

                if (report == null || project == null || task == null)
                {
                    timeSheetReports.Remove(report);
                }
            }

            return new PagedList<MonthlyTimeSheetReport>(timeSheetReports, pageIndex, pageSize);
        }

        public virtual async Task<MonthlyTimeSheetReport> GetEmployeePerformanceSummaryAsync(int searchEmployeeId, DateTime? From, DateTime? To, string ProjectIds)
        {
            var projectList = new List<int>();
            if (ProjectIds != "0" && ProjectIds != null)
            {
                projectList = ProjectIds.Split(',').Select(int.Parse).ToList();
            }
            var timeSheetReports = await GetAllEmployeePerformanceReportAsync(searchEmployeeId, From, To, projectList);

            var totalTask = timeSheetReports.Count;
            var totalDeliveredOnTime = 0;
            decimal totalEstimatedHours = 0;
            decimal totalSpentHours = 0;
            decimal extraTime = 0;

            foreach (var report in timeSheetReports)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(report.TaskId);
                if (task != null)
                {
                    var spentTime = await GetSpeantTimeByEmployeeAndTaskAsync(report.EmployeeId, report.TaskId);
                    var estimatedHours = task.EstimatedTime;
                    var allowedVariations = report.AllowedVariations;
                    if (task.DeliveryOnTime)
                    {
                        totalDeliveredOnTime++;
                    }
                    totalEstimatedHours += estimatedHours;
                    totalSpentHours += (decimal)spentTime;
                    extraTime += (decimal)(spentTime - estimatedHours >= 0 ? spentTime - estimatedHours : 0);
                }
            }
            var resultPercentage = totalTask == 0 ? 0 : Math.Round((totalDeliveredOnTime / (double)totalTask) * 100, 2);
            MonthlyTimeSheetReport summary = new MonthlyTimeSheetReport
            {
                TotalTask = totalTask,
                TotalDeliveredOnTime = totalDeliveredOnTime,
                ResultPercentage = resultPercentage,
                TotalEstimatedHours = totalEstimatedHours,
                TotalSpentHours = totalSpentHours,
                ExtraTime = extraTime
            };
            return summary;
        }

        public virtual async Task<IPagedList<TimeSheetReport>> GetReportByEmployeeAsync(int employeeId, DateTime? from, DateTime? to,
    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            if (!from.HasValue || !to.HasValue)
            {
                return new PagedList<TimeSheetReport>(new List<TimeSheetReport>(), pageIndex, pageSize);
            }
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.EmployeeId == employeeId && c.SpentDate >= from && c.SpentDate <= to);
                if (!showHidden)
                {
                    query = query
                        .Join(_projectTaskRepository.Table,
                              ts => ts.TaskId,
                              pt => pt.Id,
                              (ts, pt) => new { ts, pt })
                        .Where(x =>
                            !x.pt.IsDeleted &&
                            x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                        .Select(x => x.ts);
                }

                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();

            var dateDiff = (to - from).Value.Days;

            var timeSheetReports = query.GroupBy(c => c.SpentDate)
       .Select(g =>

            new TimeSheetReport
            {
                EmployeeId = g.First().EmployeeId,
                SpentDate = g.Key,
                SpentHours = g.Sum(c => c.SpentHours),
                CreateOnUtc = g.First().CreateOnUtc,
                UpdateOnUtc = g.First().UpdateOnUtc
            }).ToList();

            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        EmployeeId = employeeId,
                        SpentDate = date,
                        SpentHours = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }

            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.FirstOrDefault() != null)
                timeSheetReports.FirstOrDefault().TotalSpentHours = timeSheetReports.Sum(x => x.SpentHours);
            return new PagedList<TimeSheetReport>(timeSheetReports.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<IList<TimeSheetReport>> GetReportByEmployeeListWithProjectsAsync(int employeeId, DateTime? from, DateTime? to, int showById, List<int> projectIds, int HoursId, List<int> taskIdList)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
               query = query.Join(_projectTaskRepository.Table,
                          ts => ts.TaskId,
                          pt => pt.Id,
                          (ts, pt) => new { ts, pt })
                .Where(x =>
                    x.ts.EmployeeId == employeeId &&
                    (!from.HasValue || x.ts.SpentDate >= from.Value) &&
                    (!to.HasValue || x.ts.SpentDate <= to.Value) &&
                    !x.pt.IsDeleted &&
                    x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                    .Select(x => x.ts);

                if (projectIds != null)
                {
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }
                if (taskIdList != null && taskIdList.Count > 0 && !taskIdList.Contains(0))
                {
                    query = query.Where(c => taskIdList.Contains(c.TaskId));
                }
                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports;
            timeSheetReports = query
                        .GroupBy(c => c.SpentDate)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,
                            TaskId = g.First().TaskId,
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc,
                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            BillableMinutes = g.Where(c => c.Billable)
                                    .Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                        })
                        .ToList();
            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        EmployeeId = employeeId,
                        SpentDate = date,
                        SpentHours = 0,
                        SpentMinutes = 0,
                        TotalMinutes = 0,
                        BillableMinutes=0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }
            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }
        public virtual async Task<IList<TimeSheetReport>> GetReportByProjectListAsync(int projectId, DateTime? from, DateTime? to, int showById, List<int> employeeIds, int HoursId, List<int> taskIdList)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                query = query
                    .Join(_projectTaskRepository.Table,
                          ts => ts.TaskId,
                          pt => pt.Id,
                          (ts, pt) => new { ts, pt })
                .Where(x =>
                    x.ts.ProjectId == projectId &&
                    (!from.HasValue || x.ts.SpentDate >= from.Value) &&
                    (!to.HasValue || x.ts.SpentDate <= to.Value) &&
                    !x.pt.IsDeleted &&
                    x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                    .Select(x => x.ts);

                if (employeeIds != null && employeeIds.Count > 0 && !employeeIds.Contains(0))
                {
                    query = query.Where(c => employeeIds.Contains(c.EmployeeId));
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }
                if (taskIdList != null && taskIdList.Count > 0 && !taskIdList.Contains(0))
                {
                    query = query.Where(c => taskIdList.Contains(c.TaskId));
                }
                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();

            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.SpentDate)
                        .Select(g => new TimeSheetReport
                        {
                            ProjectId = g.First().ProjectId,
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,
                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            TaskId = g.First().TaskId,
                            BillableMinutes = g.Where(c => c.Billable)
                            .Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc
                        })
                        .ToList();

            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        ProjectId = projectId,
                        SpentDate = date,
                        SpentHours = 0,
                        SpentMinutes = 0,
                        TotalMinutes = 0,
                        BillableMinutes = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }
            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }


        public virtual async Task<IList<TimeSheetReport>> GetReportByDateAsync(List<int> employeeId, DateTime? from, DateTime? to, List<int> projectIds, int HoursId, List<int> taskIdList)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.SpentDate >= from && c.SpentDate <= to).Where(c => !_projectTaskRepository.Table
            .Any(t => t.Id == c.TaskId && t.IsDeleted)); ;

                query = query.Join(_projectTaskRepository.Table,
           ts => ts.TaskId,
           pt => pt.Id,
           (ts, pt) => new { ts, pt }).Where(x =>
     (!from.HasValue || x.ts.SpentDate >= from.Value) &&
     (!to.HasValue || x.ts.SpentDate <= to.Value) &&
     !x.pt.IsDeleted &&
     x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
     .Select(x => x.ts);

                if (projectIds != null)
                {
                    query = query.Where(c => projectIds.Contains(c.ProjectId));
                }
                if (employeeId != null && employeeId.Count > 0 && !employeeId.Contains(0))
                {
                    query = query.Where(c => employeeId.Contains(c.EmployeeId));
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }
                if (taskIdList != null && taskIdList.Count > 0 && !taskIdList.Contains(0))
                {
                    query = query.Where(c => taskIdList.Contains(c.TaskId));
                }
                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports;
            timeSheetReports = query
                                .GroupBy(c => new { c.TaskId, c.SpentDate, c.EmployeeId })
                                .Select(g => new TimeSheetReport
                                {
                                    Id = g.First().Id,
                                    EmployeeId = g.Key.EmployeeId,
                                    SpentDate = g.Key.SpentDate,
                                    TaskId = g.Key.TaskId,
                                    ProjectId = g.First().ProjectId,
                                    TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                                    SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                                    SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                                    CreateOnUtc = g.First().CreateOnUtc,
                                    UpdateOnUtc = g.First().UpdateOnUtc
                                })
                                .ToList();
            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }



        public virtual async Task<IList<TimeSheetReport>> GetReportByEmployeeListAsync(int employeeId, DateTime? from, DateTime? to, int showById, int projectId, int HoursId)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
               query = query.Join(_projectTaskRepository.Table,
                          ts => ts.TaskId,
                          pt => pt.Id,
                          (ts, pt) => new { ts, pt })
                .Where(x =>
                    x.ts.EmployeeId == employeeId &&
                    (!from.HasValue || x.ts.SpentDate >= from.Value) &&
                    (!to.HasValue || x.ts.SpentDate <= to.Value) &&
                    !x.pt.IsDeleted &&
                    x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                    .Select(x => x.ts);

                if (projectId != 0)
                {
                    query = query.Where(c => c.ProjectId == projectId);
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }

                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.SpentDate)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,

                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            TaskId = g.First().TaskId,
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc
                        })
                        .ToList();
            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        EmployeeId = employeeId,
                        SpentDate = date,
                        SpentHours = 0,
                        TotalMinutes = 0,
                        SpentMinutes = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }

            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }


        public virtual async Task<IList<TimeSheetReport>> GetAttendanceReportByEmployeeListAsync(int employeeId, DateTime? from, DateTime? to)
        {
            var query = await _employeeAttendanceRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.EmployeeId == employeeId && c.CheckIn.Date >= from && c.CheckIn.Date <= to);

                return query.OrderBy(c => c.CheckIn.Date);
            });

            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.CheckIn.Date)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,
                            IsHalfLeave = g.Any(c => c.StatusId == 2),
                            IsLeave = g.Any(c => c.StatusId == 3),
                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc
                        })
                        .ToList();
            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        EmployeeId = employeeId,
                        SpentDate = date,
                        SpentHours = 0,
                        SpentMinutes = 0,
                        TotalMinutes = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }
            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }
        public virtual async Task<IList<TimeSheetReport>> GetReportByProjectListAsync(int employeeId, DateTime? from, DateTime? to, int showById, int projectId, int HoursId)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
               query = query.Join(_projectTaskRepository.Table,
                          ts => ts.TaskId,
                          pt => pt.Id,
                          (ts, pt) => new { ts, pt })
                .Where(x =>
                    x.ts.ProjectId == projectId &&
                    (!from.HasValue || x.ts.SpentDate >= from.Value) &&
                    (!to.HasValue || x.ts.SpentDate <= to.Value) &&
                    !x.pt.IsDeleted &&
                    x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                    .Select(x => x.ts);

                if (employeeId != 0)
                {
                    query = query.Where(c => c.EmployeeId == employeeId);
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }
                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.SpentDate)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            ProjectId = g.First().ProjectId,
                            SpentDate = g.Key,
                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            TaskId = g.First().TaskId,
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc
                        })
                        .ToList();
            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        EmployeeId = employeeId,
                        ProjectId = projectId,
                        SpentDate = date,
                        SpentHours = 0,
                        SpentMinutes = 0,
                        TotalMinutes = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }
            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }

        public virtual async Task<IList<TimeSheetReport>> GetReportByTaskListAsync(List<int> employeeIds, DateTime? from, DateTime? to, int showById, int projectId, int HoursId, int taskId, bool IsHideEmpty)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
               query = query.Join(_projectTaskRepository.Table,
                          ts => ts.TaskId,
                          pt => pt.Id,
                          (ts, pt) => new { ts, pt })
                .Where(x =>
                    x.ts.TaskId == taskId &&
                    (!from.HasValue || x.ts.SpentDate >= from.Value) &&
                    (!to.HasValue || x.ts.SpentDate <= to.Value) &&
                    !x.pt.IsDeleted &&
                    x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                    .Select(x => x.ts);

                if (projectId != 0)
                {
                    query = query.Where(c => c.ProjectId == projectId);
                }
                if (employeeIds != null && employeeIds.Count > 0 && !employeeIds.Contains(0))
                {
                    query = query.Where(c => employeeIds.Contains(c.EmployeeId));
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }
                return query.OrderBy(c => c.SpentDate);
            });

            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.SpentDate)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,
                            TaskId = g.First().TaskId,
                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            BillableMinutes = g.Where(c => c.Billable).Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc
                        })
                        .ToList();

            for (var date = from.Value; date <= to.Value; date = date.AddDays(1))
            {
                if (!timeSheetReports.Any(r => r.SpentDate == date))
                {
                    timeSheetReports.Add(new TimeSheetReport
                    {
                        SpentDate = date,
                        TaskId = taskId,
                        SpentHours = 0,
                        SpentMinutes = 0,
                        TotalMinutes = 0,
                        BillableMinutes = 0,
                        CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                        UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                    });
                }
            }
            timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }
        public virtual async Task<IList<TimeSheetReport>> GetAllReportByTaskListAsync(List<int> employeeIds, DateTime? from, DateTime? to, int showById, int projectId, int HoursId, int taskId)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
               query = query.Join(_projectTaskRepository.Table,
                          ts => ts.TaskId,
                          pt => pt.Id,
                          (ts, pt) => new { ts, pt })
                .Where(x =>
                    x.ts.TaskId == taskId &&
                    (!from.HasValue || x.ts.SpentDate >= from.Value) &&
                    (!to.HasValue || x.ts.SpentDate <= to.Value) &&
                    !x.pt.IsDeleted &&
                    x.pt.Tasktypeid != (int)TaskTypeEnum.UserStory)
                    .Select(x => x.ts);

                if (employeeIds != null && employeeIds.Count > 0 && !employeeIds.Contains(0))
                {
                    query = query.Where(c => employeeIds.Contains(c.EmployeeId));
                }
                if (HoursId == 2)
                {
                    query = query.Where(c => c.Billable == true);
                }
                if (HoursId == 3)
                {
                    query = query.Where(c => c.Billable == false);
                }

                if (taskId == 0) 
                {
                    query = query.Where(c => c.ProjectId == projectId); 
                }
                else
                {
                    query = query.Where(c => c.TaskId == taskId); 
                }

                return query.OrderBy(c => c.SpentDate);
            });
            query = query.ToList();
            IList<TimeSheetReport> timeSheetReports = query
                        .GroupBy(c => c.SpentDate)
                        .Select(g => new TimeSheetReport
                        {
                            EmployeeId = g.First().EmployeeId,
                            SpentDate = g.Key,
                            TaskId = g.First().TaskId,
                            CreateOnUtc = g.First().CreateOnUtc,
                            UpdateOnUtc = g.First().UpdateOnUtc, 
                            TotalMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                            SpentHours = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) / 60,
                            SpentMinutes = g.Sum(c => (c.SpentHours * 60) + c.SpentMinutes) % 60,
                            BillableMinutes = g.Where(c => c.Billable).Sum(c => (c.SpentHours * 60) + c.SpentMinutes),
                        })
                        .ToList();
            if (showById == 1)
            {
                foreach (var date in Enumerable.Range(0, (to.Value - from.Value).Days + 1).Select(d => from.Value.AddDays(d)))
                {
                    if (!timeSheetReports.Any(r => r.SpentDate == date))
                    {
                        timeSheetReports.Add(new TimeSheetReport
                        {
                            SpentDate = date,
                            SpentHours = 0,
                            BillableMinutes = 0,
                            TaskId = taskId,
                            CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                            UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                        });
                    }
                }
                timeSheetReports = timeSheetReports.OrderBy(c => c.SpentDate).ToList();
            }
            if (timeSheetReports.Any())
            {
                int totalMinutes = timeSheetReports.Sum(x => x.TotalMinutes);
                timeSheetReports.First().TotalSpentHours = totalMinutes / 60;
                timeSheetReports.First().TotalSpentMinutes = totalMinutes % 60;
            }
            return timeSheetReports;
        }
        public virtual async Task<IList<int>> GetTimeSheetByEmployeeNameAsync(string employeename)
        {
            var querys = (from t1 in _employeeRepository.Table
                          join t2 in _timeSheetRepository.Table
                          on t1.Id equals t2.EmployeeId
                          where t1.FirstName.Contains(employeename) || t1.LastName.Contains(employeename) || ($"{t1.FirstName} {t1.LastName}").Contains(employeename)
                          select t1.Id).Distinct().ToList();
            return querys;
        }
        public virtual async Task<IList<int>> GetTimeSheetByProjectNameAsync(string projectname)
        {
            var querys = (from t1 in _projectRepository.Table
                          join t2 in _timeSheetRepository.Table
                          on t1.Id equals t2.ProjectId
                          where t1.ProjectTitle.Contains(projectname)
                          select t1.Id).Distinct().ToList();
            return querys;
        }

        public virtual async Task<IList<int>> GetTimeSheetByTaskNameAsync(string taskName)
        {
            var querys = (from t1 in _projectTaskRepository.Table
                          join t2 in _timeSheetRepository.Table
                          on t1.Id equals t2.TaskId
                          where t1.TaskTitle.Contains(taskName)
                          select t1.Id).Distinct().ToList();
            return querys;
        }

        public virtual async Task<IList<int>> GetTimeSheetByActivityNameAsync(string activityName)
        {
            var querys = (from t1 in _activityRepository.Table
                          join t2 in _timeSheetRepository.Table
                          on t1.Id equals t2.ActivityId
                          where t1.ActivityName.Contains(activityName)
                          select t1.Id).Distinct().ToList();
            return querys;
        }
        public virtual async Task<TimeSheet> GetTimeSheetByIdAsync(int timeSheetId)
        {
            return await _timeSheetRepository.GetByIdAsync(timeSheetId, cache => default);
        }

        public virtual async Task<TimeSheet> GetTimeSheetByIdWithoutCacheAsync(int timeSheetId)
        {
            return await _timeSheetRepository.GetByIdAsync(timeSheetId);
        }
        public virtual async Task<IList<TimeSheet>> GetTimeSheetByIdsAsync(int[] timeSheetIds)
        {
            return await _timeSheetRepository.GetByIdsAsync(timeSheetIds, cache => default, false);
        }
        public virtual async Task InsertTimeSheetAsync(TimeSheet timeSheet)
        {

            await _timeSheetRepository.InsertAsync(timeSheet);

            await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(timeSheet.SpentDate, timeSheet.EmployeeId, timeSheet.SpentHours, timeSheet.SpentMinutes, timeSheet.SpentDate, timeSheet.EmployeeId);

        }
        public virtual async Task UpdateTimeSheetAsync(TimeSheet timeSheet)
        {
            if (timeSheet == null)
                throw new ArgumentNullException(nameof(timeSheet));
            var oldTimeSheet = await _timeSheetRepository.GetByIdAsync(timeSheet.Id);
            await _timeSheetRepository.UpdateAsync(timeSheet);
            if (oldTimeSheet.TaskId != timeSheet.TaskId)
            {
                var oldTask = await _projectTaskService.GetProjectTasksByIdAsync(oldTimeSheet.TaskId);
                var newTask = await _projectTaskService.GetProjectTasksByIdAsync(timeSheet.TaskId);
                await _projectTaskService.UpdateParentTaskWorkQualityAsync(oldTask, newTask);
            }         
        }

        public virtual async Task DeleteTimeSheetAsync(TimeSheet timeSheet)
        {
            var activity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
            if (activity != null)
            {
                (activity.SpentHours, activity.SpentMinutes) = await SubtractSpentTimeAsync(activity.SpentHours, activity.SpentMinutes, timeSheet.SpentHours, timeSheet.SpentMinutes);
                await _activityService.UpdateActivityAsync(activity);
            }
            await _timeSheetRepository.DeleteAsync(timeSheet, false);
            var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(timeSheet.TaskId);
            if (projectTask != null)
            {
                (projectTask.SpentHours, projectTask.SpentMinutes) = await SubtractSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes, timeSheet.SpentHours, timeSheet.SpentMinutes);
                projectTask.DeliveryOnTime = await IsTaskDeliveredOnTimeAsync(projectTask);
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
            }
            if (timeSheet != null)
            {
                await _employeeAttendanceService.DeleteEmployeeAttendanceBasedOnTimeSheetAsync(timeSheet);
            }
        }

        public virtual async Task<IPagedList<TimeSheet>> GetAllreportAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _timeSheetRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            return new PagedList<TimeSheet>(query.ToList(), pageIndex, pageSize);
        }

        public async Task<IList<ProjectTask>> GetTasksByProjectIdAsync(int projectId)
        {
            return await _projectTaskRepository.Table
                .Where(pt => pt.ProjectId == projectId)
                .ToListAsync();
        }

        public async Task<IList<TimeSheet>> GetTimeSheetByEmpAndSpentDate(int employeeId, DateTime spentDate)
        {
            return await _timeSheetRepository.Table
                .Where(ts => ts.EmployeeId == employeeId && ts.SpentDate.Date == spentDate.Date)
                .ToListAsync();
        }

        public virtual async Task<decimal> GetSpeantTimeByEmployeeAndTaskAsync(int employeeId, int taskid)
        {
            var timeSheets = await _timeSheetRepository.GetAllAsync(async query =>
            {
                query = query.Where(c => c.EmployeeId == employeeId && c.TaskId == taskid);
                return query;
            });
            var allSpentTime = timeSheets.Sum(c => c.SpentHours);
            return allSpentTime;
        }

        public virtual async Task UpdateOrCreateActivityAsync(TimeSheet timeSheet, TimeSheet existingRow, string activityName, int prevSpentHours, int spentHours, int prevSpentMinutes, int spentMinutes, int taskId)
        {
            if (string.IsNullOrWhiteSpace(activityName))
            {
                if (timeSheet?.ActivityId > 0)
                {
                    var oldActivity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
                    if (oldActivity != null)
                    {
                        (oldActivity.SpentHours, oldActivity.SpentMinutes) =
                            await SubtractSpentTimeAsync(oldActivity.SpentHours, oldActivity.SpentMinutes, prevSpentHours, prevSpentMinutes);

                        oldActivity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                        await _activityService.UpdateActivityAsync(oldActivity);
                    }
                    timeSheet.ActivityId = 0;
                    timeSheet.SpentHours = spentHours;
                    timeSheet.SpentMinutes = spentMinutes;
                    await UpdateTimeSheetAsync(timeSheet);
                }
                return; 
            }

            var existingActivity = await _activityService.GetAllActivitiesByActivityNameTaskIdAsync(activityName, taskId);
            if (existingActivity.Count != 0)
            {
                var activity = existingActivity.First();
                var activityId = activity.Id; 
                if (timeSheet != null)
                {
                    if (timeSheet.ActivityId != activity.Id)
                    {
                        if (timeSheet.ActivityId != 0)
                        {
                            var oldActivity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
                            if (oldActivity != null)
                            {                               
                                (oldActivity.SpentHours, oldActivity.SpentMinutes) = await SubtractSpentTimeAsync(oldActivity.SpentHours, oldActivity.SpentMinutes, prevSpentHours, prevSpentMinutes);
                                oldActivity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                                await _activityService.UpdateActivityAsync(oldActivity);
                            }
                        }
                        (activity.SpentHours, activity.SpentMinutes) = await AddSpentTimeAsync(activity.SpentHours, activity.SpentMinutes, spentHours, spentMinutes);
                        activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                        await _activityService.UpdateActivityAsync(activity);
                        timeSheet.ActivityId = activity.Id;
                    }
                    else
                    {
                        var diffSpentHours = spentHours - prevSpentHours;
                        var diffSpentMinutes = spentMinutes - prevSpentMinutes;

                        if (diffSpentHours != 0 || diffSpentMinutes != 0)
                        {                          
                            (activity.SpentHours, activity.SpentMinutes) = await AddSpentTimeAsync(activity.SpentHours, activity.SpentMinutes, diffSpentHours, diffSpentMinutes);
                            activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                            await _activityService.UpdateActivityAsync(activity);
                        }
                    }
                    timeSheet.SpentHours = spentHours;
                    timeSheet.SpentMinutes = spentMinutes;
                    await UpdateTimeSheetAsync(timeSheet);
                }
            }
            else
            {
                if (timeSheet.ActivityId != 0)
                {
                    var oldActivity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
                    if (oldActivity != null)
                    {
                        (oldActivity.SpentHours, oldActivity.SpentMinutes) = await SubtractSpentTimeAsync(oldActivity.SpentHours, oldActivity.SpentMinutes, prevSpentHours, prevSpentMinutes);
                        oldActivity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                        await _activityService.UpdateActivityAsync(oldActivity);
                    }
                }
                var newActivity = new Activity
                {
                    ActivityName = activityName,
                    EmployeeId = timeSheet.EmployeeId,
                    TaskId = taskId,
                    SpentHours = spentHours,
                    SpentMinutes = spentMinutes,
                    CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                    UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                };
                await _activityService.InsertActivityAsync(newActivity);
                if (timeSheet != null)
                {
                    timeSheet.ActivityId = newActivity.Id;
                    timeSheet.SpentHours = spentHours;
                    timeSheet.SpentMinutes = spentMinutes;
                    await UpdateTimeSheetAsync(timeSheet);
                }
                if (existingRow != null)
                    await UpdateTimeSheetAsync(existingRow);
            }
        }

        public async Task<List<MismatchEntryDto>> GetMismatchEntriesAsync(DateTime from, DateTime to, int employeeId, int projectId)
        {
            var employeeIds = employeeId != 0 ? new List<int> { employeeId } : new List<int>();
            var projectIds = projectId != 0 ? new List<int> { projectId } : new List<int>();
            var timeSheetEntries = await GetAllTimeSheetAsync(
                employeeIds: employeeIds,
                projectIds: projectIds,
                taskName: "",
                from: from,
                to: to,
                0
            );
            if (timeSheetEntries == null || !timeSheetEntries.Any())
                return new List<MismatchEntryDto>();
            var taskIds = timeSheetEntries
                .Where(t => t.TaskId != 0)
                .Select(t => t.TaskId)
                .Distinct()
                .ToList();
            if (!taskIds.Any())
                return new List<MismatchEntryDto>();
            var projectTasks = await _projectTaskService.GetProjectTasksByIdsAsync(taskIds);
            if (projectTasks == null || !projectTasks.Any())
                return new List<MismatchEntryDto>();
            var allTimeSheetsForTasks = await GetAllTimeSheetsByTaskIdsAsync(taskIds);
            var timeSpentByTask = allTimeSheetsForTasks
                .GroupBy(t => t.TaskId)
                .Select(group => new
                {
                    TaskId = group.Key,
                    TotalSpentHours = group.Sum(t => t.SpentHours) + (group.Sum(t => t.SpentMinutes) / 60),
                    TotalSpentMinutes = group.Sum(t => t.SpentMinutes) % 60
                })
                .ToList();
            var mismatches = new List<MismatchEntryDto>();
            foreach (var spentTime in timeSpentByTask)
            {
                var task = projectTasks.FirstOrDefault(t => t.Id == spentTime.TaskId);
                if (task != null &&
                    (spentTime.TotalSpentHours != task.SpentHours || spentTime.TotalSpentMinutes != task.SpentMinutes))
                {
                    var project = await _projectsService.GetProjectsByIdAsync(task.ProjectId);
                    string projectName = project?.ProjectTitle ?? string.Empty;
                    var actualTime = await ConvertSpentTimeAsync(spentTime.TotalSpentHours, spentTime.TotalSpentMinutes);
                    var taskTime = await ConvertSpentTimeAsync(task.SpentHours, task.SpentMinutes);

                    mismatches.Add(new MismatchEntryDto
                    {
                        TaskId = task.Id,
                        TaskName = task.TaskTitle,
                        ProjectName = projectName,
                        EstimatedTime = task.EstimatedTime,
                        ActualTime = actualTime,
                        TaskTime = taskTime
                    });
                }
            }
            return mismatches;
        }

        public async Task<decimal> GetTotalWorkingHours(int employeeId, DateTime from, DateTime to)
        {
            var leaveList = await _leaveManagementService.GetLeaveManagementsAsync(employeeId, 0, from, to, 2);
            var leaveDates = leaveList.SelectMany(leave =>
            {
                var dates = new List<(DateTime LeaveDate, bool IsHalf)>();
                var totalDays = (leave.To - leave.From).Days + 1;

                for (var date = leave.From; date <= leave.To; date = date.AddDays(1))
                {
                    bool isHalf = false;
                    if (leave.NoOfDays < totalDays && date == leave.To)
                    {
                        isHalf = true;
                    }
                    dates.Add((date, isHalf));
                }
                return dates;
            }).ToList();
            decimal dayWorkingHours = 8;
            var allDates = Enumerable.Range(0, (to - from).Days + 1)
       .Select(i => from.AddDays(i))
       .ToList();
            var weekends = allDates
                .Where(date => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                .ToList();
            var holidays = await _holidayService.GetHolidaysBetweenAsync(from, to);
            var holidayDates = holidays.Select(h => h.Date).ToList();
            var workingDays = allDates
                .Where(date => !weekends.Contains(date) && !holidayDates.Contains(date.Date))
                .ToList();
            decimal totalWorkingHours = workingDays.Count * dayWorkingHours;
            foreach (var leaveDate in leaveDates)
            {
                if (workingDays.Any(d => d.Date == leaveDate.LeaveDate.Date))
                {
                    if (leaveDate.IsHalf)
                        totalWorkingHours -= dayWorkingHours / 2;
                    else
                        totalWorkingHours -= dayWorkingHours;
                }
            }
            return totalWorkingHours;
        }

        public async Task<(int SpentHours, int SpentMinutes)> GetActualWorkingHours(int employeeId, DateTime from, DateTime to)
        {
            var timesheet = await GetAllTimeSheetAsync(new List<int> { employeeId }, null, null, from, to, 0);
            int totalMinutes = timesheet.Sum(t => (t.SpentHours * 60) + t.SpentMinutes);
            int spentHours = totalMinutes / 60;
            int spentMinutes = totalMinutes % 60;
            return (spentHours, spentMinutes);
        }
        public async Task<(DateTime from, DateTime to)> GetWeekByDate(DateTime date)
        {
            int dayOfWeek = (int)date.DayOfWeek;     
            dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek; 
            DateTime startOfCurrentWeek = date.Date.AddDays(-(dayOfWeek - 1));
            DateTime startOfPreviousWeek = startOfCurrentWeek.AddDays(-7);     
            DateTime endOfPreviousWeek = startOfPreviousWeek.AddDays(6);     
            return (startOfPreviousWeek, endOfPreviousWeek);
        }

        public async Task<IList<int>> GetEmployeeIdsWorkOnProjects(IList<int> projectIds)
        {
            if (projectIds == null || !projectIds.Any())
                return new List<int>();
            return await _timeSheetRepository.Table
                .Where(ts => projectIds.Contains(ts.ProjectId))
                .Select(ts => ts.EmployeeId)
                .Distinct()
                .ToListAsync();
        }

        #region Operations of spentTime
        public virtual async Task<(int SpentHours, int SpentMinutes)> ConvertSpentTimeAsync(string spentTime)
        {
            return await Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(spentTime) || !spentTime.Contains(":") || spentTime == null)
                {
                    throw new ArgumentException("Invalid spentTime format. Expected format: HH:MM");
                }

                var timeParts = spentTime.Split(':');
                if (timeParts.Length != 2 || !int.TryParse(timeParts[0], out int hours) || !int.TryParse(timeParts[1], out int minutes))
                {
                    throw new ArgumentException("Invalid spentTime format. Expected format: HH:MM");
                }

                return (hours, minutes);
            });
        }
        public virtual async Task<string> ConvertSpentTimeAsync(int SpentHours, int SpentMinutes)
        {
            return await Task.FromResult($"{SpentHours:D2}:{SpentMinutes:D2}");
        }
        public virtual async Task<(int SpentHours, int SpentMinutes)> AddSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes)
        {
            return await Task.Run(() =>
            {
                int totalMinutes = (prevSpentHours * 60 + prevSpentMinutes) + (spentHours * 60 + spentMinutes);
                int resultHours = totalMinutes / 60;
                int resultMinutes = totalMinutes % 60;
                return (resultHours, resultMinutes);
            });
        }

        public virtual async Task<(int SpentHours, int SpentMinutes)> SubtractSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes)
        {
            return await Task.Run(() =>
            {
                int totalPrevMinutes = (prevSpentHours * 60) + prevSpentMinutes;
                int totalSpentMinutes = (spentHours * 60) + spentMinutes;
                int remainingMinutes = totalPrevMinutes - totalSpentMinutes;
                if (remainingMinutes < 0)
                {
                    remainingMinutes = 0; 
                }
                int resultHours = remainingMinutes / 60;
                int resultMinutes = remainingMinutes % 60;
                return (resultHours, resultMinutes);
            });
        }

        public virtual async Task<decimal> ConvertToTotalHours(int spentHours, int spentMinutes)
        {
            if (spentHours < 0 || spentMinutes < 0 || spentMinutes >= 60)
            {
                throw new ArgumentException("Invalid time input. Minutes should be between 0 and 59.");
            }
            return spentHours + (spentMinutes / 60.0m);
        }
        #endregion

        public virtual async Task<string> ConvertToHHMMFormat(decimal totalHours)
        {
            if (totalHours < 0)
            {
                throw new ArgumentException("Total hours cannot be negative.");
            }
            int hours = (int)totalHours;
            int minutes = (int)Math.Round((totalHours - hours) * 60);
            string HHMM = await ConvertSpentTimeAsync(hours, minutes);
            return HHMM;
        }
        public virtual async Task<string> ConvertToHHMMFormat(int minutes)
        {
            if (minutes < 0)
            {
                throw new ArgumentException("Minutes cannot be negative.");
            }
            int hours = minutes / 60;
            int remainingMinutes = minutes % 60;
            string HHMM = await ConvertSpentTimeAsync(hours, remainingMinutes);
            return HHMM;
        }
        public virtual async Task<decimal> ConvertHHMMToDecimal(string HHMM)
        {
            var (spentHours, spentMinutes) = await ConvertSpentTimeAsync(HHMM);
            decimal hours = await ConvertToTotalHours(spentHours, spentMinutes);

            return hours;
        }
        public virtual async Task<(int SpentHours, int SpentMinutes)> GetDevelopmentTimeByTaskId(int taskId)
        {
            if (taskId <= 0)
                throw new ArgumentException("Invalid Task ID");

            var qaRoleId = await _designationService.GetQARoleId();
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();
            var devMinutesQuery = _timeSheetRepository.Table
                .Where(t => t.TaskId == taskId && !qaEmployeeIds.Contains(t.EmployeeId))
                .Select(t => (t.SpentHours * 60) + t.SpentMinutes);
            int totalDevMinutes = await devMinutesQuery.SumAsync();
            return (totalDevMinutes / 60, totalDevMinutes % 60);
        }

        public virtual async Task<(int SpentHours, int SpentMinutes)> GetBillableDevelopmentTimeByTaskId(int taskId)
        {
            if (taskId <= 0)
                throw new ArgumentException("Invalid Task ID");

            var qaRoleId = await _designationService.GetQARoleId();
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();
            var devMinutesQuery = _timeSheetRepository.Table
                .Where(t => t.TaskId == taskId && t.Billable && !qaEmployeeIds.Contains(t.EmployeeId))
                .Select(t => (t.SpentHours * 60) + t.SpentMinutes);
            int totalDevMinutes = await devMinutesQuery.SumAsync();
            return (totalDevMinutes / 60, totalDevMinutes % 60);
        }

        public virtual async Task<(int SpentHours, int SpentMinutes)> GetQATimeByTaskId(int taskId)
        {
            if (taskId <= 0)
                throw new ArgumentException("Invalid Task ID");
            var qaRoleId = await _designationService.GetQARoleId();
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();
            if (!qaEmployeeIds.Any())
                return (0, 0);
            var qaMinutesQuery = _timeSheetRepository.Table
                .Where(t => t.TaskId == taskId && qaEmployeeIds.Contains(t.EmployeeId))
                .Select(t => (t.SpentHours * 60) + t.SpentMinutes);
            int totalQaMinutes = await qaMinutesQuery.SumAsync();
            return (totalQaMinutes / 60, totalQaMinutes % 60);
        }

        public virtual async Task<(int SpentHours, int SpentMinutes)> GetBillableQATimeByTaskId(int taskId)
        {
            if (taskId <= 0)
                throw new ArgumentException("Invalid Task ID");
            var qaRoleId = await _designationService.GetQARoleId();
            var qaEmployeeIds = await _employeeRepository.Table
                .Where(e => e.EmployeeStatusId == 1 && e.DesignationId == qaRoleId)
                .Select(e => e.Id)
                .ToListAsync();
            if (!qaEmployeeIds.Any())
                return (0, 0);
            var qaMinutesQuery = _timeSheetRepository.Table
                .Where(t => t.TaskId == taskId && t.Billable && qaEmployeeIds.Contains(t.EmployeeId))
                .Select(t => (t.SpentHours * 60) + t.SpentMinutes);
            int totalQaMinutes = await qaMinutesQuery.SumAsync();
            return (totalQaMinutes / 60, totalQaMinutes % 60);
        }
        public async Task<bool> IsTaskDeliveredOnTimeAsync(ProjectTask projectTask, ProjectTask previousState = null)
        {
            if (projectTask == null)
                throw new ArgumentNullException(nameof(projectTask));

            if (projectTask.IsManualDOT)
                return projectTask.DeliveryOnTime;

            var allowedVariations = _monthlyReportSettings.AllowedVariations;
            if (previousState != null && previousState.Tasktypeid == 3 && previousState.ParentTaskId != 0 &&
                (projectTask.Tasktypeid != 3 || projectTask.ParentTaskId != previousState.ParentTaskId))
            {
                var oldParent = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(previousState.ParentTaskId);
                if (oldParent != null)
                {
                    await UpdateParentTaskDOTAsync(oldParent, excludedBugTaskId: projectTask.Id);
                }
            }
            if (projectTask.Tasktypeid == 3 && projectTask.ParentTaskId != 0)
            {
                int bugEstimatedMinutes = (int)(projectTask.EstimatedTime * 60);
                int bugAllowedVariationMinutes = (int)(bugEstimatedMinutes * (allowedVariations / 100));

                var (bugHours, bugMinutes) = await GetDevelopmentTimeByTaskId(projectTask.Id);
                int bugTotalSpentMinutes = (bugHours * 60) + bugMinutes;

                bool isBugDOT = bugTotalSpentMinutes <= (bugEstimatedMinutes + bugAllowedVariationMinutes);
                projectTask.DeliveryOnTime = isBugDOT;
                await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                var newParent = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(projectTask.ParentTaskId);
                if (newParent != null && newParent.Tasktypeid != 3)
                {
                    await UpdateParentTaskDOTAsync(newParent);
                }
                return isBugDOT;
            }
            var (ownHours, ownMinutes) = await GetDevelopmentTimeByTaskId(projectTask.Id);
            int totalSpentMinutes = (ownHours * 60) + ownMinutes;
            var childTasks = await _projectTaskService.GetProjectTasksByParentIdAsync(projectTask.Id);
            var bugChildren = childTasks?
                .Where(t => t.Tasktypeid == 3 && t.ParentTaskId == projectTask.Id)
                .ToList();
            if (bugChildren != null)
            {
                foreach (var bug in bugChildren)
                {
                    var (bh, bm) = await GetDevelopmentTimeByTaskId(bug.Id);
                    totalSpentMinutes += (bh * 60) + bm;
                }
            }
            int estimatedMinutes = (int)(projectTask.EstimatedTime * 60);
            int allowedVariationMinutes = (int)(estimatedMinutes * (allowedVariations / 100));
            bool isDeliveredOnTime = totalSpentMinutes <= (estimatedMinutes + allowedVariationMinutes);
            projectTask.DeliveryOnTime = isDeliveredOnTime;
            await _projectTaskService.UpdateProjectTaskAsync(projectTask);
            return isDeliveredOnTime;
        }

        private async Task UpdateParentTaskDOTAsync(ProjectTask parentTask, int? excludedBugTaskId = null)
        {
            var allowedVariations = _monthlyReportSettings.AllowedVariations;
            int parentEstimatedMinutes = (int)(parentTask.EstimatedTime * 60);
            int parentAllowedVariationMinutes = (int)(parentEstimatedMinutes * (allowedVariations / 100));
            var (parentHours, parentMinutes) = await GetDevelopmentTimeByTaskId(parentTask.Id);
            int parentTotalMinutes = (parentHours * 60) + parentMinutes;
            var allBugChildren = await _projectTaskService.GetProjectTasksByParentIdAsync(parentTask.Id);
            var bugChildren = allBugChildren?
                .Where(t => t.Tasktypeid == 3 && t.ParentTaskId == parentTask.Id &&
                            (!excludedBugTaskId.HasValue || t.Id != excludedBugTaskId.Value))
                .ToList();
            if (bugChildren != null)
            {
                foreach (var bug in bugChildren)
                {
                    var (bh, bm) = await GetDevelopmentTimeByTaskId(bug.Id);
                    parentTotalMinutes += (bh * 60) + bm;
                }
            }

            bool isParentDOT = parentTotalMinutes <= (parentEstimatedMinutes + parentAllowedVariationMinutes);
            parentTask.DeliveryOnTime = isParentDOT;
            await _projectTaskService.UpdateProjectTaskAsync(parentTask);
        }
        public async Task<bool> IsQALoggedIn(int employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);
            if (employee != null)
            {
                var QaRole = await _designationService.GetQARoleId();
                if (QaRole == employee.DesignationId)
                {
                    return true;
                }
                else
                    return false;
            }
            return false;
        }

        public virtual async Task SyncDOT()
        {
            var allTask = await _projectTaskService.GetAllProjectTasksAsync();
            if (allTask != null)
            {
                foreach (var task in allTask)
                {
                    bool prevDOT = task.DeliveryOnTime;
                    bool newDOT = await IsTaskDeliveredOnTimeAsync(task);
                    if (newDOT != prevDOT)
                    {
                        task.DeliveryOnTime = newDOT;
                        await _projectTaskService.UpdateProjectTaskAsync(task);
                    }
                }
            }
            await _staticCacheManager.ClearAsync();
        }

        public async Task<SpentTimeDto> GetSpentTimeWithTypesById(int taskId)
        {
            var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
            if (projectTask == null)
                throw new ArgumentException("Task not found", nameof(taskId));
            SpentTimeDto spentTimeDto = new SpentTimeDto();
            var developmentTime = await GetDevelopmentTimeByTaskId(projectTask.Id);
            var qaTime = await GetQATimeByTaskId(projectTask.Id);
            var billableDevelopment = await GetBillableDevelopmentTimeByTaskId(projectTask.Id);
            var billableQa = await GetBillableQATimeByTaskId(projectTask.Id);
            var nonBillableDevelopment = await SubtractSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes, billableDevelopment.SpentHours, billableDevelopment.SpentMinutes);
            var nonBillableQa = await SubtractSpentTimeAsync(qaTime.SpentHours, qaTime.SpentMinutes, billableQa.SpentHours, billableQa.SpentMinutes);
            var totalBillable = await AddSpentTimeAsync(billableDevelopment.SpentHours, billableDevelopment.SpentMinutes, billableQa.SpentHours, billableQa.SpentMinutes);
            var totalNonBillable = await AddSpentTimeAsync(nonBillableDevelopment.SpentHours, nonBillableDevelopment.SpentMinutes, nonBillableQa.SpentHours, nonBillableQa.SpentMinutes);
            var totalAddition = await AddSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes, qaTime.SpentHours, qaTime.SpentMinutes);
            spentTimeDto.TotalDevelopmentTime = await ConvertSpentTimeAsync(developmentTime.SpentHours, developmentTime.SpentMinutes);
            spentTimeDto.TotalQATime = await ConvertSpentTimeAsync(qaTime.SpentHours, qaTime.SpentMinutes);
            spentTimeDto.BillableDevelopmentTime = await ConvertSpentTimeAsync(billableDevelopment.SpentHours, billableDevelopment.SpentMinutes);
            spentTimeDto.NotBillableDevelopmentTime = await ConvertSpentTimeAsync(nonBillableDevelopment.SpentHours, nonBillableDevelopment.SpentMinutes);
            spentTimeDto.BillableQATime = await ConvertSpentTimeAsync(billableQa.SpentHours, billableQa.SpentMinutes);
            spentTimeDto.NotBillableQATime = await ConvertSpentTimeAsync(nonBillableQa.SpentHours, nonBillableQa.SpentMinutes);
            spentTimeDto.TotalBillableTime = await ConvertSpentTimeAsync(totalBillable.SpentHours, totalBillable.SpentMinutes);
            spentTimeDto.TotalNotBillableTime = await ConvertSpentTimeAsync(totalNonBillable.SpentHours, totalNonBillable.SpentMinutes);
            spentTimeDto.TotalSpentTime = await ConvertSpentTimeAsync(totalAddition.SpentHours, totalAddition.SpentMinutes);
            return spentTimeDto;
        }

        public async Task<(DateTime? FirstCheckIn, DateTime? LastCheckOut)> GetCheckInCheckOutAsync(int employeeId, DateTime spentDate)
        {
            var result = await _timeSheetRepository.Table
                .Where(ts =>
                    ts.EmployeeId == employeeId &&
                    ts.SpentDate.Date == spentDate.Date &&
                    ts.StartTime != null &&
                    ts.EndTime != null)
                .ToListAsync();

            if (!result.Any())
                return (null, null);

            var firstCheckIn = result.Min(x => x.StartTime);
            var lastCheckOut = result.Max(x => x.EndTime);
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            if (firstCheckIn != null)
                firstCheckIn = TimeZoneInfo.ConvertTimeFromUtc(firstCheckIn.Value, istTimeZone);
            if (lastCheckOut != null)
                lastCheckOut = TimeZoneInfo.ConvertTimeFromUtc(lastCheckOut.Value, istTimeZone);

            return (firstCheckIn, lastCheckOut);
        }

        public async Task<(DateTime? From, DateTime? To)> GetDateRange(int periodId)
        {
            if (!Enum.IsDefined(typeof(SearchPeriodEnum), periodId))
                return (null, null);
            var period = (SearchPeriodEnum)periodId;
            var currentTime = await _dateTimeHelper.GetIndianTimeAsync();
            DateTime today = currentTime.Date;

            switch (period)
            {
                case SearchPeriodEnum.Today:
                    return (today, today);

                case SearchPeriodEnum.Yesterday:
                    var yesterday = today.AddDays(-1);
                    return (yesterday, yesterday);

                case SearchPeriodEnum.CurrentWeek:
                    var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
                    var endOfWeek = startOfWeek.AddDays(6);
                    return (startOfWeek, endOfWeek);

                case SearchPeriodEnum.LastWeek:
                    var startOfLastWeek = today.AddDays(-(int)today.DayOfWeek + (today.DayOfWeek == DayOfWeek.Sunday ? -13 : -6));
                    var endOfLastWeek = startOfLastWeek.AddDays(6);
                    return (startOfLastWeek, endOfLastWeek);

                case SearchPeriodEnum.CurrentMonth:
                    var firstDayOfCurrentMonth = new DateTime(today.Year, today.Month, 1);
                    var lastDayOfCurrentMonth = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));
                    return (firstDayOfCurrentMonth, lastDayOfCurrentMonth);

                case SearchPeriodEnum.LastMonth:
                    var lastMonth = today.AddMonths(-1);
                    return (
                        new DateTime(lastMonth.Year, lastMonth.Month, 1),
                        new DateTime(lastMonth.Year, lastMonth.Month, DateTime.DaysInMonth(lastMonth.Year, lastMonth.Month))
                    );

                case SearchPeriodEnum.CurrentYear:
                    var currentFinancialYearStart = new DateTime(today.Month >= 4 ? today.Year : today.Year - 1, 4, 1);
                    var currentFinancialYearEnd = currentFinancialYearStart.AddYears(1).AddDays(-1); 
                    return (currentFinancialYearStart, currentFinancialYearEnd);

                case SearchPeriodEnum.LastYear:
                    var lastFinancialYearStart = new DateTime(today.Month >= 4 ? today.Year - 1 : today.Year - 2, 4, 1);
                    var lastFinancialYearEnd = lastFinancialYearStart.AddYears(1).AddDays(-1); 
                    return (lastFinancialYearStart, lastFinancialYearEnd);

                case SearchPeriodEnum.CustomRange:
                    return (null, null);
            }
            return (null, null);
        }
        #endregion
    }
}