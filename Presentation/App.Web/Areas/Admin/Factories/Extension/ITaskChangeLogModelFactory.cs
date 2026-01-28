using App.Core.Domain.ProjectEmployeeMappings;
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
    public partial interface ITaskChangeLogModelFactory
    {
        Task<TaskChangeLogSearchModel> PrepareTaskChangeLogSearchModelAsync(TaskChangeLogSearchModel searchModel);

        Task<TaskChangeLogListModel> PrepareTaskChangeLogListModelAsync
         (TaskChangeLogSearchModel searchModel);

        Task<TaskChangeLogModel> PrepareTaskChangeLogModelAsync(TaskChangeLogModel model, TaskChangeLog taskChangeLog, bool excludeProperties = false);
    }
}