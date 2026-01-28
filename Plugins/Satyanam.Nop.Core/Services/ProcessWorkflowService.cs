using App.Core;
using App.Data;
using App.Services.Messages;
using Humanizer;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LinqToDB.Sql;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Title service
    /// </summary>
    public partial class ProcessWorkflowService : IProcessWorkflowService
    {
        #region Fields

        private readonly IRepository<ProcessWorkflow> _processWorkflowRepository;
      

        #endregion

        #region Ctor

        public ProcessWorkflowService(IRepository<ProcessWorkflow> processWorkflowRepository)
        {
            _processWorkflowRepository = processWorkflowRepository;
  
        }

        #endregion

        #region Methods

        #region Campaings

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

        public virtual async Task<IPagedList<ProcessWorkflow>> GetAllProcessWorkflowsAsync(string name =null, 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _processWorkflowRepository.Table;
          
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.Name.Contains(name));

            if (!showHidden)
                query = query.Where(c => c.IsActive);

            query = query.OrderBy(c => c.DisplayOrder);

            return await Task.FromResult(new PagedList<ProcessWorkflow>(query.ToList(), pageIndex, pageSize));
        }


        /// <summary>
        /// Gets a TaskComments
        /// </summary>
        /// <param name="Id">identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the campaings
        /// </returns>
        public virtual async Task<ProcessWorkflow> GetProcessWorkflowByIdAsync(int Id)
        {
            return await _processWorkflowRepository.GetByIdAsync(Id);
        }

        public virtual async Task<IList<ProcessWorkflow>> GetProcessWorkflowsByIdsAsync(int[] processWorkflowIds)
        {
            var workflows = await _processWorkflowRepository.GetByIdsAsync(processWorkflowIds);

            // Sort by DisplayOrder (ascending)
            var orderedWorkflows = workflows
       .Where(w => w.IsActive) 
       .OrderBy(w => w.DisplayOrder)
       .ToList();


            return orderedWorkflows;
        }


        /// <summary>
        /// Inserts a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProcessWorkflowAsync(ProcessWorkflow processWorkflow)
        {
            await _processWorkflowRepository.InsertAsync(processWorkflow);
            
        }

        /// <summary>
        /// Updates the taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProcessWorkflowAsync(ProcessWorkflow processWorkflow)
        {
            await _processWorkflowRepository.UpdateAsync(processWorkflow);
        }

        /// <summary>
        /// Deletes a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProcessWorkflowAsync(ProcessWorkflow processWorkflow)
        {
            await _processWorkflowRepository.DeleteAsync(processWorkflow);
        }

        #endregion

        #endregion
    }
}