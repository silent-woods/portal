using App.Core.Domain.TaskAlerts;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.ProjectTasks;
using App.Services.TaskAlerts;
using App.Web.Areas.Admin.Models.TaskAlerts;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertConfiguration;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReason;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReport;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories;

public partial class TaskAlertModelFactory : ITaskAlertModelFactory
{
    #region Fields

    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IEmployeeService _employeeService;
    protected readonly ILocalizationService _localizationService;
    protected readonly IProjectTaskService _projectTaskService;
	protected readonly ITaskAlertService _taskAlertService;

	#endregion

	#region Ctor

	public TaskAlertModelFactory(IDateTimeHelper dateTimeHelper,
        IEmployeeService employeeService,
        ILocalizationService localizationService,
        IProjectTaskService projectTaskService,
        ITaskAlertService taskAlertService)
	{
        _dateTimeHelper = dateTimeHelper;
        _employeeService = employeeService;
        _localizationService = localizationService;
        _projectTaskService = projectTaskService;
		_taskAlertService = taskAlertService;
	}

    #endregion

    protected async Task<IList<SelectListItem>> AddCommonOptionsAsync(IList<SelectListItem> items, bool includeAll = false, bool includeSelect = false)
    {
        if (includeSelect)
        {
            items.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = "0"
            });
        }

        if (includeAll)
        {
            items.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0"
            });
        }

        return items;
    }

    protected virtual async Task<IList<SelectListItem>> PrepareAvailableTaskAlertTypesAsync(bool includeAll = false, bool includeSelect = false)
    {
        var availableTaskAlertTypes = await TaskAlertsEnum.TaskOverdue.ToSelectListAsync();
        var taskAlertTypes = availableTaskAlertTypes.Select(availableTaskAlertType => new SelectListItem
        {
            Text = availableTaskAlertType.Text,
            Value = availableTaskAlertType.Value
        }).ToList();

        return await AddCommonOptionsAsync(taskAlertTypes, includeAll, includeSelect);
    }

    protected virtual async Task PrepareEmployeeListAsync(TaskAlertReportSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        var employeeName = "";
        var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);

        foreach (var p in employees)
        {
            searchModel.AvailableEmployees.Add(new SelectListItem
            {
                Text = p.FirstName + " " + p.LastName,
                Value = p.Id.ToString()
            });
        }
    }

    #region Task Alert Configuration Methods

    public virtual async Task<TaskAlertConfigurationSearchModel> PrepareTaskAlertConfigurationSearchModelAsync(TaskAlertConfigurationSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.AvailableTaskAlertTypes = await PrepareAvailableTaskAlertTypesAsync(true, false);

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<TaskAlertConfigurationListModel> PrepareTaskAlertConfigurationListModelAsync(TaskAlertConfigurationSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var taskAlertConfigurations = await _taskAlertService.GetAllTaskAlertConfigurationsAsync(taskAlertTypeId: searchModel.SearchTaskAlertTypeId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new TaskAlertConfigurationListModel().PrepareToGridAsync(searchModel, taskAlertConfigurations, () =>
        {
            return taskAlertConfigurations.SelectAwait(async taskAlertConfiguration =>
            {
                var taskAlertConfigurationModel = new TaskAlertConfigurationModel
                {
                    Id = taskAlertConfiguration.Id,
                    TaskAlertType = Enum.GetName(typeof(TaskAlertsEnum), taskAlertConfiguration.TaskAlertTypeId),
                    Percentage = taskAlertConfiguration.Percentage,
                    EnableComment = taskAlertConfiguration.EnableComment,
                    EnableReason = taskAlertConfiguration.EnableReason,
                    EnableCoordinatorMail = taskAlertConfiguration.EnableCoordinatorMail,
                    EnableLeaderMail = taskAlertConfiguration.EnableLeaderMail,
                    EnableManagerMail = taskAlertConfiguration.EnableManagerMail,
                    EnableDeveloperMail = taskAlertConfiguration.EnableDeveloperMail,
                    IsActive = taskAlertConfiguration.IsActive,
                    DisplayOrder = taskAlertConfiguration.DisplayOrder
                };

                return taskAlertConfigurationModel;
            });
        });

        return model;
    }

    public virtual async Task<TaskAlertConfigurationModel> PrepareTaskAlertConfigurationModelAsync(TaskAlertConfigurationModel model,
        TaskAlertConfiguration taskAlertConfiguration)
    {
        if (taskAlertConfiguration != null)
        {
            model = model ?? new TaskAlertConfigurationModel();

            model.Id = taskAlertConfiguration.Id;
            model.TaskAlertTypeId = taskAlertConfiguration.TaskAlertTypeId;
            model.Message = taskAlertConfiguration.Message;
            model.Percentage = taskAlertConfiguration.Percentage;
            model.EnableComment = taskAlertConfiguration.EnableComment;
            model.CommentRequired = taskAlertConfiguration.CommentRequired;
            model.EnableReason = taskAlertConfiguration.EnableReason;
            model.ReasonRequired = taskAlertConfiguration.ReasonRequired;
            model.EnableCoordinatorMail = taskAlertConfiguration.EnableCoordinatorMail;
            model.EnableLeaderMail = taskAlertConfiguration.EnableLeaderMail;
            model.EnableManagerMail = taskAlertConfiguration.EnableManagerMail;
            model.EnableDeveloperMail = taskAlertConfiguration.EnableDeveloperMail;
            model.NewETA = taskAlertConfiguration.NewETA;
            model.EnableOnTrack = taskAlertConfiguration.EnableOnTrack;
            model.IsActive = taskAlertConfiguration.IsActive;
            model.DisplayOrder = taskAlertConfiguration.DisplayOrder;
        }

        model.AvailableTaskAlertTypes = await PrepareAvailableTaskAlertTypesAsync(false, false);

        return model;
    }

    #endregion

    #region Task Alert Reasons Methods

    public virtual async Task<TaskAlertReasonSearchModel> PrepareTaskAlertReasonSearchModelAsync(TaskAlertReasonSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<TaskAlertReasonListModel> PrepareTaskAlertReasonListModelAsync(TaskAlertReasonSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var taskAlertReasons = await _taskAlertService.GetAllTaskAlertReasonsAsync(name: searchModel.SearchName, pageIndex: searchModel.Page - 1,
            pageSize: searchModel.PageSize);

        var model = await new TaskAlertReasonListModel().PrepareToGridAsync(searchModel, taskAlertReasons, () =>
        {
            return taskAlertReasons.SelectAwait(async taskAlertReason =>
            {
                var taskAlertReasonModel = new TaskAlertReasonModel
                {
                    Id = taskAlertReason.Id,
                    Name = taskAlertReason.Name,
                    IsActive = taskAlertReason.IsActive,
                    DisplayOrder = taskAlertReason.DisplayOrder
                };

                return taskAlertReasonModel;
            });
        });

        return model;
    }

    public virtual async Task<TaskAlertReasonModel> PrepareTaskAlertReasonModelAsync(TaskAlertReasonModel model, TaskAlertReason taskAlertReason)
    {
        if (taskAlertReason != null)
        {
            model = model ?? new TaskAlertReasonModel();

            model.Id = taskAlertReason.Id;
            model.Name = taskAlertReason.Name;
            model.IsActive = taskAlertReason.IsActive;
            model.DisplayOrder = taskAlertReason.DisplayOrder;
        }

        return model;
    }

    #endregion

    #region Task Alert Report Methods

    public virtual async Task<TaskAlertReportSearchModel> PrepareTaskAlertReportSearchModelAsync(TaskAlertReportSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        await PrepareEmployeeListAsync(searchModel);
        searchModel.AvailableTaskAlertTypes = await PrepareAvailableTaskAlertTypesAsync(true, false);

        searchModel.SetGridPageSize();

        return searchModel;
    }

    public virtual async Task<TaskAlertReportListModel> PrepareTaskAlertReportListModelAsync(TaskAlertReportSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(nameof(searchModel));

        var startDateValue = !searchModel.SearchFromDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchFromDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
        var endDateValue = !searchModel.SearchToDate.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.SearchToDate.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
        var employeeIds = (searchModel.SelectedEmployeeIds?.Contains(0) ?? true) ? null : searchModel.SelectedEmployeeIds.ToList();

        var taskAlertReports = await _taskAlertService.GetAllTaskAlertLogsAsync(createdFromUtc: startDateValue, createdToUtc: endDateValue,
            employeeIds: employeeIds, taskAlertConfigurationId: searchModel.SearchTaskAlertConfigurationId, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var model = await new TaskAlertReportListModel().PrepareToGridAsync(searchModel, taskAlertReports, () =>
        {
            return taskAlertReports.SelectAwait(async taskAlertReport =>
            {
                var taskAlertReportModel = new TaskAlertReportModel
                {
                    Id = taskAlertReport.Id,
                    Comment = taskAlertReport.Comment,
                    ETAHours = taskAlertReport.ETAHours,
                    DeliverOnTime = taskAlertReport.OnTrack,
                    CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(taskAlertReport.CreatedOnUtc, DateTimeKind.Utc)
                };

                var existingEmployee = await _employeeService.GetEmployeeByIdAsync(taskAlertReport.EmployeeId);
                if (existingEmployee != null)
                    taskAlertReportModel.EmployeeName = existingEmployee.FirstName + " " + existingEmployee.LastName;

                var existingProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskAlertReport.TaskId);
                if (existingProjectTask != null)
                    taskAlertReportModel.TaskName = existingProjectTask.TaskTitle;

                if (taskAlertReport.ReasonId > 0)
                    taskAlertReportModel.Reason = (await _taskAlertService.GetTaskAlertReasonByIdAsync(taskAlertReport.ReasonId)).Name;

                if (taskAlertReport.AlertId > 0)
                {
                    var alertConfiguration = await _taskAlertService.GetTaskAlertConfigurationByIdAsync(taskAlertReport.AlertId);
                    if (alertConfiguration != null)
                        taskAlertReportModel.AlertPercentage = alertConfiguration.Percentage;
                }    

                return taskAlertReportModel;
            });
        });

        return model;
    }

    public virtual async Task<TaskAlertReportModel> PrepareTaskAlertReportModelAsync(TaskAlertReportModel model, TaskAlertLog taskAlertLog)
    {
        if (taskAlertLog != null)
        {
            model = model ?? new TaskAlertReportModel();

            model.Id = taskAlertLog.Id;
            model.Comment = taskAlertLog.Comment;
            model.ETAHours = taskAlertLog.ETAHours;
            model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(taskAlertLog.CreatedOnUtc, DateTimeKind.Utc);

            if (taskAlertLog.AlertId > 0)
            {
                var alertConfiguration = await _taskAlertService.GetTaskAlertConfigurationByIdAsync(taskAlertLog.AlertId);
                if (alertConfiguration != null)
                    model.AlertPercentage = alertConfiguration.Percentage;
            }

            var existingEmployee = await _employeeService.GetEmployeeByIdAsync(taskAlertLog.EmployeeId);
            if (existingEmployee != null)
                model.EmployeeName = existingEmployee.FirstName + " " + existingEmployee.LastName;

            var existingProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskAlertLog.TaskId);
            if (existingProjectTask != null)
                model.TaskName = existingProjectTask.TaskTitle;

            if (taskAlertLog.ReasonId > 0)
                model.Reason = (await _taskAlertService.GetTaskAlertReasonByIdAsync(taskAlertLog.ReasonId)).Name;
        }
        return model;
    }

    #endregion
}
