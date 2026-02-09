using App.Core;
using App.Core.Domain.ProjectTasks;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface IWorkflowStatusService
    {
        Task<IPagedList<WorkflowStatus>> GetAllWorkflowStatusAsync(int processWorkflowId = 0, IList<string> statusNames = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<WorkflowStatus> GetWorkflowStatusByIdAsync(int Id);
        Task<IList<WorkflowStatus>> GetWorkflowStatusByIdsAsync(int[] workflowStatusIds);
        Task InsertWorkflowStatusAsync(WorkflowStatus workflowStatus);
        Task UpdateWorkflowStatusAsync(WorkflowStatus workflowStatus);
        Task DeleteWorkflowStatusAsync(WorkflowStatus workflowStatus);
        Task<int> GetDefaultStatusIdByWorkflowId(int processWorkflowId);
        Task<bool> IsTaskOverdue(int taskId);
        Task<int> GetAssignToIdByStatus(ProjectTask projectTask);
        Task<int> GetPendingCodeReviewCountAsync(int currEmployeeId);
        Task<int> GetPendingReadyToTestCountAsync(int currEmployeeId);
    }
}