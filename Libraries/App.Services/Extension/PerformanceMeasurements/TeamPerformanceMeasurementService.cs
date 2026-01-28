using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.ProjectEmployeeMappings;

namespace App.Services.PerformanceMeasurements
{
    /// <summary>
    /// TeamPerformanceMeasurementService service
    /// </summary>
    public partial class TeamPerformanceMeasurementService:ITeamPerformanceMeasurementService
    {
        #region Fields

        private readonly IRepository<TeamPerformanceMeasurement> _teamPerformanceRepository;
        private readonly IRepository<KPIMaster> _kPIMasterRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IDesignationService _designationService;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IRepository<TimeSheet> _timesheetRepository;
        #endregion

        #region Ctor

        public TeamPerformanceMeasurementService(IRepository<TeamPerformanceMeasurement> teamPerformanceRepository,
            IRepository<KPIMaster> kPIMasterRepository,
            IRepository<Employee> employeeRepository,
            IDesignationService designationService,
            IEmployeeService employeeService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IRepository<TimeSheet> timesheetRepository
           )
        {
            _teamPerformanceRepository = teamPerformanceRepository;
            _kPIMasterRepository = kPIMasterRepository;
            _employeeRepository = employeeRepository;
            _designationService = designationService;
            _employeeService = employeeService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _timesheetRepository = timesheetRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all TeamPerformanceMeasurement
        /// </summary>
        /// <param name="teamPerformanceName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<TeamPerformanceMeasurement>> GetAllTeamPerformanceMeasurementAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _teamPerformanceRepository.GetAllAsync(async query =>
            {
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<TeamPerformanceMeasurement>(query.ToList(), pageIndex, pageSize);
        }

        public virtual async Task<IPagedList<TeamPerformanceMeasurement>> GetAllTeamPerformanceMeasurementByEmployeeManagerIdAsync(int employeeManagerId,int monthId , int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {
            var query = await _teamPerformanceRepository.GetAllAsync(async query =>
            {
                query = query.Where(tp => tp.EmployeeManagerId == employeeManagerId && tp.MonthId == monthId);
                return query.OrderByDescending(c => c.Id);
            });
            //paging
            return new PagedList<TeamPerformanceMeasurement>(query.ToList(), pageIndex, pageSize);
        }

        /// <summary>
        /// Get TeamPerformanceMeasurement by id
        /// </summary>
        /// <param name="teamPerformanceId"></param>
        /// <returns></returns>
        public virtual async Task<TeamPerformanceMeasurement> GetTeamPerformanceMeasurementByIdAsync(int teamPerformanceId)
        {
            return await _teamPerformanceRepository.GetByIdAsync(teamPerformanceId, cache => default);
        }

        /// <summary>
        /// Get TeamPerformanceMeasurement by ids
        /// </summary>
        /// <param name="teamPerformanceIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<TeamPerformanceMeasurement>> GetTeamPerformanceMeasurementByIdsAsync(int[] teamPerformanceIds)
        {
            return await _teamPerformanceRepository.GetByIdsAsync(teamPerformanceIds, cache => default, false);
        }
        
        /// <summary>
        /// Insert TeamPerformanceMeasurement
        /// </summary>
        /// <param name="teamPerformance"></param>
        /// <returns></returns>
        public virtual async Task InsertTeamPerformanceMeasurementAsync(TeamPerformanceMeasurement teamPerformance)
        {
            await _teamPerformanceRepository.InsertAsync(teamPerformance);
        }

        /// <summary>
        /// Update TeamPerformanceMeasurement
        /// </summary>
        /// <param name="teamPerformance"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateTeamPerformanceMeasurementAsync(TeamPerformanceMeasurement teamPerformance)
        {
            if (teamPerformance == null)
                throw new ArgumentNullException(nameof(teamPerformance));

            await _teamPerformanceRepository.UpdateAsync(teamPerformance);
        }

        /// <summary>
        /// delete TeamPerformanceMeasurement by record
        /// </summary>
        /// <param name="kPIWeightage"></param>
        /// <returns></returns>
        public virtual async Task DeleteTeamPerformanceMeasurementAsync(TeamPerformanceMeasurement teamPerformance)
        {
            await _teamPerformanceRepository.DeleteAsync(teamPerformance, false);
        }

        public virtual async Task<bool> IsEmployeeCanAddRatings(int employeeId)
        {
            var managerId = await _designationService.GetRoleIdProjectManager();

            var projectLeaderId = await _designationService.GetProjectLeaderRoleId();

            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if(employee != null && managerId != 0 && projectLeaderId != 0)
            if(employee.DesignationId == managerId || employee.DesignationId == projectLeaderId)
            {
                return true;
            }
            return false;
        }

        //public virtual async Task<IList<Employee>> GetEmployeeForRatingReminder(int employeeId, int previousMonth)
        //{


        //    // Fetch all team performance measurements for the previous month
        //    var teamPerformances = await GetAllTeamPerformanceMeasurementByEmployeeManagerIdAsync(employeeId, previousMonth);


        //    // Get the list of junior employee IDs managed by the provided employeeId
        //    var employeeIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdAsync(employeeId);


        //    employeeIds.Remove(employeeId);
        //    if (!employeeIds.Any())
        //    {
        //        return new List<Employee>();
        //    }



        //    // Find employee IDs from juniors that match team performance records
        //    var matchedEmployeeIds = teamPerformances
        //        .Where(tp => employeeIds.Contains(tp.EmployeeId))
        //        .Select(tp => tp.EmployeeId)
        //        .Distinct()
        //        .ToList();

        //    if (teamPerformances.Count == 0)
        //    {
        //        // Fetch employee details for the matched IDs
        //        return await _employeeRepository.GetByIdsAsync(employeeIds);
        //    }

        //    // Fetch employee details for the matched IDs
        //    return await _employeeRepository.GetByIdsAsync(matchedEmployeeIds);
        //}

        public virtual async Task<IList<Employee>> GetEmployeeForRatingReminder(int employeeId, int previousMonth, int previousYear)
        {
            // Get the list of junior employee IDs managed by the provided employeeId
            var employeeIds = await _projectEmployeeMappingService.GetJuniorsIdsByEmployeeIdForReminderAsync(employeeId);

            // Remove the manager's own ID if it exists in the list
            employeeIds.Remove(employeeId);
            if (!employeeIds.Any())
            {
                return new List<Employee>(); // Return empty if no juniors are found
            }

            var projectIds = await _projectEmployeeMappingService.GetProjectIdsManagedByEmployeeIdAsync(employeeId);

            if (!projectIds.Any())
            {
                return new List<Employee>(); // Return empty if no projects are managed by the employee
            }

            // Fetch juniors with timesheet entries for the specific projects in the previous month
            var timesheetEntries = await _timesheetRepository.Table
                .Where(ts => employeeIds.Contains(ts.EmployeeId) && projectIds.Contains(ts.ProjectId)
                             && ts.SpentDate.Month == previousMonth && ts.SpentDate.Year == previousYear)
                .Select(ts => ts.EmployeeId)
                .Distinct()
                .ToListAsync();

            // Fetch all team performance measurements for the previous month
            var teamPerformances = await _teamPerformanceRepository.Table
                .Where(tp => timesheetEntries.Contains(tp.EmployeeId) && tp.MonthId == previousMonth
                             && tp.EmployeeManagerId == employeeId && tp.Year == previousYear)
                .Select(tp => tp.EmployeeId)
                .Distinct()
                .ToListAsync();

            // Find employees who are missing ratings for the previous month but have timesheet entries
            var unmatchedEmployeeIds = timesheetEntries
                .Where(id => !teamPerformances.Contains(id)) // Exclude those with ratings
                .ToList();

            if (!unmatchedEmployeeIds.Any())
            {
                return new List<Employee>(); // Return empty if all employees have ratings
            }

            // Fetch and return details of employees who are missing ratings
            return await _employeeRepository.GetByIdsAsync(unmatchedEmployeeIds);
        }

       
        #endregion
    }
}