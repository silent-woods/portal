using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface ITaskCommentsService
    {
        Task<IPagedList<TaskComments>> GetAllTaskCommentsAsync(int taskid, int statusid, int employeeid, DateTime? from, DateTime? to,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<TaskComments> GetTaskCommentsByIdAsync(int id);
        Task InsertTaskCommentsAsync(TaskComments taskComments);
        Task UpdateTaskCommentsAsync(TaskComments taskComments);
        Task DeleteTaskCommentsAsync(TaskComments taskComments);
        Task<IList<TaskComments>> GetTaskCommentsByIdsAsync(int[] taskCommentIds);
        Task<TaskComments> GetLastCommentByTaskIdAsync(int taskId);
    }
}