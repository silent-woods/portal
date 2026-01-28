using App.Core;
using App.Data;
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
    /// <summary>
    /// Title service
    /// </summary>
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

        /// <summary>
        /// Gets all Task Comments
        /// </summary>
        /// <param name="taskid">taskid</param>
        /// <param name="statusid">statusid</param>
        /// <param name="taskid">name</param>
        /// <param name="employeeid">employeeid</param>
        /// <param name="from">from</param>
        /// <param name="to">to</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
       
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the title
        /// </returns>

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


        /// <summary>
        /// Gets a TaskComments
        /// </summary>
        /// <param name="Id">identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the campaings
        /// </returns>
        public virtual async Task<TaskComments> GetTaskCommentsByIdAsync(int Id)
        {
            return await _taskCommentsRepository.GetByIdAsync(Id);
        }

        public virtual async Task<IList<TaskComments>> GetTaskCommentsByIdsAsync(int[] taskCommentIds)
        {
            return await _taskCommentsRepository.GetByIdsAsync(taskCommentIds);
        }


        /// <summary>
        /// Inserts a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertTaskCommentsAsync(TaskComments taskComments)
        {
            await _taskCommentsRepository.InsertAsync(taskComments);


            //await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, taskComments.EmployeeId, taskComments.TaskId, taskComments.Description);

            
        }

        /// <summary>
        /// Updates the taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateTaskCommentsAsync(TaskComments taskComments)
        {
            await _taskCommentsRepository.UpdateAsync(taskComments);
        }

        /// <summary>
        /// Deletes a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteTaskCommentsAsync(TaskComments taskComments)
        {
            await _taskCommentsRepository.DeleteAsync(taskComments);
        }

        #endregion

        #endregion
    }
}