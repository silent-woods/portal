using App.Core;
using App.Data;
using App.Data.Extensions;
using App.Services.Messages;
using Humanizer;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Sql;

namespace Satyanam.Nop.Core.Services
{
    public partial class TaskCommentsService : ITaskCommentsService
    {
        #region Fields

        private readonly IRepository<TaskComments> _taskCommentsRepository;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public TaskCommentsService(IRepository<TaskComments> taskCommentsRepository, IWorkflowMessageService workflowMessageService,IWorkContext workContext)
        {
            _taskCommentsRepository = taskCommentsRepository;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        #region TaskCommentService

        public virtual async Task<IPagedList<TaskComments>> GetAllTaskCommentsAsync(int taskid, int statusid, int employeeid, DateTime? from, DateTime? to,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _taskCommentsRepository.Table;
          
            if (statusid > 0)
                query = query.Where(c => c.StatusId == statusid);

            if(taskid > 0)
                query = query.Where(c => c.TaskId == taskid);

            if (statusid > 0)
                query = query.Where(c => c.StatusId == statusid);

            if (from.HasValue)
                query = query.Where(pr => from.Value <= pr.CreatedOn);
            if (to.HasValue)
                query = query.Where(pr => to.Value >= pr.CreatedOn);

            query = query.OrderByDescending(c => c.Id);

            return await Task.FromResult(new PagedList<TaskComments>(query.ToList(), pageIndex, pageSize));
        }

        public virtual async Task<TaskComments> GetTaskCommentsByIdAsync(int Id)
        {
            return await _taskCommentsRepository.GetByIdAsync(Id);
        }

        public virtual async Task<IList<TaskComments>> GetTaskCommentsByIdsAsync(int[] taskCommentIds)
        {
            return await _taskCommentsRepository.GetByIdsAsync(taskCommentIds);
        }

        public virtual async Task InsertTaskCommentsAsync(TaskComments taskComments)
        {
            await _taskCommentsRepository.InsertAsync(taskComments);
        }
        public virtual async Task UpdateTaskCommentsAsync(TaskComments taskComments)
        {
            await _taskCommentsRepository.UpdateAsync(taskComments);
        }
        public virtual async Task DeleteTaskCommentsAsync(TaskComments taskComments)
        {
            await _taskCommentsRepository.DeleteAsync(taskComments);
        }
        public async Task<TaskComments> GetLastCommentByTaskIdAsync(int taskId)
        {
            return await _taskCommentsRepository.Table
                .Where(c => c.TaskId == taskId)
                .OrderByDescending(c => c.CreatedOn)
                .FirstOrDefaultAsync();
        }
        #endregion
        #endregion
    }
}