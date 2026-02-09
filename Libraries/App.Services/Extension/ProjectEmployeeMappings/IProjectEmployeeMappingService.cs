using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.ProjectEmployeeMappings;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.ProjectEmployeeMappings
{
    public partial interface IProjectEmployeeMappingService
    {
        Task<IPagedList<ProjectEmployeeMapping>> GetAllProjectsEmployeeMappingAsync(string projectempName, int projectid,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);
        Task<ProjectEmployeeMapping> GetProjectsEmployeeMappingByIdAsync(int projectsempId);
        Task InsertProjectEmployeeMappingAsync(ProjectEmployeeMapping projectEmployeeMapping);
        Task UpdateProjectEmployeeMappingAsync(ProjectEmployeeMapping projectEmployeeMapping);
        Task DeleteProjectEmployeeMappingAsync(ProjectEmployeeMapping projectEmployeeMapping);
        Task<IList<ProjectEmployeeMapping>> GetprojectsEmployeeMappingByIdsAsync(int[] projectempIds);
        Task<IList<int>> GetProjectManagersByEmployeeIdAsync(int employeeId);
        Task<IList<int>> GetProjectLeadersByEmployeeIdAsync(int employeeId);
         Task<IList<int>> GetJuniorsIdsByEmployeeIdAsync(int employeeId);
        Task<IList<int>> GetJuniorsIdsByEmployeeIdForReminderAsync(int employeeId);
        Task<bool> CheckTeamLeaderExist(int proejctId, int roleId);
        Task<bool> CheckManagerExist(int proejctId, int roleId);
        Task<bool> IsEmployeeExist(int proejctId, int employeeId);
        Task<IList<int>> GetProjectIdsManagedByEmployeeIdAsync(int employeeId);
        Task<IList<Employee>> GetEmployeesByProjectIdsAsync(int[] projectIds);
        Task<IList<int>> GetProjectIdsManagedOrCoordinateByEmployeeIdAsync(int employeeId);
        Task<IList<int>> GetVisibleProjectIdsForDashboardAsync(int employeeId);
        Task<IList<int>> GetProjectIdsByEmployeeAsync(int employeeId);
        Task<IList<int>> GetProjectIdsQaByEmployeeIdAsync(int employeeId);
    }
}