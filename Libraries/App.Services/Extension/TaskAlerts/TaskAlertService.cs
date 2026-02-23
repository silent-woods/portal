using App.Core;
using App.Core.Domain.TaskAlerts;
using App.Data;
using App.Data.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.TaskAlerts;

public partial class TaskAlertService : ITaskAlertService
{
    #region Fields

    protected readonly IRepository<TaskAlertConfiguration> _taskAlertConfigurationRepository;
    protected readonly IRepository<TaskAlertLog> _taskAlertLogRepository;
    protected readonly IRepository<TaskAlertReason> _taskAlertReasonRepository;

    #endregion

    #region Ctor

    public TaskAlertService(IRepository<TaskAlertConfiguration> taskAlertConfigurationRepository,
        IRepository<TaskAlertLog> taskAlertLogRepository,
        IRepository<TaskAlertReason> taskAlertReasonRepository)
    {
        _taskAlertConfigurationRepository = taskAlertConfigurationRepository;
        _taskAlertLogRepository = taskAlertLogRepository;
        _taskAlertReasonRepository = taskAlertReasonRepository;
    }

    #endregion

    #region Task Alert Configuration Methods

    public virtual async Task InsertTaskAlertConfigurationAsync(TaskAlertConfiguration taskAlertConfiguration)
    {
        ArgumentNullException.ThrowIfNull(taskAlertConfiguration);

        await _taskAlertConfigurationRepository.InsertAsync(taskAlertConfiguration);
    }

    public virtual async Task UpdateTaskAlertConfigurationAsync(TaskAlertConfiguration taskAlertConfiguration)
    {
        ArgumentNullException.ThrowIfNull(taskAlertConfiguration);

        await _taskAlertConfigurationRepository.UpdateAsync(taskAlertConfiguration);
    }

    public virtual async Task DeleteTaskAlertConfigurationAsync(TaskAlertConfiguration taskAlertConfiguration)
    {
        ArgumentNullException.ThrowIfNull(taskAlertConfiguration);

        await _taskAlertConfigurationRepository.DeleteAsync(taskAlertConfiguration);
    }

    public virtual async Task<TaskAlertConfiguration> GetTaskAlertConfigurationByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _taskAlertConfigurationRepository.GetByIdAsync(id);
    }

    public virtual async Task<TaskAlertConfiguration> GetNextTaskAlertConfigurationAsync(decimal percentage = decimal.Zero)
    {
        return await _taskAlertConfigurationRepository.Table.Where(tac => tac.Percentage > percentage && tac.IsActive && !tac.Deleted).FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<TaskAlertConfiguration>> GetAllTaskAlertConfigurationsAsync(int taskAlertTypeId = 0, int pageIndex = 0,
        int pageSize = int.MaxValue, bool showHidden = false)
    {
        var taskAlertConfigurations = from tac in _taskAlertConfigurationRepository.Table
                                      where !tac.Deleted
                                      select tac;

        if (taskAlertTypeId > 0)
            taskAlertConfigurations = taskAlertConfigurations.Where(tac => tac.TaskAlertTypeId == taskAlertTypeId);

        if (showHidden)
            taskAlertConfigurations = taskAlertConfigurations.Where(tac => tac.IsActive && (tac.EnableLeaderMail || tac.EnableManagerMail));

        taskAlertConfigurations = taskAlertConfigurations.OrderBy(tac => tac.DisplayOrder);

        return await taskAlertConfigurations.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Task Alert Reasons Methods

    public virtual async Task InsertTaskAlertReasonAsync(TaskAlertReason taskAlertReason)
    {
        ArgumentNullException.ThrowIfNull(taskAlertReason);

        await _taskAlertReasonRepository.InsertAsync(taskAlertReason);
    }

    public virtual async Task UpdateTaskAlertReasonAsync(TaskAlertReason taskAlertReason)
    {
        ArgumentNullException.ThrowIfNull(taskAlertReason);

        await _taskAlertReasonRepository.UpdateAsync(taskAlertReason);
    }

    public virtual async Task DeleteTaskAlertReasonAsync(TaskAlertReason taskAlertReason)
    {
        ArgumentNullException.ThrowIfNull(taskAlertReason);

        await _taskAlertReasonRepository.DeleteAsync(taskAlertReason);
    }

    public virtual async Task<TaskAlertReason> GetTaskAlertReasonByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _taskAlertReasonRepository.GetByIdAsync(id);
    }

    public virtual async Task<TaskAlertReason> GetTaskAlertReasonByNameAsync(string name = null)
    {
        ArgumentNullException.ThrowIfNull(name);

        var taskAlertReason = from tar in _taskAlertReasonRepository.Table
                              where tar.Name == name
                              select tar;

        return await taskAlertReason.FirstOrDefaultAsync();
    }


    public virtual async Task<IPagedList<TaskAlertReason>> GetAllTaskAlertReasonsAsync(string name = null, int pageIndex = 0, int pageSize = int.MaxValue,
        bool showHidden = false)
    {
        var taskAlertReasons = from tar in _taskAlertReasonRepository.Table
                               where !tar.Deleted
                               select tar;

        if (!string.IsNullOrWhiteSpace(name))
            taskAlertReasons = taskAlertReasons.Where(tar => tar.Name.Contains(name));

        if (showHidden)
            taskAlertReasons = taskAlertReasons.Where(tar => tar.IsActive);

        taskAlertReasons = taskAlertReasons.OrderBy(tar => tar.DisplayOrder);

        return await taskAlertReasons.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Task Alert Report Methods

    public virtual async Task InsertTaskAlertLogAsync(TaskAlertLog taskAlertLog)
    {
        ArgumentNullException.ThrowIfNull(taskAlertLog);

        await _taskAlertLogRepository.InsertAsync(taskAlertLog);
    }

    public virtual async Task DeleteTaskAlertLogAsync(TaskAlertLog taskAlertLog)
    {
        ArgumentNullException.ThrowIfNull(taskAlertLog);

        await _taskAlertLogRepository.DeleteAsync(taskAlertLog);
    }

    public virtual async Task<TaskAlertLog> GetTaskAlertLogByIdAsync(int id = 0)
    {
        ArgumentNullException.ThrowIfNull(id);

        return await _taskAlertLogRepository.GetByIdAsync(id);
    }

    public virtual async Task<TaskAlertLog> GetTaskAlertLogByEmployeeIdAsync(int employeeId = 0, int alertId = 0, int taskId = 0, 
        decimal variation = decimal.Zero)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        ArgumentNullException.ThrowIfNull(alertId);

        ArgumentNullException.ThrowIfNull(taskId);

        var taskAlertLogDetails = from tal in _taskAlertLogRepository.Table
                                  where tal.EmployeeId == employeeId && tal.AlertId == alertId && tal.TaskId == taskId && tal.Variation == variation
                                  select tal;

        return await taskAlertLogDetails.FirstOrDefaultAsync();
    }

    public virtual async Task<IPagedList<TaskAlertLog>> GetAllTaskAlertLogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null, 
        List<int> employeeIds = null, int taskAlertConfigurationId = 0,int followUpTaskid = 0,int taskId=0 ,int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var taskAlertLogs = from tal in _taskAlertLogRepository.Table
                            where !tal.Deleted
                            select tal;

        if (createdFromUtc.HasValue)
            taskAlertLogs = taskAlertLogs.Where(tal => createdFromUtc.Value <= tal.CreatedOnUtc);

        if (createdToUtc.HasValue)
            taskAlertLogs = taskAlertLogs.Where(tal => createdToUtc.Value >= tal.CreatedOnUtc);

        if (taskId > 0)
            taskAlertLogs = taskAlertLogs.Where(tal => tal.TaskId == taskId);

        if (taskAlertConfigurationId > 0)
            taskAlertLogs = taskAlertLogs.Where(tal => tal.AlertId == taskAlertConfigurationId);

        if (followUpTaskid > 0)
            taskAlertLogs = taskAlertLogs.Where(tal => tal.FollowUpTaskId == followUpTaskid);

        if (employeeIds != null && employeeIds.Any())
            taskAlertLogs = taskAlertLogs.Where(tal => employeeIds.Contains(tal.EmployeeId));

        taskAlertLogs = taskAlertLogs.OrderByDescending(tal => tal.Id);

        return await taskAlertLogs.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<decimal> GetTaskAlertLogByAlertIdAsync(int employeeId = 0, int taskId = 0)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        ArgumentNullException.ThrowIfNull(taskId);

        decimal percentage = await (from tal in _taskAlertLogRepository.Table
                                join tac in _taskAlertConfigurationRepository.Table on tal.AlertId equals tac.Id
                                where tal.EmployeeId == employeeId && tal.TaskId == taskId
                                orderby tal.CreatedOnUtc descending
                                select tac.Percentage).FirstOrDefaultAsync();

        return percentage;
    }

    public virtual async Task<TaskAlertLog?> GetTaskAlertLogByEmployeeAndTaskIdAsync(int employeeId = 0, int taskId = 0)
    {
        ArgumentNullException.ThrowIfNull(employeeId);

        ArgumentNullException.ThrowIfNull(taskId);

        var existingTaskAlertLog = from tal in _taskAlertLogRepository.Table
                                   join tac in _taskAlertConfigurationRepository.Table on tal.AlertId equals tac.Id
                                   where tal.EmployeeId == employeeId && tal.TaskId == taskId
                                   orderby tal.CreatedOnUtc descending
                                   select tal;

        return await existingTaskAlertLog.FirstOrDefaultAsync();
    }

    #endregion
}
