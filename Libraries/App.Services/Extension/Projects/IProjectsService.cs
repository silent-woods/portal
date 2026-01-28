using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Projects;

namespace App.Services.Projects
{
    /// <summary>
    /// Category service interface
    /// </summary>
    public partial interface IProjectsService
    {
        /// <summary>
        /// Gets all Projects
        /// </summary>
        Task<IPagedList<Project>> GetAllProjectsAsync(string projectName = null,
             int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        Task<Project> GetProjectsByIdAsync(int projectId);

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task InsertProjectsAsync(Project project);

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task UpdateProjectsAsync(Project project);

        /// <summary>
        /// Delete project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        Task DeleteProjectsAsync(Project project);

        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="designationIds"></param>
        /// <returns></returns>
        Task<IList<Project>> GetProjectsByIdsAsync(int[] projectIds);
        Task<int> GetProjectQAIdByIdAsync(int projectId);
        Task<int> GetProjectManagerIdByIdAsync(int projectId);
        Task<int> GetProjectLeaderIdByIdAsync(int projectId);
        Task<int> GetProjectCoordinatorIdByIdAsync(int projectId);
        Task<IList<Project>> GetAllProjectsListAsync();
        Task<IEnumerable<int>> GetAllProjectIdsAsync();
        Task<IList<Project>> GetProjectListByEmployee(int employeeId);
        Task<int> GetReviewerIdByProjectIdAsync(int projectId);
    }
}