using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Employees;
using App.Core.Domain.ProjectEmployeeMappings;
using App.Core.Domain.Projects;
using App.Data;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Projects;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace App.Services.ProjectEmployeeMappings
{
    public partial class ProjectEmployeeMappingService : IProjectEmployeeMappingService
    {
        #region Fields

        private readonly IRepository<ProjectEmployeeMapping> _projectEmployeeMappingRepository;
        private readonly IProjectsService _projectsService;
        private readonly IEmployeeService _employeeService;
        private readonly IRepository<Project> _projectRepository;
        private readonly IDesignationService _designationService;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly EmployeeSettings _employeeSettings;
        #endregion

        #region Ctor
        public ProjectEmployeeMappingService(IRepository<ProjectEmployeeMapping> projectEmployeeMappingRepository, IProjectsService projectsService, IEmployeeService employeeService, IRepository<Project> projectRepository, IDesignationService designationService
, IRepository<Employee> employeeRepository, EmployeeSettings employeeSettings)
        {
            _projectEmployeeMappingRepository = projectEmployeeMappingRepository;
            _projectsService = projectsService;
            _employeeService = employeeService;
            _projectRepository = projectRepository;
            _designationService = designationService;
            _employeeRepository = employeeRepository;
            _employeeSettings = employeeSettings;
        }

        #endregion

        #region Methods
        public virtual async Task<IPagedList<ProjectEmployeeMapping>> GetAllProjectsEmployeeMappingAsync(string projectempName, int projectid,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _projectEmployeeMappingRepository.GetAllAsync(async query =>
            {
                if (projectid != 0)
                {
                    query = query.Where(b => b.ProjectId == projectid);
                }
           
                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            return new PagedList<ProjectEmployeeMapping>(query.ToList(), pageIndex, pageSize);
        }
        public virtual async Task<IList<ProjectEmployeeMapping>> GetAllProjectsEmployeeMappingByEmployeeIdAsync(int employeeId)
        {
            var query = await _projectEmployeeMappingRepository.GetAllAsync(async query =>
            {
                if (employeeId != 0)
                {
                    query = from pem in query
                            join p in _projectRepository.Table on pem.ProjectId equals p.Id
                            where pem.EmployeeId == employeeId && pem.IsActive && !p.IsDeleted && p.StatusId !=3 && p.StatusId !=4
                            select pem;
                }              
                return query.OrderByDescending(c => c.CreateOnUtc);
            });
            
            return query.ToList();
        }
        public virtual async Task<ProjectEmployeeMapping> GetProjectsEmployeeMappingByIdAsync(int projectempId)
        {
            return await _projectEmployeeMappingRepository.GetByIdAsync(projectempId, cache => default);
        }
        public virtual async Task<IList<ProjectEmployeeMapping>> GetprojectsEmployeeMappingByIdsAsync(int[] projectempIds)
        {
            return await _projectEmployeeMappingRepository.GetByIdsAsync(projectempIds, cache => default, false);
        }
        public virtual async Task InsertProjectEmployeeMappingAsync(ProjectEmployeeMapping projectEmployeeMapping)
        {
            await _projectEmployeeMappingRepository.InsertAsync(projectEmployeeMapping);
        }
        public virtual async Task UpdateProjectEmployeeMappingAsync(ProjectEmployeeMapping projectEmployeeMapping)
        {
            if (projectEmployeeMapping == null)
                throw new ArgumentNullException(nameof(projectEmployeeMapping));

            await _projectEmployeeMappingRepository.UpdateAsync(projectEmployeeMapping);
        }
        public virtual async Task DeleteProjectEmployeeMappingAsync(ProjectEmployeeMapping projectEmployeeMapping)
        {
            await _projectEmployeeMappingRepository.DeleteAsync(projectEmployeeMapping, false);
        }
        public virtual async Task<IList<int>> GetProjectManagersByEmployeeIdAsync(int employeeId)
        {
            var projects = await GetAllProjectsEmployeeMappingByEmployeeIdAsync(employeeId);
            var projectmanagerIds = new List<int>();
            foreach (var project in projects)
            {           
                projectmanagerIds.Add(await _projectsService.GetProjectManagerIdByIdAsync(project.ProjectId));              
            }
            return projectmanagerIds.ToList();
        }
        public virtual async Task<IList<int>> GetProjectLeadersByEmployeeIdAsync(int employeeId)
        {
            var projects = await GetAllProjectsEmployeeMappingByEmployeeIdAsync(employeeId);
            var projectleadersIds = new List<int>();
            foreach (var project in projects)
            {
                projectleadersIds.Add(await _projectsService.GetProjectLeaderIdByIdAsync(project.ProjectId));
            }
            return projectleadersIds.ToList();
        }

        public virtual async Task<IList<int>> GetJuniorsIdsByEmployeeIdAsync(int employeeId)
        {
            var mappings = await GetAllProjectsEmployeeMappingByEmployeeIdAsync(employeeId);
            var juniorsEmployeeIds = new List<int>();
            var projectleadersId = await _designationService.GetProjectLeaderRoleId();
            var projectManagerId = await _designationService.GetRoleIdProjectManager();
            var projectCoordinatorRoleId = _employeeSettings.CoordinatorRoleId;
            foreach (var mapping in mappings)
            {             
                    if (projectleadersId == mapping.RoleId)
                    {
                        var selectedMappings = await GetAllProjectsEmployeeMappingAsync("", mapping.ProjectId);

                    foreach (var sm in selectedMappings)
                    {
                        var employee = await _employeeService.GetEmployeeByIdAsync(sm.EmployeeId);
                        if (employee != null)
                        {
                            if (sm.IsActive && !(projectManagerId == sm.RoleId) )
                                juniorsEmployeeIds.Add(sm.EmployeeId);
                        }
                    }
                      
                    }
                    else if (projectManagerId == mapping.RoleId)
                    {
                        var selectedMappings = await GetAllProjectsEmployeeMappingAsync("", mapping.ProjectId);
                        foreach (var sm in selectedMappings)
                        {
                        if(sm.IsActive)
                            juniorsEmployeeIds.Add(sm.EmployeeId);
                        }                     
                    }
                else if (mapping.RoleId == projectCoordinatorRoleId)
                {
                    var selectedMappings = await GetAllProjectsEmployeeMappingAsync("", mapping.ProjectId);

                    juniorsEmployeeIds.AddRange(
                        selectedMappings
                            .Where(sm =>
                                sm.IsActive &&
                                sm.RoleId != projectManagerId)
                            .Select(sm => sm.EmployeeId)
                    );
                }
            }
                
                if (!juniorsEmployeeIds.Any())
                {                 
                    juniorsEmployeeIds.Add(employeeId);
                }
            
            return juniorsEmployeeIds.Distinct().ToList();
        }

        public virtual async Task<IList<int>> GetJuniorsIdsByEmployeeIdForReminderAsync(int employeeId)
        {
            var mappings = await GetAllProjectsEmployeeMappingByEmployeeIdAsync(employeeId);
            var juniorsEmployeeIds = new List<int>();
            var projectleadersId = await _designationService.GetProjectLeaderRoleId();
            var projectManagerId = await _designationService.GetRoleIdProjectManager();
            foreach (var mapping in mappings)
            {
                if (projectleadersId == mapping.RoleId)
                {
                     var selectedMappings =
          await GetAllProjectsEmployeeMappingAsync("", mapping.ProjectId);
                    foreach (var sm in selectedMappings)
                    {
                        var employee = await _employeeService.GetEmployeeByIdAsync(sm.EmployeeId);
                        if (employee != null)
                        {
                            if (sm.IsActive && !(projectManagerId == sm.RoleId) && !(projectleadersId == employee.DesignationId && employee.Id != employeeId))
                                juniorsEmployeeIds.Add(sm.EmployeeId);
                        }
                    }

                }
                else if (projectManagerId == mapping.RoleId)
                {
                    var selectedMappings =
         await GetAllProjectsEmployeeMappingAsync("", mapping.ProjectId);
                    foreach (var sm in selectedMappings)
                    {
                        if (sm.IsActive)
                            juniorsEmployeeIds.Add(sm.EmployeeId);
                    }

                }

            }

            if (!juniorsEmployeeIds.Any())
            {
                juniorsEmployeeIds.Add(employeeId);
            }

            return juniorsEmployeeIds.Distinct().ToList();
        }

        public virtual async Task<bool> CheckTeamLeaderExist(int proejctId,int roleId)
        {
            var projectleadersId = await _designationService.GetProjectLeaderRoleId();
            if(projectleadersId==roleId)
            {
                var mappings = await GetAllProjectsEmployeeMappingAsync("", proejctId);
                foreach (var sm in mappings)
                {
                    if(sm.RoleId == roleId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual async Task<bool> CheckManagerExist(int proejctId, int roleId)
        {
            var projectManagerId = await _designationService.GetRoleIdProjectManager();
            if (projectManagerId == roleId)
            {
                var mappings = await GetAllProjectsEmployeeMappingAsync("", proejctId);
                foreach (var sm in mappings)
                {
                    if (sm.RoleId == roleId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public virtual async Task<bool> IsEmployeeExist(int proejctId, int employeeId)
        {
            var mappings = await GetAllProjectsEmployeeMappingAsync("", proejctId);
           
            if(mappings != null)
                foreach(var mapping in mappings)
                {
                    if (mapping.EmployeeId == employeeId)
                        return true;
                }

            return false;
        }

        public virtual async Task<IList<int>> GetProjectIdsManagedByEmployeeIdAsync(int employeeId)
        {
            var projectLeaderRole = await _designationService.GetProjectLeaderRoleId();
            var projectManagerRole = await _designationService.GetRoleIdProjectManager();

            // Fetch the list of project IDs where the given employeeId is assigned as a leader or manager
            var projectIds = await _projectEmployeeMappingRepository.Table
                .Where(pem => pem.EmployeeId == employeeId
                              && (pem.RoleId == projectLeaderRole || pem.RoleId == projectManagerRole)
                              && pem.IsActive)
                .Select(pem => pem.ProjectId)
                .Distinct()
                .ToListAsync();

            return projectIds;
        }

        public virtual async Task<IList<int>> GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(int employeeId)
        {
            var projectLeaderRole = await _designationService.GetProjectLeaderRoleId();
            var projectManagerRole = await _designationService.GetRoleIdProjectManager();
            var projectCoordinatorRole = _employeeSettings.CoordinatorRoleId;
            var projectIds = await _projectEmployeeMappingRepository.Table
                .Where(pem => pem.EmployeeId == employeeId
                              && (pem.RoleId == projectLeaderRole || pem.RoleId == projectManagerRole || pem.RoleId == projectCoordinatorRole)
                              && pem.IsActive)
                .Select(pem => pem.ProjectId)
                .Distinct()
                .ToListAsync();

            return projectIds;
        }
        public async Task<IList<Employee>> GetEmployeesByProjectIdsAsync(int[] projectIds)
        {
            if (projectIds == null || projectIds.Length == 0)
                return new List<Employee>();

            var query = from pe in _projectEmployeeMappingRepository.Table
                        join e in _employeeRepository.Table on pe.EmployeeId equals e.Id
                        where projectIds.Contains(pe.ProjectId)
                              && pe.IsActive
                        select e;

            return await query.Distinct().ToListAsync();
        }
        public virtual async Task<IList<int>> GetProjectIdsByEmployeeAsync(int employeeId)
        {
            if (employeeId <= 0)
                return new List<int>();

            return await _projectEmployeeMappingRepository.Table
                .Where(x => x.EmployeeId == employeeId && x.IsActive)
                .Select(x => x.ProjectId)
                .Distinct()
                .ToListAsync();
        }


        public async Task<IList<int>> GetVisibleProjectIdsForDashboardAsync(int employeeId)
        {
            if (employeeId <= 0)
                return new List<int>();
            var managedProjectIds =
                await GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(employeeId);
            var assignedProjectIds =
                await GetProjectIdsByEmployeeAsync(employeeId);
            return managedProjectIds
                .Concat(assignedProjectIds)
                .Distinct()
                .ToList();
        }
        #endregion
    }
}