using App.Core;
using App.Core.Domain.ProjectTasks;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface ITaskChangeLogService
    {
        Task<IPagedList<TaskChangeLog>> GetAllTaskChangeLogAsync(int taskid, int statusid, int employeeid, int logTypeId, DateTime? from, DateTime? to, int assignedTo,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<TaskChangeLog> GetTaskChangeLogByIdAsync(int id);
        Task InsertTaskChangeLogAsync(TaskChangeLog taskChangeLog);
        Task UpdateTaskChangeLogAsync(TaskChangeLog taskChangeLog);
        Task DeleteTaskChangeLogAsync(TaskChangeLog taskChangeLog);
        Task<IList<TaskChangeLog>> GetTaskChangeLogByIdsAsync(int[] taskChangeLogIds);
        Task InsertTaskChangeLogByUpdateTaskAsync(ProjectTask prevProjectTask, ProjectTask newProjectTask, int currentEmployeeId, List<TaskChecklistItemDto> checklistEntries = null);
        Task<DateTime?> GetCurrentStatusStartDateAsync(int taskId,int statusId);
    }
}