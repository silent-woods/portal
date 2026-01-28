using App.Core;
using Satyanam.Nop.Core.Domains;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    public partial interface IProjectTaskCategoryMappingService
    {
        Task<IPagedList<ProjectTaskCategoryMapping>> GetAllMappingsAsync(
            int projectId = 0,
            int taskCategoryId = 0,
            bool isActive = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        Task<ProjectTaskCategoryMapping> GetMappingByIdAsync(int id);

        Task<IList<ProjectTaskCategoryMapping>> GetMappingsByIdsAsync(int[] ids);

        Task InsertMappingAsync(ProjectTaskCategoryMapping mapping);

        Task UpdateMappingAsync(ProjectTaskCategoryMapping mapping);

        Task DeleteMappingAsync(ProjectTaskCategoryMapping mapping);

        Task<bool> IsCategoryExistAsync(int projectTaskId, int categoryId);

        Task<IList<ProjectTaskCategoryMapping>> GetAllMappingsByProjectIdAsync(int projectId);

    }
}
