using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.ProjectTasks;
using App.Data;
using App.Data.Extensions;
using Satyanam.Nop.Core.Domains;
using Satyanam.Plugin.Misc.AzureIntegration.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AzureIntegration.Services;

public partial class AzureIntegrationService : IAzureIntegrationService
{
	#region Fields

	protected readonly IRepository<AzureSyncLog> _azureSyncLogRepository;
    protected readonly IRepository<Employee> _employeeRepository;
    protected readonly IRepository<ProjectTask> _projectTaskRepository;
    protected readonly IRepository<TaskComments> _taskCommentsRepository;

	#endregion

	#region Ctor

	public AzureIntegrationService(IRepository<AzureSyncLog> azureSyncLogRepository,
        IRepository<Employee> employeeRepository,
        IRepository<ProjectTask> projectTaskRepository,
        IRepository<TaskComments> taskCommentsRepository)
	{
		_azureSyncLogRepository = azureSyncLogRepository;
        _employeeRepository = employeeRepository;
        _projectTaskRepository = projectTaskRepository;
        _taskCommentsRepository = taskCommentsRepository;
    }

    #endregion

    #region Methods

    public virtual async Task InsertIAzureSyncLogAsync(AzureSyncLog azureSyncLog)
	{
		ArgumentNullException.ThrowIfNull(nameof(azureSyncLog));

		await _azureSyncLogRepository.InsertAsync(azureSyncLog);
	}

    public virtual async Task<IPagedList<AzureSyncLog>> GetAllAzureSyncLogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
        int pageIndex = 0, int pageSize = int.MaxValue)
	{
        var azureSyncLogs = from asl in _azureSyncLogRepository.Table
                            join pt in _projectTaskRepository.Table on asl.TaskId equals pt.Id
                            select asl;

        if (createdFromUtc.HasValue)
            azureSyncLogs = azureSyncLogs.Where(asl => createdFromUtc.Value <= asl.CreatedOnUtc);

        if (createdToUtc.HasValue)
            azureSyncLogs = azureSyncLogs.Where(asl => createdToUtc.Value >= asl.CreatedOnUtc);

        azureSyncLogs = azureSyncLogs.OrderByDescending(asl => asl.Id);

        return await azureSyncLogs.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task DeleteAzureSyncLogsAsync(IList<AzureSyncLog> azureSyncLogs)
    {
        await _azureSyncLogRepository.DeleteAsync(azureSyncLogs);
    }

    public virtual async Task<IList<AzureSyncLog>> GetAzureSyncLogByIdsAsync(int[] azureSyncLogIds)
    {
        return await _azureSyncLogRepository.GetByIdsAsync(azureSyncLogIds);
    }

    public virtual async Task ClearAzureSyncLogAsync()
    {
        await _azureSyncLogRepository.TruncateAsync();
    }

    public virtual async Task<IList<TaskComments>> GetAllTaskCommentsByTaskIdAsync(int taskId = 0)
    {
        ArgumentNullException.ThrowIfNull(nameof(taskId));

        var taskComments = from tc in _taskCommentsRepository.Table
                           join e in _employeeRepository.Table on tc.EmployeeId equals e.Id
                           where tc.TaskId == taskId
                           select tc;

        taskComments = taskComments.OrderByDescending(tcl => tcl.Id);

        return await taskComments.ToListAsync();
    }

    #endregion
}
