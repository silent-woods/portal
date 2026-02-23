using App.Core;
using App.Core.Domain.TaskAlerts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.TaskAlerts;

public partial interface ITaskAlertService
{
    #region Task Alert Configuration Methods

    Task InsertTaskAlertConfigurationAsync(TaskAlertConfiguration taskAlertConfiguration);

    Task UpdateTaskAlertConfigurationAsync(TaskAlertConfiguration taskAlertConfiguration);

    Task DeleteTaskAlertConfigurationAsync(TaskAlertConfiguration taskAlertConfiguration);

    Task<TaskAlertConfiguration> GetTaskAlertConfigurationByIdAsync(int id = 0);

    Task<TaskAlertConfiguration> GetNextTaskAlertConfigurationAsync(decimal percentage = decimal.Zero);

    Task<IPagedList<TaskAlertConfiguration>> GetAllTaskAlertConfigurationsAsync(int taskAlertTypeId = 0, int pageIndex = 0, int pageSize = int.MaxValue, 
        bool showHidden = false);

    #endregion

    #region Task Alert Reasons Methods

    Task InsertTaskAlertReasonAsync(TaskAlertReason taskAlertReason);

    Task UpdateTaskAlertReasonAsync(TaskAlertReason taskAlertReason);

    Task DeleteTaskAlertReasonAsync(TaskAlertReason taskAlertReason);

    Task<TaskAlertReason> GetTaskAlertReasonByIdAsync(int id = 0);

    Task<TaskAlertReason> GetTaskAlertReasonByNameAsync(string name = null);

    Task<IPagedList<TaskAlertReason>> GetAllTaskAlertReasonsAsync(string name = null, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

    #endregion

    #region Task Alert Report Methods

    Task InsertTaskAlertLogAsync(TaskAlertLog taskAlertLog);

    Task DeleteTaskAlertLogAsync(TaskAlertLog taskAlertLog);

    Task<TaskAlertLog> GetTaskAlertLogByIdAsync(int id = 0);

    Task<TaskAlertLog> GetTaskAlertLogByEmployeeIdAsync(int employeeId = 0, int alertId = 0, int taskId = 0, decimal variation = decimal.Zero);

    Task<IPagedList<TaskAlertLog>> GetAllTaskAlertLogsAsync(DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
         List<int> employeeIds = null, int taskAlertConfigurationId = 0, int followUpTaskid = 0, int taskId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

    Task<decimal> GetTaskAlertLogByAlertIdAsync(int employeeId = 0, int taskId = 0);

    Task<TaskAlertLog?> GetTaskAlertLogByEmployeeAndTaskIdAsync(int employeeId = 0, int taskId = 0);

    #endregion
}
