using App.Core.Domain.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Extension.ProcessRules;
using App.Web.Areas.Admin.Models.Extension.WorkflowStatus;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory
    /// </summary>
    public partial interface IProcessRulesModelFactory
    {
        Task<ProcessRulesSearchModel> PrepareProcessRulesSearchModelAsync(ProcessRulesSearchModel searchModel);
        Task<ProcessRulesListModel> PrepareProcessRulesListModelAsync
         (ProcessRulesSearchModel searchModel);

        Task<ProcessRulesModel> PrepareProcessRulesModelAsync(ProcessRulesModel model, ProcessRules processRules, bool excludeProperties = false);
    }
}