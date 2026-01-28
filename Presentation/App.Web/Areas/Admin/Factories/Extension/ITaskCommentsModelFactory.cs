using App.Core.Domain.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory
    /// </summary>
    public partial interface ITaskCommentsModelFactory
    {
        Task<TaskCommentsSearchModel> PrepareTaskCommentsSearchModelAsync(TaskCommentsSearchModel searchModel);

        Task<TaskCommentsListModel> PrepareTaskCommentsListModelAsync
          (TaskCommentsSearchModel searchModel);

        Task<TaskCommentsModel> PrepareTaskCommentsModelAsync(TaskCommentsModel model, TaskComments taskComments, bool excludeProperties = false);
    }
}