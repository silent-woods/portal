using App.Core;
using App.Core.Domain.Designations;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Employees;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Data;
using App.Services.Designations;
using App.Services.ProjectEmployeeMappings;
using App.Services.ProjectTasks;
using Pipelines.Sockets.Unofficial.Arenas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.Projects
{
    /// <summary>
    /// Projects service
    /// </summary>
    public partial class ProjectsService : IProjectsService
    {
        #region Fields

        private readonly IRepository<Project> _projectRepository;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IRepository<ProjectEmployeeMapping> _projectEmployeeMappingRepository;
        private readonly IDesignationService _designationService;
        private readonly EmployeeSettings _employeeSettings;
        #endregion

        #region Ctor

        public ProjectsService(IRepository<Project> projectRepository, IProjectTaskService projectTaskService, IRepository<ProjectEmployeeMapping> projectEmployeeMappingRepository, IDesignationService designationService
, EmployeeSettings employeeSettings)
        {
            _projectRepository = projectRepository;
            _projectTaskService = projectTaskService;
            _projectEmployeeMappingRepository = projectEmployeeMappingRepository;
            _designationService = designationService;
            _employeeSettings = employeeSettings;
        }

        #endregion
        public virtual async Task<IList<ProjectEmployeeMapping>> GetAllProjectsEmployeeMappingByEmployeeIdAsync(int employeeId)
        {
            var query = await _projectEmployeeMappingRepository.GetAllAsync(async query =>
            {
                //if (!string.IsNullOrWhiteSpace(projectempName))
                //  query = query.Where(c => c.ProjectId.Contains(projectempName));
                if (employeeId != 0)
                {
                    query = from pem in query
                            join p in _projectRepository.Table on pem.ProjectId equals p.Id
                            where pem.EmployeeId == employeeId && pem.IsActive && !p.IsDeleted && p.StatusId != 3 && p.StatusId != 4
                            select pem;
                }

                return query.OrderByDescending(c => c.CreateOnUtc);
            });

            return query.ToList();
        }

        public virtual async Task<IPagedList<ProjectEmployeeMapping>> GetAllProjectsEmployeeMappingAsync(string projectempName, int projectid,
           int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _projectEmployeeMappingRepository.GetAllAsync(async query =>
            {
                //if (!string.IsNullOrWhiteSpace(projectempName))
                //  query = query.Where(c => c.ProjectId.Contains(projectempName));
                if (projectid != 0)
                {
                    query = query.Where(b => b.ProjectId == projectid);
                }

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<ProjectEmployeeMapping>(query.ToList(), pageIndex, pageSize);
        }
        #region Methods

        /// <summary>
        /// Get all project
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<Project>> GetAllProjectsAsync(string projectName =null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _projectRepository.GetAllAsync(async query =>
            {
                    query = query.Where(p => !p.IsDeleted);
                
                if (!string.IsNullOrWhiteSpace(projectName))
                    query = query.Where(c => c.ProjectTitle.Contains(projectName));

                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            //paging
            return new PagedList<Project>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IList<Project>> GetAllProjectsListAsync()
        {
          var query = await _projectRepository.GetAllAsync(async query =>
          {

              query = query.Where(p => !p.IsDeleted);
              return query;
          });

              return query.ToList();
          
        }
        public virtual async Task<IEnumerable<int>> GetAllProjectIdsAsync()
        {
            var projects = await GetAllProjectsListAsync();
            return projects.Select(p => p.Id);
        }
    
        /// <summary>
        /// Get project by id
        /// </summary>
        /// <param name="projectId"></param>
        /// <returns></returns>
        public virtual async Task<Project> GetProjectsByIdAsync(int projectId)
        {
  
            var project= await _projectRepository.GetByIdAsync(projectId, cache => default);
            if (project != null && project.IsDeleted)
            {
                return null; 
            }
            return project;
        }

        /// <summary>
        /// Get project by ids
        /// </summary>
        /// <param name="projectIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<Project>> GetProjectsByIdsAsync(int[] projectIds)
        {

            var projects= await _projectRepository.GetByIdsAsync(projectIds, cache => default, false);
            var filteredProjects = projects.Where(project => !project.IsDeleted).ToList();

            return filteredProjects;

        }

        /// <summary>
        /// Insert project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task InsertProjectsAsync(Project project)
        {
            await _projectRepository.InsertAsync(project);
        }

        /// <summary>
        /// Update project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateProjectsAsync(Project project)
        {
            if (project == null)
                throw new ArgumentNullException(nameof(project));

            await _projectRepository.UpdateAsync(project);
        }

        /// <summary>
        /// delete project by record
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public virtual async Task DeleteProjectsAsync(Project project)
        {


            project.IsDeleted = true;
            await _projectRepository.UpdateAsync(project);
        }



        //public virtual async Task<IList<int>> GetProjectManagerByIdsAsync(int[] projectIds)
        //{
        //    var projectmanagerIds = new List<int>();
        //    foreach(var projectid in projectIds)
        //    {
        //       var project=  await GetProjectsByIdAsync(projectid);
        //        projectmanagerIds.Add(project.ProjectManagerId);

        //    }
        //    return projectmanagerIds.ToList();
        //}

        public virtual async Task<int> GetProjectQAIdByIdAsync(int projectId)
        {
            var projectQARoleId = await _designationService.GetQARoleId();
            var mappings = await GetAllProjectsEmployeeMappingAsync("", projectId);
            int projectManagerId = 0;
            foreach (var mapping in mappings)
            {
                if (mapping.RoleId == projectQARoleId)
                    projectManagerId = mapping.EmployeeId;
            }

            return projectManagerId;
        }

        public virtual async Task<int> GetProjectManagerIdByIdAsync(int projectId)
        {
            var projectManagerRoleId = await _designationService.GetRoleIdProjectManager();
                var mappings = await GetAllProjectsEmployeeMappingAsync("",projectId);
            int projectManagerId = 0;
            foreach(var mapping in mappings)
            {
                if(mapping.RoleId == projectManagerRoleId)
                {
                    projectManagerId = mapping.EmployeeId;    
                }
            }

            return projectManagerId;
        }

        public virtual async Task<int> GetProjectLeaderIdByIdAsync(int projectId)
        {
            var projectLeaderRoleId = await _designationService.GetProjectLeaderRoleId();
            var mappings = await GetAllProjectsEmployeeMappingAsync("", projectId);
            int projectLeaderId = 0;
            foreach (var mapping in mappings)
            {
                if (mapping.RoleId == projectLeaderRoleId)
                {
                    projectLeaderId = mapping.EmployeeId;
                }
            }

            return projectLeaderId;
        }
        public virtual async Task<int> GetProjectCoordinatorIdByIdAsync(int projectId)
        {
            var projectCoordinatorRoleId = _employeeSettings.CoordinatorRoleId;
            var mappings = await GetAllProjectsEmployeeMappingAsync("", projectId);
            int projectCoordinatorId = 0;
            foreach (var mapping in mappings)
            {
                if (mapping.RoleId == projectCoordinatorRoleId)
                {
                    projectCoordinatorId = mapping.EmployeeId;
                }
            }

            return projectCoordinatorId;
        }

        public virtual async Task<IList<Project>> GetProjectListByEmployee(int employeeId)
        {
            // Get all project-employee mappings by the employee ID
            var mappings = await GetAllProjectsEmployeeMappingByEmployeeIdAsync(employeeId);

            // Initialize the list to store the projects
            var projects = new List<Project>();

            // Loop through each mapping to retrieve the associated project
            foreach (var mapping in mappings)
            {
                // Get the project by its ID
                var project = await GetProjectsByIdAsync(mapping.ProjectId);

                // Check if the project exists and is not already in the list, then add it
                if (project != null && !projects.Any(p => p.Id == project.Id))
                {
                    projects.Add(project);
                }
            }

            return projects;
        }


        public virtual async Task<int> GetReviewerIdByProjectIdAsync(int projectId)
        {
            int reviewerId = await GetProjectCoordinatorIdByIdAsync(projectId);

            return reviewerId;
        }

        #endregion
    }
}