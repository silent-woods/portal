using App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface IProjectTaskCategoryMappingModelFactory
    {
        Task<ProjectTaskCategoryMappingSearchModel> PrepareProjectTaskCategoryMappingSearchModelAsync(ProjectTaskCategoryMappingSearchModel searchModel);

        Task<ProjectTaskCategoryMappingListModel> PrepareProjectTaskCategoryMappingListModelAsync(ProjectTaskCategoryMappingSearchModel searchModel);

        Task<ProjectTaskCategoryMappingModel> PrepareProjectTaskCategoryMappingModelAsync(ProjectTaskCategoryMappingModel model,
            ProjectTaskCategoryMapping projectTaskCategoryMapping,
            bool excludeProperties = false);
    }
}
