using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;

namespace App.Services.TimeSheets
{
    public partial interface ITimeSheetsService
    {
        Task<IPagedList<TimeSheet>> GetAllTimeSheetAsync(IList<int> employeeIds = null, IList<int> projectIds = null, string taskName = null, DateTime? from = null, DateTime? to = null, int SelectedBillable = 0, string activityName = null,int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, int taskId = 0);
        Task<TimeSheet> GetTimeSheetByIdAsync(int timesheetId);
        Task InsertTimeSheetAsync(TimeSheet timeSheet);
        Task UpdateTimeSheetAsync(TimeSheet timeSheet);
        Task DeleteTimeSheetAsync(TimeSheet timeSheet);
        Task<IList<TimeSheet>> GetTimeSheetByIdsAsync(int[] timeSheetIds);
        Task<IList<int>> GetTimeSheetByEmployeeNameAsync(string employeename);
        Task<IList<int>> GetTimeSheetByProjectNameAsync(string projectname);
        Task<int> GetTotalTimeSheetCountAsync(List<int> employeeIds,List<int> projectIds,string taskName,DateTime? from,DateTime? to,bool showHidden);
        Task<IPagedList<TimeSheet>> GetAllreportAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<IList<TimeSheet>> GetAllTimeSheetsByTaskIdsAsync(List<int> taskIds);
        Task<IList<ProjectTask>> GetTasksByProjectIdAsync(int projectId);
        Task<IList<TimeSheet>> GetTimeSheetByEmpAndSpentDate(int employeeId, DateTime spentDate);
        Task<IPagedList<MonthlyTimeSheetReport>> GetAllEmployeePerformanceReportAsync(int employeeId, DateTime? fromDate, DateTime? toDate, IList<int> projectIdList,
 int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null, bool? isOnlyShowNotDOT = false);
        Task<MonthlyTimeSheetReport> GetEmployeePerformanceSummaryAsync(int searchEmployeeId, DateTime? From, DateTime? To, string ProjectIds);
        Task<decimal> GetSpeantTimeByEmployeeAndTaskAsync(int employeeId, int taskid);     
        Task<IPagedList<TimeSheetReport>> GetReportByEmployeeAsync(int employeeId, DateTime? from, DateTime? to,
    int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<IList<TimeSheetReport>> GetReportByEmployeeListAsync(int employeeId, DateTime? from, DateTime? to, int showById, int projectId, int HoursId);
        Task<IList<TimeSheetReport>> GetReportByProjectListAsync(int employeeId, DateTime? from, DateTime? to, int showById, int projectId, int HoursId);
        Task<IList<TimeSheetReport>> GetReportByTaskListAsync(List<int> employeeIds, DateTime? from, DateTime? to, int showById, int projectId, int HoursId, int taskId, bool IsHideEmpty);
        Task<IList<TimeSheetReport>> GetAllReportByTaskListAsync(List<int> employeeIds, DateTime? from, DateTime? to, int showById, int projectId, int HoursId, int taskId);
        Task<IList<TimeSheetReport>> GetReportByEmployeeListWithProjectsAsync(int employeeId, DateTime? from, DateTime? to, int showById, List<int> projectIds, int HoursId, List<int> taskIdList);
        Task<IList<TimeSheetReport>> GetReportByDateAsync(List<int> employeeId, DateTime? from, DateTime? to, List<int> projectIds, int HoursId, List<int> taskIdList);
        Task<IList<TimeSheetReport>> GetReportByProjectListAsync(int projectId, DateTime? from, DateTime? to, int showById, List<int> employeeIds, int HoursId, List<int> taskIdList);
        Task<IList<TimeSheetReport>> GetAttendanceReportByEmployeeListAsync(int employeeId, DateTime? from, DateTime? to);
        Task<TimeSheet> GetTimeSheetByIdWithoutCacheAsync(int timeSheetId);
        Task<List<MismatchEntryDto>> GetMismatchEntriesAsync(DateTime from, DateTime to, int employeeId, int projectId);
        Task UpdateOrCreateActivityAsync(TimeSheet timeSheet, TimeSheet existingRow, string activityName, int prevSpentHours, int spentHours, int prevSpentMinutes, int spentMinutes, int taskId);
        Task<(int SpentHours, int SpentMinutes)> ConvertSpentTimeAsync(string spentTime);
        Task<string> ConvertSpentTimeAsync(int SpentHours, int SpentMinutes);
        Task<(int SpentHours, int SpentMinutes)> AddSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes);
        Task<(int SpentHours, int SpentMinutes)> SubtractSpentTimeAsync(
    int prevSpentHours, int prevSpentMinutes, int spentHours, int spentMinutes);
        Task<decimal> ConvertToTotalHours(int spentHours, int spentMinutes);
        Task<(int SpentHours, int SpentMinutes)> GetDevelopmentTimeByTaskId(int TaskId);
        Task<(int SpentHours, int SpentMinutes)> GetBillableDevelopmentTimeByTaskId(int taskId);
        Task<(int SpentHours, int SpentMinutes)> GetQATimeByTaskId(int TaskId);
        Task<(int SpentHours, int SpentMinutes)> GetBillableQATimeByTaskId(int taskId);
        Task<bool> IsTaskDeliveredOnTimeAsync(ProjectTask projectTask, ProjectTask previousState = null);
        Task<bool> IsQALoggedIn(int employeeId);
        Task<string> ConvertToHHMMFormat(decimal totalHours);
        Task<string> ConvertToHHMMFormat(int minutes);
        Task<decimal> ConvertHHMMToDecimal(string HHMM);
        Task SyncDOT();
        Task<decimal> GetTotalWorkingHours(int employeeId, DateTime from, DateTime to);
        Task<(int SpentHours, int SpentMinutes)> GetActualWorkingHours(int employeeId, DateTime from, DateTime to);
        Task<(DateTime from, DateTime to)> GetWeekByDate(DateTime date);
        Task<IList<int>> GetEmployeeIdsWorkOnProjects(IList<int> projectIds);
        Task<SpentTimeDto> GetSpentTimeWithTypesById(int taskId);
        Task<(DateTime? FirstCheckIn, DateTime? LastCheckOut)> GetCheckInCheckOutAsync(int employeeId, DateTime spentDate);
        Task<(DateTime? From, DateTime? To)> GetDateRange(int periodId);
        Task<IPagedList<MonthlyTimeSheetReport>> GetAllEmployeePerformanceReportForParentTaskAsync(int employeeId, DateTime? fromDate, DateTime? toDate, IList<int> projectIdList, int pageIndex = 0, int pageSize = int.MaxValue,bool showHidden = false, bool? overridePublished = null, bool? isOnlyShowNotDOT = false);
    }
}