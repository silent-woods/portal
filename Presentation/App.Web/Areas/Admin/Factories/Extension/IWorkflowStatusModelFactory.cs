using App.Core.Domain.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory
    /// </summary>
    public partial interface IWorkflowStatusModelFactory
    {
        Task<WorkflowStatusSearchModel> PrepareWorkflowStatusSearchModelAsync(WorkflowStatusSearchModel searchModel);
        Task<WorkflowStatusListModel> PrepareWorkflowStatusListModelAsync
          (WorkflowStatusSearchModel searchModel);

        Task<WorkflowStatusModel> PrepareWorkflowStatusModelAsync(WorkflowStatusModel model, WorkflowStatus workflowStatus, bool excludeProperties = false);
    }
}