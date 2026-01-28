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
    /// <summary>
    /// Title service
    /// </summary>
    public partial class ProcessRulesService : IProcessRulesService
    {
        #region Fields

        private readonly IRepository<ProcessRules> _processRulesRepository;
        private readonly IRepository<WorkflowStatus> _workflowStatusRepository;


        #endregion

        #region Ctor

        public ProcessRulesService(IRepository<ProcessRules> processRulesRepository, IRepository<WorkflowStatus> workflowStatusRepository)
        {
            _processRulesRepository = processRulesRepository;
            _workflowStatusRepository = workflowStatusRepository;
        }

        #endregion

        #region Methods

        #region ProcessRules

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

        public virtual async Task<IPagedList<ProcessRules>> GetAllProcessRulesAsync(int processWorkflowId, 
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = _processRulesRepository.Table;
          
            if (processWorkflowId !=0)
                query = query.Where(c => c.ProcessWorkflowId == processWorkflowId);

            query = query.OrderByDescending(c => c.Id);

            return await Task.FromResult(new PagedList<ProcessRules>(query.ToList(), pageIndex, pageSize));
        }


        /// <summary>
        /// Gets a TaskComments
        /// </summary>
        /// <param name="Id">identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the campaings
        /// </returns>
        public virtual async Task<ProcessRules> GetProcessRuleByIdAsync(int Id)
        {
            return await _processRulesRepository.GetByIdAsync(Id);
        }

        public virtual async Task<IList<ProcessRules>> GetProcessRulesByIdsAsync(int[] processRuleIds)
        {
            return await _processRulesRepository.GetByIdsAsync(processRuleIds);
        }


        /// <summary>
        /// Inserts a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertProcessRuleAsync(ProcessRules processRule)
        {
            await _processRulesRepository.InsertAsync(processRule);
            
        }

        /// <summary>
        /// Updates the taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateProcessRuleAsync(ProcessRules processRule)
        {
            await _processRulesRepository.UpdateAsync(processRule);
        }

        /// <summary>
        /// Deletes a taskComments
        /// </summary>
        /// <param name="taskComments">taskComments</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteProcessRuleAsync(ProcessRules processRule)
        {
            await _processRulesRepository.DeleteAsync(processRule);
        }


        public virtual async Task<IList<int>> GetPossibleStatusIds(int processWorkflowId, int currentStatusId)
        {
           
            var toStateIds = await _processRulesRepository.Table
                .Where(p => p.ProcessWorkflowId == processWorkflowId && p.FromStateId == currentStatusId)
                .Select(p => p.ToStateId)
                .Distinct()
                .ToListAsync();

            if (!toStateIds.Contains(currentStatusId))
                toStateIds.Add(currentStatusId);

          
            var allStatusEntities = await _workflowStatusRepository.Table
                .Where(s => toStateIds.Contains(s.Id))
                .ToListAsync();

          
            var orderedIds = allStatusEntities
                .OrderBy(s => s.DisplayOrder)
                .Select(s => s.Id)
                .ToList();

            return orderedIds;
        }

        public virtual async Task<bool> IsCommentRequired(int processWorkflow, int fromState, int toState)
        {
            if(fromState == toState)
            {
                return false;
            }
            var rule = await _processRulesRepository.Table
                .Where(p => p.ProcessWorkflowId == processWorkflow
                            && p.FromStateId == fromState
                            && p.ToStateId == toState)
                .Select(p => (bool?)p.IsCommentRequired) 
                .FirstOrDefaultAsync();

            return rule ?? true; 
        }

        public async Task<ProcessRules> GetRulesByStatesAsync(int processWorkflowId, int fromStateId, int toStateId)
        {
            return await _processRulesRepository.Table
                .Where(r => r.ProcessWorkflowId == processWorkflowId
                         && r.FromStateId == fromStateId
                         && r.ToStateId == toStateId
                         && r.IsActive)
                .FirstOrDefaultAsync();
        }


        #endregion

        #endregion
    }
}