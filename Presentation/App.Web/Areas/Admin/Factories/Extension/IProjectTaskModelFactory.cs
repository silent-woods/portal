using App.Core.Domain.ProjectTasks;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories.Extension
{
    public partial interface IProjectTaskModelFactory
    {
        /// <summary>
        /// Prepares the project list for a project task model
        /// </summary>
        /// <param name="model">Project task model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrepareProjectListAsync(ProjectTaskModel model);

        /// <summary>
        /// Prepares the project list for a project task search model
        /// </summary>
        /// <param name="searchModel">Project task search model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task PrepareProjectListAsync(ProjectTaskSearchModel searchModel);

        /// <summary>
        /// Prepares the project task search model
        /// </summary>
        /// <param name="searchModel">Project task search model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<ProjectTaskSearchModel> PrepareProjectTaskSearchModelAsync(ProjectTaskSearchModel searchModel);

        /// <summary>
        /// Prepares the project task list model
        /// </summary>
        /// <param name="searchModel">Project task search model</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<ProjectTaskListModel> PrepareProjectTaskListModelAsync(ProjectTaskSearchModel searchModel);

        /// <summary>
        /// Prepares the project task model
        /// </summary>
        /// <param name="model">Project task model</param>
        /// <param name="projectTask">Project task entity</param>
        /// <param name="excludeProperties">Whether to exclude some properties</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task<ProjectTaskModel> PrepareProjectTaskModelAsync(ProjectTaskModel model, ProjectTask projectTask, bool excludeProperties = false);

        Task<ProjectTaskModel> PrepareProjectTaskModelByEmployeeAsync(ProjectTaskModel model, ProjectTask projectTask, bool excludeProperties = false);
    }
}
