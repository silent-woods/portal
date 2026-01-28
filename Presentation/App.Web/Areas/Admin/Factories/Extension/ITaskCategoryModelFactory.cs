using System.Threading.Tasks;
using App.Web.Areas.Admin.Models.Extension.TaskCategories;
using Satyanam.Nop.Core.Domains;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// TaskCategory model factory interface
    /// </summary>
    public partial interface ITaskCategoryModelFactory
    {
        Task<TaskCategorySearchModel> PrepareTaskCategorySearchModelAsync(TaskCategorySearchModel searchModel);
        Task<TaskCategoryListModel> PrepareTaskCategoryListModelAsync(TaskCategorySearchModel searchModel);
        Task<TaskCategoryModel> PrepareTaskCategoryModelAsync(TaskCategoryModel model, TaskCategory entity, bool excludeProperties = false);
    }
}
