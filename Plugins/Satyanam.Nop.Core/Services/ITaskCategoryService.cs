using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface ITaskCategoryService
    {
        Task<IPagedList<TaskCategory>> GetAllTaskCategoriesAsync(
              string categoryName = null,
              bool isActive = false,
              int pageIndex = 0,
              int pageSize = int.MaxValue);

        Task<TaskCategory> GetTaskCategoryByIdAsync(int id);

        Task<IList<TaskCategory>> GetTaskCategoriesByIdsAsync(int[] ids);

        Task InsertTaskCategoryAsync(TaskCategory taskCategory);

        Task UpdateTaskCategoryAsync(TaskCategory taskCategory);

        Task DeleteTaskCategoryAsync(TaskCategory taskCategory);

    }
}
