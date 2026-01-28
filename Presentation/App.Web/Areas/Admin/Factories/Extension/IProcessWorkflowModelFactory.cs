using App.Core.Domain.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Extension.ProcessWorkflows;
using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory
    /// </summary>
    public partial interface IProcessWorkflowModelFactory
    {
        Task<ProcessWorkflowSearchModel> PrepareProcessWorkflowSearchModelAsync(ProcessWorkflowSearchModel searchModel);

        Task<ProcessWorkflowListModel> PrepareProcessWorkflowListModelAsync
          (ProcessWorkflowSearchModel searchModel);

        Task<ProcessWorkflowModel> PrepareProcessWorkflowModelAsync(ProcessWorkflowModel model, ProcessWorkflow processWorkflow, bool excludeProperties = false);
    }
}