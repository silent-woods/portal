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
    public partial interface IProcessWorkflowService
    {
        Task<IPagedList<ProcessWorkflow>> GetAllProcessWorkflowsAsync(string name = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<ProcessWorkflow> GetProcessWorkflowByIdAsync(int Id);
        Task<IList<ProcessWorkflow>> GetProcessWorkflowsByIdsAsync(int[] processWorkflowIds);
        Task InsertProcessWorkflowAsync(ProcessWorkflow processWorkflow);
        Task UpdateProcessWorkflowAsync(ProcessWorkflow processWorkflow);
        Task DeleteProcessWorkflowAsync(ProcessWorkflow processWorkflow);
    }
}