using App.Core.Domain.Projects;
using App.Web.Areas.Admin.Models.Projects;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the iproject model factory
    /// </summary>
    public partial interface IProjectModelFactory
    {
        Task<ProjectSearchModel> PrepareProjectsSearchModelAsync(ProjectSearchModel searchModel);

        Task<ProjectListModel> PrepareProjectsListModelAsync(ProjectSearchModel searchModel);

        Task<ProjectModel> PrepareProjectsModelAsync(ProjectModel model, Project project, bool excludeProperties = false);
    }
}