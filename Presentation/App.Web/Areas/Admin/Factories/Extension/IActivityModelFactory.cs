using System.Threading.Tasks;
using App.Core.Domain.Activities;
using App.Web.Areas.Admin.Models.Extension.Activities;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the activity model factory interface
    /// </summary>
    public partial interface IActivityModelFactory
    {
        /// <summary>
        /// Prepares the activity search model.
        /// </summary>
        /// <param name="searchModel">Activity search model.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the prepared search model.</returns>
        Task<ActivitySearchModel> PrepareActivitySearchModelAsync(ActivitySearchModel searchModel);

        /// <summary>
        /// Prepares the activity list model.
        /// </summary>
        /// <param name="searchModel">Activity search model.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the prepared activity list model.</returns>
        Task<ActivityListModel> PrepareActivityListModelAsync(ActivitySearchModel searchModel);

        /// <summary>
        /// Prepares the activity model.
        /// </summary>
        /// <param name="model">Activity model.</param>
        /// <param name="activity">Activity entity.</param>
        /// <param name="excludeProperties">A value indicating whether to exclude properties.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the prepared activity model.</returns>
        Task<ActivityModel> PrepareActivityModelAsync(ActivityModel model, Activity activity, bool excludeProperties = false);

        /// <summary>
        /// Prepares the project list for the activity model.
        /// </summary>
        /// <param name="model">Activity model.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PrepareProjectListAsync(ActivityModel model);

        /// <summary>
        /// Prepares the employee list for the activity model.
        /// </summary>
        /// <param name="model">Activity model.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PrepareEmployeeListAsync(ActivityModel model);

        /// <summary>
        /// Prepares the project list for the activity search model.
        /// </summary>
        /// <param name="searchModel">Activity search model.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PrepareProjectListAsync(ActivitySearchModel searchModel);

        /// <summary>
        /// Prepares the project list by employee for the project task model.
        /// </summary>
        /// <param name="model">Project task model.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task PrepareProjectListByEmployeeAsync(ProjectTaskModel model);
    }
}
