using App.Core.Domain.ProjectEmployeeMappings;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the projectemployeemapping model factory
    /// </summary>
    public partial interface IProjectEmployeeMappingModelFactory
    {
        Task<ProjectEmployeeMappingSearchModel> PrepareProjectEmployeeMappingSearchModelAsync(ProjectEmployeeMappingSearchModel searchModel);

        Task<ProjectEmployeeMappingListModel> PrepareProjectEmployeeMappingListModelAsync(ProjectEmployeeMappingSearchModel searchModel);

        Task<ProjectEmployeeMappingModel> PrepareProjectEmployeeMappingModelAsync(ProjectEmployeeMappingModel model, ProjectEmployeeMapping projectEmployeeMapping, bool excludeProperties = false);
    }
}