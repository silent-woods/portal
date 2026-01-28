using App.Core;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using Satyanam.Nop.Core.Domains;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Nop.Core.Services
{
    /// <summary>
    /// Categorys service interface
    /// </summary>
    public partial interface ICommonPluginService
    {
        Task<IPagedList<ProjectTask>> GetAllProjectTasksAsync(
            int taskId,
            int taskTypeId,
            IList<int> employeeIds,
            IList<int> projectIds,
            string taskName,
            DateTime? from,
            DateTime? to,
            DateTime? dueDate,
            int SelectedStatusId,
            int processWorkflowId,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            bool showHidden = false,
            bool? overridePublished = null,
            int filterDeliveryOnTime = 0,
            int searchParentTaskId = 0);
        Task<MonthlyTimeSheetReport> GetEmployeePerformanceSummaryAsync(int searchEmployeeId, DateTime? From, DateTime? To, string ProjectIds);

        Task<IList<decimal>> GetOverDueEmailPercentage();

        Task<decimal> GetOverduePercentageByTimeAsync(string bigTime, string smallTime);

        Task<decimal> GetOverduePercentageByTaskIdAsync(int taskId);

        Task<IList<ProjectTask>> GetOverdueTasksByEmployeeIdAsync(int employeeId);

        Task<string> GetLearningTimeOfEmployeeByRange(int employeeId, DateTime from, DateTime to);

        Task<int> GetManaualTimeOfEmployeeByRange(int employeeId, DateTime from, DateTime to);
    }
}