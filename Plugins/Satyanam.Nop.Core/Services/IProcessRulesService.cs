using App.Core;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Campaings service interface
    /// </summary>
    public partial interface IProcessRulesService
    {
        Task<IPagedList<ProcessRules>> GetAllProcessRulesAsync(int processWorkflowId,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<ProcessRules> GetProcessRuleByIdAsync(int Id);
        Task<IList<ProcessRules>> GetProcessRulesByIdsAsync(int[] processRuleIds);
        Task InsertProcessRuleAsync(ProcessRules processRule);
        Task UpdateProcessRuleAsync(ProcessRules processRule);
        Task DeleteProcessRuleAsync(ProcessRules processRule);
        Task<IList<int>> GetPossibleStatusIds(int processWorkflowId, int currentStatusId);
        Task<bool> IsCommentRequired(int processWorkflow, int fromState, int toState);
        Task<ProcessRules> GetRulesByStatesAsync(int processWorkflowId, int fromStateId, int toStateId);
    }
}