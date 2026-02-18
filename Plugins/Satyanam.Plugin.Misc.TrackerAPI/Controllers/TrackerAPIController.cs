using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.ActivityEvents;
using App.Core.Domain.Customers;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TaskAlerts;
using App.Core.Domain.TimeSheets;
using App.Data.Extensions;
using App.Services;
using App.Services.Authentication;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.TaskAlerts;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Models.TaskAlerts;
using App.Web.Framework.Controllers;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Models;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.TrackerAPI.Domain;
using Satyanam.Plugin.Misc.TrackerAPI.DTO;
using Satyanam.Plugin.Misc.TrackerAPI.Models;
using Satyanam.Plugin.Misc.TrackerAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.TrackerAPI.Controllers;

public partial class TrackerAPIController : BaseController
{
    #region Fields

    protected readonly IAuthenticationService _authenticationService;
    protected readonly ICheckListMappingService _checkListMappingService;
    protected readonly ICheckListMasterService _checkListMasterService;
    protected readonly ICustomerRegistrationService _customerRegistrationService;
    protected readonly ICustomerService _customerService;
    protected readonly IDateTimeHelper _dateTimeHelper;
    protected readonly IEmployeeService _employeeService;
    protected readonly IFollowUpTaskService _followUpTaskService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly IProjectsService _projectsService;
    protected readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
    protected readonly IStoreContext _storeContext;
    protected readonly ITaskCategoryService _taskCategoryService;
    protected readonly ITaskAlertService _taskAlertService;
    protected readonly ITaskChangeLogService _taskChangeLogService;
    protected readonly ITaskCheckListEntryService _taskCheckListEntryService;
    protected readonly ITimeSheetsService _timeSheetsService;
    protected readonly ITrackerAPIService _trackerAPIService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly CustomerSettings _customerSettings;
    protected readonly ProjectTaskSetting _projectTaskSetting;
    protected readonly TrackerAPISettings _trackerAPISettings;

    #endregion

    #region Ctor

    public TrackerAPIController(IAuthenticationService authenticationService,
        ICheckListMappingService checkListMappingService,
        ICheckListMasterService checkListMasterService,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDateTimeHelper dateTimeHelper,
        IEmployeeService employeeService,
        IFollowUpTaskService followUpTaskService,
        ILocalizationService localizationService,
        ILogger logger,
        IProjectsService projectsService,
        IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
        IStoreContext storeContext,
        ITaskCategoryService taskCategoryService,
        ITaskAlertService taskAlertService,
        ITaskChangeLogService taskChangeLogService,
        ITaskCheckListEntryService taskCheckListEntryService,
        ITimeSheetsService timeSheetsService,
        ITrackerAPIService trackerAPIService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        CustomerSettings customerSettings,
        ProjectTaskSetting projectTaskSetting,
        TrackerAPISettings trackerAPISettings)
    {
        _authenticationService = authenticationService;
        _checkListMappingService = checkListMappingService;
        _checkListMasterService = checkListMasterService;
        _customerRegistrationService = customerRegistrationService;
        _customerService = customerService;
        _dateTimeHelper = dateTimeHelper;
        _employeeService = employeeService;
        _followUpTaskService = followUpTaskService;
        _localizationService = localizationService;
        _logger = logger;
        _projectsService = projectsService;
        _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
        _storeContext = storeContext;
        _taskCategoryService = taskCategoryService;
        _taskAlertService = taskAlertService;
        _taskChangeLogService = taskChangeLogService;
        _taskCheckListEntryService = taskCheckListEntryService;
        _timeSheetsService = timeSheetsService;
        _trackerAPIService = trackerAPIService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _customerSettings = customerSettings;
        _projectTaskSetting = projectTaskSetting;
        _trackerAPISettings = trackerAPISettings;
    }

    #endregion

    #region Utilities

    private async Task<TrackerAPIResponseModel> CheckIfAuthenticated(TrackerAPIResponseModel trackerAPIResponseModel, bool isLoginAPI = false)
    {
        bool ValidateApiKeyAndSecret(string apiKey, string apiSecret)
        {
            return apiKey != null && apiSecret != null && apiKey == _trackerAPISettings.APIKey && apiSecret == _trackerAPISettings.APISecretKey;
        }

        string unauthorizedMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.UnauthorizedAccess");

        if (isLoginAPI)
        {
            if (Request.Headers.TryGetValue(TrackerAPIDefaults.APIKeyHeader, out var apiKeyValues) &&
                Request.Headers.TryGetValue(TrackerAPIDefaults.APISecretKeyHeader, out var apiSecretValues))
            {
                var apiKey = apiKeyValues.FirstOrDefault();
                var apiSecret = apiSecretValues.FirstOrDefault();

                trackerAPIResponseModel.Success = ValidateApiKeyAndSecret(apiKey, apiSecret);
                trackerAPIResponseModel.ResponseMessage = trackerAPIResponseModel.Success ? null : unauthorizedMessage;

                return trackerAPIResponseModel;
            }
        }
        else
        {
            if (Request.Headers.TryGetValue(TrackerAPIDefaults.APIKeyHeader, out var apiKeyValues) &&
                Request.Headers.TryGetValue(TrackerAPIDefaults.APISecretKeyHeader, out var apiSecretValues) &&
                Request.Headers.TryGetValue(TrackerAPIDefaults.EmployeeIdHeader, out var employeeIdValues))
            {
                var apiKey = apiKeyValues.FirstOrDefault();
                var apiSecret = apiSecretValues.FirstOrDefault();
                int employeeId = Convert.ToInt32(employeeIdValues.FirstOrDefault());

                if (employeeId == 0)
                {
                    trackerAPIResponseModel.Success = false;
                    trackerAPIResponseModel.EmployeeId = employeeId;
                    trackerAPIResponseModel.ResponseMessage = unauthorizedMessage;
                    return trackerAPIResponseModel;
                }

                var employee = await _trackerAPIService.GetEmployeeByIdAsync(employeeId);
                if (employee == null)
                {
                    trackerAPIResponseModel.Success = false;
                    trackerAPIResponseModel.EmployeeId = employeeId;
                    trackerAPIResponseModel.ResponseMessage = unauthorizedMessage;
                    return trackerAPIResponseModel;
                }

                trackerAPIResponseModel.Success = ValidateApiKeyAndSecret(apiKey, apiSecret);
                trackerAPIResponseModel.EmployeeId = employeeId;
                trackerAPIResponseModel.ResponseMessage = trackerAPIResponseModel.Success ? null : unauthorizedMessage;

                return trackerAPIResponseModel;
            }
        }

        trackerAPIResponseModel.Success = false;
        trackerAPIResponseModel.ResponseMessage = unauthorizedMessage;
        return trackerAPIResponseModel;
    }

    private async Task<JsonResult> HandleTrackerAPIFailureAsync(TrackerAPIResponseModel trackerAPIResponseModel, TrackerAPILog trackerAPILog)
    {
        await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);
        var message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.UnauthorizedAccess");
        return Json(new
        {
            result = false,
            message
        });
    }

    private async Task<JsonResult> HandleTrackerAPIErrorAsync(TrackerAPIResponseModel trackerAPIResponseModel, TrackerAPILog trackerAPILog,
        string message)
    {
        trackerAPIResponseModel.Success = false;
        trackerAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync(message);

        await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

        return Json(new
        {
            result = false,
            message = trackerAPIResponseModel.ResponseMessage
        });
    }

    private async Task<JsonResult> HandleTrackerAPIExceptionAsync(Exception exception)
    {
        await _logger.ErrorAsync(TrackerAPIDefaults.TrackerLog + exception.Message, exception);

        return Json(new
        {
            result = false,
            message = exception.Message
        });
    }

    private async Task LogTrackerAPICallAsync<TModel>(TModel model, TrackerAPILog trackerAPILog)
    {
        ArgumentNullException.ThrowIfNull(nameof(model));

        if (_trackerAPISettings.EnableLogging)
        {
            trackerAPILog.Success = (bool)typeof(TModel).GetProperty("Success")?.GetValue(model, null);
            trackerAPILog.ResponseMessage = typeof(TModel).GetProperty("ResponseMessage")?.GetValue(model, null)?.ToString();
            trackerAPILog.ResponseJson = JsonConvert.SerializeObject(model);
            trackerAPILog.CreatedOnUtc = DateTime.UtcNow;
            trackerAPILog.EmployeeId = (int)typeof(TModel).GetProperty("EmployeeId")?.GetValue(model, null);

            await _trackerAPIService.InsertTrackerAPILogAsync(trackerAPILog);
        }
    }

    private string FormatTaskTitle(int taskTypeId, string title)
    {
        return taskTypeId switch
        {
            (int)TaskTypeEnum.UserStory => $"UserStory: {title}",
            (int)TaskTypeEnum.Bug => $"Bug: {title}",
            (int)TaskTypeEnum.ChangeRequest => $"CR: {title}",
            _ => title
        };
    }

    private string FormatEnumValue(string enumValue)
    {
        if (enumValue.Contains("_"))
        {
            var valueWithoutUnderscores = enumValue.Replace("_", string.Empty);
            var result = Regex.Replace(valueWithoutUnderscores, "(?<=\\w) (?=[A-Z])", string.Empty);
            return result;
        }
        else
            return Regex.Replace(enumValue, "(?<!^)(?=[A-Z])", " ");
    }

    private async Task<List<EmployeeDetailsRootObject>> GetEmployeesAsync()
    {
        var employees = await _trackerAPIService.GetAllEmployeesAsync();
        return employees.Select(emp => new EmployeeDetailsRootObject
        {
            Id = emp.Id,
            FullName = $"{emp.FirstName} {emp.LastName}"
        }).ToList();
    }

    private async Task InsertActivityTrackingAsync(int statusId, int employeeId)
    {
        var existingActivityTracking = await _trackerAPIService.GetActivityTrackingByEmployeeIdAsync(employeeId);
        if (existingActivityTracking == null)
        {
            var activityTracking = new ActivityTracking()
            {
                EmployeeId = employeeId,
                ActiveDuration = 0,
                AwayDuration = 0,
                OfflineDuration = 0,
                StoppedDuration = 0,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow
            };
            var jsonActivityTracking = new EmployeeJsonActivityTrackingEventParametersModel()
            {
                StatusId = statusId,
                Duration = _trackerAPISettings.TrackingDuration,
                CreatedOnUtc = DateTime.UtcNow
            };
            activityTracking.JsonString = JsonConvert.SerializeObject(new List<object> { jsonActivityTracking });
            if (statusId == (int)ActivityTrackingEnum.Active)
                activityTracking.ActiveDuration = _trackerAPISettings.TrackingDuration;
            else if (statusId == (int)ActivityTrackingEnum.Away)
                activityTracking.AwayDuration = _trackerAPISettings.TrackingDuration;
            activityTracking.TotalDuration += _trackerAPISettings.TrackingDuration;
            await _trackerAPIService.InsertActivityTrackingAsync(activityTracking);
        }
        else
        {
            var existingActivityTrackings = JsonConvert.DeserializeObject<List<EmployeeJsonActivityTrackingEventParametersModel>>
                (existingActivityTracking.JsonString) ?? new List<EmployeeJsonActivityTrackingEventParametersModel>();
            var lastNodeOfActivityTracking = existingActivityTrackings?.LastOrDefault();
            if (lastNodeOfActivityTracking != null && lastNodeOfActivityTracking.StatusId == statusId)
            {
                lastNodeOfActivityTracking.Duration += _trackerAPISettings.TrackingDuration;
                lastNodeOfActivityTracking.CreatedOnUtc = DateTime.UtcNow;
            }
            else
            {
                existingActivityTrackings.Add(new EmployeeJsonActivityTrackingEventParametersModel
                {
                    StatusId = statusId,
                    Duration = _trackerAPISettings.TrackingDuration,
                    CreatedOnUtc = DateTime.UtcNow
                });
            }

            if (statusId == (int)ActivityTrackingEnum.Active)
                existingActivityTracking.ActiveDuration += _trackerAPISettings.TrackingDuration;
            else if (statusId == (int)ActivityTrackingEnum.Away)
                existingActivityTracking.AwayDuration += _trackerAPISettings.TrackingDuration;
            else if (statusId == (int)ActivityTrackingEnum.Offline)
                existingActivityTracking.OfflineDuration += _trackerAPISettings.TrackingDuration;
            existingActivityTracking.TotalDuration += _trackerAPISettings.TrackingDuration;

            existingActivityTracking.JsonString = JsonConvert.SerializeObject(existingActivityTrackings);
            existingActivityTracking.EndTime = DateTime.UtcNow;
            await _trackerAPIService.UpdateActivityTrackingAsync(existingActivityTracking);
        }
    }

    private async Task UpdateTaskTimeAndStatusAsync(int taskId = 0, int spentHours = 0, int spentMinutes = 0)
    {
        var existingTask = await _trackerAPIService.GetProjectTaskByIdAsync(taskId);
        if (existingTask == null)
            return;

        var updatedTime = await _timeSheetsService.AddSpentTimeAsync(existingTask.SpentHours, existingTask.SpentMinutes, spentHours, spentMinutes);
        existingTask.SpentHours = updatedTime.SpentHours;
        existingTask.SpentMinutes = updatedTime.SpentMinutes;
        existingTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(existingTask);
        await _trackerAPIService.UpdateProjectTaskAsync(existingTask);
    }

    private async Task InsertTimeSheetEntryAsync(int activityId = 0, int employeeId = 0, int projectId = 0, int taskId = 0, bool billable = false,
        DateTime? spentDate = null, int spentHours = 0, int spentMinutes = 0, bool manualTimeEntry = false)
    {
        var timesheet = new TimeSheet
        {
            ActivityId = activityId,
            EmployeeId = employeeId,
            ProjectId = projectId,
            TaskId = taskId,
            Billable = billable,
            SpentDate = manualTimeEntry && spentDate.HasValue ? spentDate.Value : DateTime.UtcNow.Date,
            SpentHours = spentHours,
            SpentMinutes = spentMinutes,
            IsManualEntry = manualTimeEntry,
            CreateOnUtc = DateTime.UtcNow,
            UpdateOnUtc = DateTime.UtcNow
        };

        if (!manualTimeEntry)
            timesheet.StartTime = DateTime.UtcNow;

        await _trackerAPIService.InsertTimeSheetAsync(timesheet);
    }

    private async Task<int> InsertOrUpdateActivityAsync(int employeeId = 0, int taskId = 0, string activityDescription = null, int spentHours = 0,
        int spentMinutes = 0, bool manualTimeEntry = false)
    {
        if (string.IsNullOrWhiteSpace(activityDescription))
            return 0;

        var existingActivity = await _trackerAPIService.GetActivityAsync(taskId, activityDescription);
        if (existingActivity != null)
        {
            if (manualTimeEntry)
            {
                var updatedTime = await _timeSheetsService.AddSpentTimeAsync(existingActivity.SpentHours, existingActivity.SpentMinutes,
                    spentHours, spentMinutes);
                existingActivity.SpentHours = updatedTime.SpentHours;
                existingActivity.SpentMinutes = updatedTime.SpentMinutes;
            }

            existingActivity.UpdateOnUtc = DateTime.UtcNow;

            await _trackerAPIService.UpdateActivityAsync(existingActivity);
            return existingActivity.Id;
        }

        var newActivity = new Activity
        {
            EmployeeId = employeeId,
            TaskId = taskId,
            ActivityName = activityDescription,
            CreateOnUtc = DateTime.UtcNow,
            UpdateOnUtc = DateTime.UtcNow
        };

        await _trackerAPIService.InsertActivityAsync(newActivity);

        if (manualTimeEntry)
        {
            var calculatedTime = await _timeSheetsService.AddSpentTimeAsync(newActivity.SpentHours, newActivity.SpentMinutes, spentHours, spentMinutes);
            newActivity.SpentHours = calculatedTime.SpentHours;
            newActivity.SpentMinutes = calculatedTime.SpentMinutes;
        }

        newActivity.UpdateOnUtc = DateTime.UtcNow;
        await _trackerAPIService.UpdateActivityAsync(newActivity);

        return newActivity.Id;
    }

    private async Task UpdateActivityTimeAsync(int activityId = 0, int spentHours = 0, int spentMinutes = 0)
    {
        var existingActivity = await _trackerAPIService.GetActivityByIdAsync(activityId);
        if (existingActivity == null)
            return;

        var calculateActivityTime = await _timeSheetsService.AddSpentTimeAsync(existingActivity.SpentHours,
            existingActivity.SpentMinutes, spentHours, spentHours);
        existingActivity.SpentHours = calculateActivityTime.SpentHours;
        existingActivity.SpentMinutes = calculateActivityTime.SpentMinutes;
        existingActivity.UpdateOnUtc = DateTime.UtcNow;
        await _trackerAPIService.UpdateActivityAsync(existingActivity);
    }

    private async Task InsertOrUpdateAttendanceAsync(int employeeId = 0, int spentHours = 0, int spentMinutes = 0)
    {
        var existingAttendance = await _trackerAPIService.GetTodayEmployeeAttendanceAsync(employeeId);
        if (existingAttendance == null)
        {
            var newAttendance = new EmployeeAttendance
            {
                EmployeeId = employeeId,
                CheckIn = DateTime.UtcNow.Date,
                CheckOut = DateTime.UtcNow.Date,
                SpentHours = spentHours,
                SpentMinutes = spentMinutes,
                StatusId = (int)App.Core.Domain.EmployeeAttendances.StatusEnum.Present,
                CreateOnUtc = DateTime.UtcNow,
                UpdateOnUtc = DateTime.UtcNow
            };

            await _trackerAPIService.InsertEmployeeAttendanceAsync(newAttendance);
            return;
        }

        var updatedTime = await _timeSheetsService.AddSpentTimeAsync(existingAttendance.SpentHours, existingAttendance.SpentMinutes, spentHours,
            spentMinutes);
        existingAttendance.SpentHours = updatedTime.SpentHours;
        existingAttendance.SpentMinutes = updatedTime.SpentMinutes;
        existingAttendance.UpdateOnUtc = DateTime.UtcNow;

        await _trackerAPIService.UpdateEmployeeAttendanceAsync(existingAttendance);
    }

    private async Task<int> GetEmployeedIdBasedOnStatusAsync(string status, ProjectTask task, Project project)
    {
        int employeeId = 0;
        switch (status?.ToLowerInvariant())
        {
            case "code review":
                employeeId = await _projectsService.GetProjectCoordinatorIdByIdAsync(project.Id);
                if (employeeId == 0)
                    employeeId = await _projectsService.GetProjectLeaderIdByIdAsync(project.Id);
                if (employeeId == 0)
                    employeeId = await _projectsService.GetProjectManagerIdByIdAsync(project.Id);
                break;

            case "ready to test":
            case "qa on live":
                employeeId = await _projectsService.GetProjectQAIdByIdAsync(project.Id);
                break;

            case "active":
            case "code review done":
            case "test failed":
            case "ready for live":
            case "closed":
                employeeId = task.DeveloperId;
                break;
        }

        return employeeId;
    }

    #endregion

    #region Setting API Method

    [HttpGet]
    [Route("/api/settings", Name = nameof(Settings))]
    public virtual async Task<IActionResult> Settings()
    {
        try
        {
            var hasMissingKeys = string.IsNullOrWhiteSpace(_trackerAPISettings.APIKey) || string.IsNullOrWhiteSpace(_trackerAPISettings.APISecretKey);
            if (hasMissingKeys)
                return Json(new { result = false, message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.APIKeySecretKey") });

            return Json(new
            {
                result = true,
                keyboard_click = _trackerAPISettings.EnableKeyboardClick,
                mouse_click = _trackerAPISettings.EnableMouseClick,
                capture_screenshot = _trackerAPISettings.EnableScreenShot,
                tracking_duration = _trackerAPISettings.TrackingDuration,
                alert_duration = _trackerAPISettings.AlertDuration,
                alert_minimum_duration = _trackerAPISettings.AlertMinimumDuration,
                switch_task_duration = _trackerAPISettings.SwitchTaskDuration,
                client_id = _trackerAPISettings.ClientId,
                client_secret = _trackerAPISettings.ClientSecret,
                tenant_id = _trackerAPISettings.TenantId,
                user_id = _trackerAPISettings.UserId
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }


    #endregion

    #region Login API Method

    [HttpPost]
    [Route("/api/login", Name = nameof(EmployeeLogin))]
    public virtual async Task<IActionResult> EmployeeLogin([FromBody] EmployeeLoginParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.EmployeeLoginEndPoint
            };

            await CheckIfAuthenticated(parameters, true);
            if (!parameters.Success)
                return await HandleTrackerAPIFailureAsync(parameters, trackerAPILog);

            if (string.IsNullOrWhiteSpace(parameters.Username))
                return await HandleTrackerAPIErrorAsync(parameters, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.Username");

            if (string.IsNullOrWhiteSpace(parameters.Password))
                return await HandleTrackerAPIErrorAsync(parameters, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.Password");

            var loginResult = await _customerRegistrationService.ValidateCustomerAsync(parameters.Username.Trim(), parameters.Password.Trim());
            if (loginResult == CustomerLoginResults.Successful)
            {
                var customer = _customerSettings.UsernamesEnabled
                    ? await _customerService.GetCustomerByUsernameAsync(parameters.Username.Trim())
                    : await _customerService.GetCustomerByEmailAsync(parameters.Username.Trim());

                var employee = await _trackerAPIService.GetEmployeeByCustomerIdAsync(customer.Id);
                int employeeId = employee?.Id ?? 0;

                parameters.Success = true;
                parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.LoginSuccess");
                await LogTrackerAPICallAsync(parameters, trackerAPILog);

                return Json(new
                {
                    result = true,
                    message = parameters.ResponseMessage,
                    employee_id = employeeId,
                    employee_email = parameters.Username
                });
            }

            parameters.ResponseMessage = loginResult switch
            {
                CustomerLoginResults.CustomerNotExist => await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.CustomerNotExist"),
                CustomerLoginResults.Deleted => await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.Deleted"),
                CustomerLoginResults.NotActive => await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.NotActive"),
                CustomerLoginResults.NotRegistered => await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.NotRegistered"),
                CustomerLoginResults.LockedOut => await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.LockedOut"),
                _ => await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.WrongCredentials")
            };

            return await HandleTrackerAPIErrorAsync(parameters, trackerAPILog, parameters.ResponseMessage);
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Employee Details and Employee Profile API Method

    [HttpGet]
    [Route("/api/employee", Name = nameof(GetEmployee))]
    public virtual async Task<IActionResult> GetEmployee([FromQuery] int? employeeId = null)
    {
        try
        {
            string requestPath = employeeId.HasValue ? $"/api/employee?employeeId={employeeId}" : "/api/employee";
            string requestEndPoint = employeeId.HasValue ? TrackerAPIDefaults.GetEmployeeProfileEndPoint : TrackerAPIDefaults.GetEmployeeDetailsEndPoint;
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = requestPath,
                EndPoint = TrackerAPIDefaults.GetEmployeeDetailsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (employeeId.HasValue)
            {
                if (employeeId <= 0)
                    return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

                var employee = await _trackerAPIService.GetEmployeeByIdAsync(employeeId.Value);
                if (employee == null)
                    return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoEmployeeDetailsFound");

                return Json(new
                {
                    result = true,
                    first_name = employee.FirstName,
                    last_name = employee.LastName,
                    personal_email = employee.PersonalEmail,
                    gender = employee.Gender,
                    mobile_number = employee.MobileNo,
                    official_email = employee.OfficialEmail
                });
            }

            return Json(new
            {
                result = true,
                employee_details = await GetEmployeesAsync()
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #region Calculate Day Time API Method

    [HttpGet]
    [Route("/api/calculate_day_time/{employeeId}", Name = nameof(CalculateDayTime))]
    public virtual async Task<IActionResult> CalculateDayTime([FromRoute] int employeeId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = $"/api/calculate_day_time/{employeeId}",
                EndPoint = TrackerAPIDefaults.GetDayTimeEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (employeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            var timeSheets = await _trackerAPIService.GetTodayTimeSheetByEmployeeIdAsync(employeeId) ?? new List<TimeSheet>();

            var totalMinutes = timeSheets.Sum(ts => ts.SpentHours * 60 + ts.SpentMinutes);

            int spentHours = totalMinutes / 60;
            int spentMinutes = totalMinutes % 60;

            trackerAPIResponseModel.Success = true;
            trackerAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.DayTimeFetchedSuccessfully");

            await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

            return Json(new
            {
                result = true,
                spent_hours = spentHours,
                spent_minutes = spentMinutes
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Projects API Method

    [HttpGet]
    [Route("/api/projects/{employeeId}", Name = nameof(GetProjectsByEmployeeId))]
    public virtual async Task<IActionResult> GetProjectsByEmployeeId([FromRoute] int employeeId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = $"/api/projects/{employeeId}",
                EndPoint = TrackerAPIDefaults.GetProjectsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (employeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            var projects = await _trackerAPIService.GetProjectsByEmployeeIdAsync(employeeId);
            if (!projects.Any())
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoProjectsFound");

            var model = projects.Where(p => p != null).Select(p => new ProjectsRootObject
            {
                Id = p.Id,
                ProjectName = p.ProjectTitle
            }).ToList();

            trackerAPIResponseModel.Success = true;
            trackerAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

            return Json(new
            {
                result = true,
                projects = model
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Tasks Methods

    #region Create Task API Method

    [HttpPost]
    [Route("/api/task/create", Name = nameof(CreateTask))]
    public virtual async Task<IActionResult> CreateTask([FromBody] EmployeeTaskParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.CreateTaskEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.EmployeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            if (parameters.ProjectId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.ProjectId");

            if (string.IsNullOrWhiteSpace(parameters.TaskTitle))
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskTitle");

            var existingTask = await _trackerAPIService.GetTaskTitleByProjectIdAsync(parameters.ProjectId, parameters.TaskTitle.Trim());
            if (existingTask != null)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskExistsForAGivenTitle");

            var (hours, minutes) = await _timeSheetsService.ConvertSpentTimeAsync(parameters.EstimationTime);
            var estimatedTime = await _timeSheetsService.ConvertToTotalHours(hours, minutes);

            int statusId = 0;
            var workflowStatus = await _trackerAPIService.GetWorkflowStatusByProcessWorkflowIdAsync(parameters.ProcessWorkflowId);
            if (workflowStatus != null)
                statusId = workflowStatus.Id;

            var projectTask = new ProjectTask
            {
                AssignedTo = parameters.AssignedTo,
                DeveloperId = parameters.AssignedTo,
                ProjectId = parameters.ProjectId,
                ParentTaskId = parameters.ParentTaskId,
                TaskCategoryId = parameters.TaskCategoryId,
                TaskTitle = FormatTaskTitle(parameters.TaskTypeId, parameters.TaskTitle.Trim()),
                Description = parameters.TaskDescription?.Trim(),
                EstimatedTime = estimatedTime,
                StatusId = statusId,
                DeliveryOnTime = true,
                Tasktypeid = parameters.TaskTypeId,
                DueDate = parameters.DueDate,
                ProcessWorkflowId = parameters.ProcessWorkflowId,
                IsSync = parameters.IsSync,
                CreatedOnUtc = DateTime.UtcNow
            };

            if (parameters.TaskTypeId != (int)TaskTypeEnum.Bug)
                projectTask.WorkQuality = 100.00m;

            await _trackerAPIService.InsertProjectTaskAsync(projectTask);

            parameters.Success = true;
            parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.TaskCreatedSuccessfully");

            return Json(new
            {
                result = true,
                message = parameters.ResponseMessage
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #region Task API Method

    [HttpGet]
    [Route("/api/tasks/{projectId}", Name = nameof(GetTasksByProjectId))]
    public virtual async Task<IActionResult> GetTasksByProjectId([FromRoute] int projectId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = $"/api/tasks/{projectId}",
                EndPoint = TrackerAPIDefaults.GetTasksEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (projectId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.ProjectId");

            int assignedTo = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId) ? 0 : trackerAPIResponseModel.EmployeeId;
            var projectTasks = await _trackerAPIService.GetProjectTasksByProjectIdAsync(projectId, assignedTo);
            if (!projectTasks.Any())
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoTasksFound");

            var estimationTimes = await Task.WhenAll(projectTasks.Select(projectTask => _trackerAPIService.ConvertToHHMMFormat(projectTask.EstimatedTime)));

            var model = projectTasks.Select((task, index) => new ProjectTasksRootObject
            {
                Id = task.Id,
                TaskTitle = task.TaskTitle,
                EstimationTime = Convert.ToDecimal(estimationTimes[index]),
                SpentTime = task.SpentHours + (decimal)task.SpentMinutes / 60
            }).ToList();

            trackerAPIResponseModel.Success = true;
            trackerAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

            return Json(new
            {
                result = true,
                tasks = model
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Activity Task API Method

    [HttpGet]
    [Route("/api/activity_tasks/{projectId}", Name = nameof(GetEmployeeActivityTasksByProjectId))]
    public virtual async Task<IActionResult> GetEmployeeActivityTasksByProjectId([FromRoute] int projectId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = $"/api/activity_tasks/{projectId}",
                EndPoint = TrackerAPIDefaults.GetActivityTasksEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (projectId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.ProjectId");

            bool isQa = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId);
            int assignedTo = isQa ? 0 : trackerAPIResponseModel.EmployeeId;
            var projectTasks = await _trackerAPIService.GetProjectTasksByProjectIdAsync(projectId, assignedTo);
            if (!projectTasks.Any())
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoTasksFound");

            var estimationTimes = await Task.WhenAll(projectTasks.Select(t => _trackerAPIService.ConvertToHHMMFormat(t.EstimatedTime)));

            var model = new EmployeeActivityProjectTasksRootObject
            {
                AvailableProjectTasks = projectTasks.Select((task, index) => new ProjectTasksRootObject
                {
                    Id = task.Id,
                    TaskTitle = task.TaskTitle,
                    EstimationTime = Convert.ToDecimal(estimationTimes[index]),
                    SpentTime = task.SpentHours + (decimal)task.SpentMinutes / 60
                }).ToList()
            };

            var lastTimeSheet = await _trackerAPIService.GetLastTimeSheetByEmployeeIdAsync(trackerAPIResponseModel.EmployeeId, projectId);
            if (lastTimeSheet != null)
            {
                var existingTask = await _trackerAPIService.GetProjectTaskByIdAsync(lastTimeSheet.TaskId);
                if (existingTask != null)
                {
                    var lastActivity = lastTimeSheet.ActivityId != 0 ? await _trackerAPIService.GetActivityByIdAsync(lastTimeSheet.ActivityId) : null;

                    var lastActivityTask = new EmployeeActivityProjectTaskRootObject
                    {
                        TaskId = existingTask.Id,
                        TaskName = existingTask.TaskTitle,
                        ActivityId = lastActivity?.Id ?? 0,
                        ActivityName = lastActivity?.ActivityName ?? string.Empty,
                        EstimationTime = Convert.ToDecimal(await _trackerAPIService.ConvertToHHMMFormat(existingTask.EstimatedTime)),
                        Billable = lastTimeSheet.Billable
                    };

                    var time = isQa ? await _timeSheetsService.GetQATimeByTaskId(existingTask.Id) : await _timeSheetsService.GetDevelopmentTimeByTaskId(existingTask.Id);
                    lastActivityTask.SpentHours = time.SpentHours;
                    lastActivityTask.SpentMinutes = time.SpentMinutes;

                    model.AvailableLastActivity = lastActivityTask;
                }
            }

            trackerAPIResponseModel.Success = true;
            trackerAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

            return Json(new
            {
                result = true,
                task_details = model
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Task Detail API Method

    [HttpGet]
    [Route("/api/task_details/{taskId}", Name = nameof(GetTaskDetailsByTaskId))]
    public virtual async Task<IActionResult> GetTaskDetailsByTaskId([FromRoute] int taskId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = $"/api/task_details/{taskId}",
                EndPoint = TrackerAPIDefaults.GetTaskDetailsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (taskId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskId");

            var projectTask = await _trackerAPIService.GetProjectTaskByIdAsync(taskId);

            var activities = await _trackerAPIService.GetActivitiesByTaskIdAsync(taskId);

            bool isQa = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId);
            var model = new ProjectTaskDetailsRootObject()
            {
                EstimationTime = Convert.ToDecimal(await _trackerAPIService.ConvertToHHMMFormat(projectTask.EstimatedTime))
            };

            var time = isQa ? await _timeSheetsService.GetQATimeByTaskId(taskId) : await _timeSheetsService.GetDevelopmentTimeByTaskId(taskId);
            model.SpentHours = time.SpentHours;
            model.SpentMinutes = time.SpentMinutes;

            model.AvailableTaskActivities = activities.Select(activity => new TaskActivitiesRootObject
            {
                Id = activity.Id,
                ActivityName = activity.ActivityName,
            }).ToList();

            trackerAPIResponseModel.Success = true;
            trackerAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

            return Json(new
            {
                result = true,
                task_details = model
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Search Task API Method

    [HttpGet]
    [Route("/api/search_tasks", Name = nameof(GetTaskDetails))]
    public virtual async Task<IActionResult> GetTaskDetails()
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = "/api/search_tasks",
                EndPoint = TrackerAPIDefaults.SearchTasksEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            var projects = await _trackerAPIService.GetProjectsByEmployeeIdAsync(trackerAPIResponseModel.EmployeeId);
            if (!projects.Any())
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoProjectsFound");

            var model = new EmployeeTaskDetailsRootObject();
            model.AvailableProjects = projects.Select(project => new ProjectsRootObject
            {
                Id = project.Id,
                ProjectName = project.ProjectTitle
            }).ToList();

            model.AvailableTaskTypes = (await TaskTypeEnum.Select.ToSelectListAsync()).Select(taskType => new TaskTypesRootObject
            {
                Text = FormatEnumValue(taskType.Text),
                Value = taskType.Value
            }).ToList();

            model.AvailableProcessWorkflows = (await _trackerAPIService.GetAllProcessWorkflowsAsync()).Select(processWorkflow => new ProcessWorkflowRootObject
            {
                Text = processWorkflow.Name,
                Value = processWorkflow.Id
            }).ToList();
            model.AvailableProcessWorkflows.Insert(0, new ProcessWorkflowRootObject
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = 0
            });

            model.AvailableStatuses.Insert(0, new TaskStatusesRootObject
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = "0",
                ColorCode = string.Empty
            });

            return Json(model);
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Search Task Details API Method

    [HttpPost]
    [Route("/api/search_task_details", Name = nameof(SearchTasks))]
    public virtual async Task<IActionResult> SearchTasks([FromBody] EmployeeTaskChangeLogsParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = "/api/search_task_details",
                EndPoint = TrackerAPIDefaults.SearchTaskDetailsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            var taskDetails = await _trackerAPIService.GetTaskLogDetailsByProjectIdAsync(parameters.ProjectId, parameters.TaskId,
                parameters.TaskName, parameters.TaskStatusId, parameters.TaskTypeId, parameters.ProcessWorkflowId, parameters.DueDate);
            if (!taskDetails.Any())
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoTasksFound");

            bool isQA = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId);

            var model = new List<TaskLogsRootObject>();

            foreach (var taskDetail in taskDetails)
            {
                var employeeDetails = await _trackerAPIService.GetEmployeeByIdAsync(taskDetail.AssignedTo);
                if (employeeDetails == null)
                    continue;

                var projectDetails = await _trackerAPIService.GetProjectByIdAsync(taskDetail.ProjectId);
                if (projectDetails == null)
                    continue;

                string estimationTime = await _trackerAPIService.ConvertToHHMMFormat(taskDetail.EstimatedTime);

                var taskLogsRootObject = new TaskLogsRootObject()
                {
                    TaskId = taskDetail.Id,
                    EmployeeName = employeeDetails.FirstName + " " + employeeDetails.LastName,
                    ProjectName = projectDetails.ProjectTitle,
                    TaskName = taskDetail.TaskTitle,
                    EstimationTime = estimationTime,
                    SpentHours = taskDetail.SpentHours,
                    SpentMinutes = taskDetail.SpentMinutes
                };

                if (taskDetail.AssignedTo != trackerAPIResponseModel.EmployeeId)
                {
                    var time = isQA ? await _timeSheetsService.GetQATimeByTaskId(taskDetail.Id) :
                        await _timeSheetsService.GetDevelopmentTimeByTaskId(taskDetail.Id);
                    if (time.SpentHours != 0 || time.SpentMinutes != 0)
                    {
                        taskLogsRootObject.SpentHours = time.SpentHours;
                        taskLogsRootObject.SpentMinutes = time.SpentMinutes;
                    }
                }

                if (taskDetail.StatusId > 0)
                {
                    var workFlowStatus = await _trackerAPIService.GetWorkflowStatusByIdAsync(taskDetail.StatusId);
                    taskLogsRootObject.Status = workFlowStatus.StatusName;
                    taskLogsRootObject.ColorCode = workFlowStatus.ColorCode;
                }

                if (taskDetail.Tasktypeid > 0)
                    taskLogsRootObject.TaskType = Enum.GetName(typeof(TaskTypeEnum), taskDetail.Tasktypeid);

                model.Add(taskLogsRootObject);
            }

            return Json(new
            {
                result = true,
                task_log_details = model
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Get Task Details API Method

    [HttpGet]
    [Route("/api/get_task_details/{taskId}", Name = nameof(GetTaskDetailByTaskId))]
    public virtual async Task<IActionResult> GetTaskDetailByTaskId([FromRoute] int taskId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = $"/api/get_task_details/{taskId}",
                EndPoint = TrackerAPIDefaults.GetTaskDetailsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            var projectTask = await _trackerAPIService.GetProjectTaskByIdAsync(taskId);

            var model = new TaskLogDetailsRootObject();

            var project = await _trackerAPIService.GetProjectByIdAsync(projectTask.ProjectId);

            var taskDetails = new TaskDetailsRootObject()
            {
                TaskTitle = projectTask.TaskTitle,
                ParentTaskId = projectTask.ParentTaskId,
                ProjectName = project.ProjectTitle,
                AssignedTo = projectTask.AssignedTo,
                EstimationTime = await _trackerAPIService.ConvertToHHMMFormat(projectTask.EstimatedTime),
                StatusId = projectTask.StatusId,
                TaskTypeId = projectTask.Tasktypeid,
                DueDate = projectTask.DueDate,
                TaskDescription = projectTask.Description,
            };

            var processRules = await _trackerAPIService.GetProcessRulesByStatusAndProcessWorkflowIdAsync(projectTask.StatusId,
                projectTask.ProcessWorkflowId);

            var workflow = await _trackerAPIService.GetWorkflowStatusByIdAsync(projectTask.StatusId);
            taskDetails.AvailableStatuses.Add(new TaskStatusesRootObject
            {
                Text = workflow.StatusName,
                Value = workflow.Id.ToString(),
                DisplayOrder = workflow.DisplayOrder,
                ColorCode = workflow.ColorCode,
                Selected = projectTask.StatusId.ToString() == workflow.Id.ToString()
            });

            if (processRules.Any())
            {
                foreach (var processRule in processRules)
                {
                    workflow = await _trackerAPIService.GetWorkflowStatusByIdAsync(processRule.ToStateId);
                    taskDetails.AvailableStatuses.Add(new TaskStatusesRootObject
                    {
                        Text = workflow.StatusName,
                        Value = workflow.Id.ToString(),
                        DisplayOrder = workflow.DisplayOrder,
                        ColorCode = workflow.ColorCode,
                        Selected = projectTask.StatusId.ToString() == workflow.Id.ToString()
                    });
                }
            }
            taskDetails.AvailableStatuses = taskDetails.AvailableStatuses.OrderBy(s => s.DisplayOrder).ToList();

            taskDetails.AvailableTaskTypes = (await TaskTypeEnum.Select.ToSelectListAsync()).Select(taskType => new TaskTypesRootObject
            {
                Text = FormatEnumValue(taskType.Text),
                Value = taskType.Value,
                Selected = projectTask.Tasktypeid.ToString() == taskType.Value
            }).ToList();
            model.TaskDetails = taskDetails;

            var processWorkflow = await _trackerAPIService.GetProcessWorkflowByIdAsync(projectTask.ProcessWorkflowId);
            taskDetails.AvailableProcessWorkflows.Add(new ProcessWorkflowRootObject
            {
                Text = processWorkflow.Name,
                Value = processWorkflow.Id
            });

            model.AvailableEmployeeDetails = await GetEmployeesAsync();

            var taskChangeLogs = await _trackerAPIService.GetAllTaskChangeLogsByTaskIdAsync(taskId);
            if (taskChangeLogs.Any())
            {
                model.AvailableTaskChangeLogs = await taskChangeLogs.SelectAwait(async taskChangeLog =>
                {
                    var employee = await _trackerAPIService.GetEmployeeByIdAsync(taskChangeLog.EmployeeId);
                    return new TaskChangeLogsRootObject
                    {
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        Comments = taskChangeLog.Notes,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(taskChangeLog.CreatedOn, DateTimeKind.Utc)
                    };
                }).ToListAsync();
            }

            var taskComments = await _trackerAPIService.GetAllTaskCommentsByTaskIdAsync(taskId);
            if (taskComments.Any())
            {
                model.AvailableTaskComments = await taskComments.SelectAwait(async taskComment =>
                {
                    var employee = await _trackerAPIService.GetEmployeeByIdAsync(taskComment.EmployeeId);
                    return new TaskCommentsRootObject
                    {
                        EmployeeName = $"{employee.FirstName} {employee.LastName}",
                        Comments = taskComment.Description,
                        CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(taskComment.CreatedOn, DateTimeKind.Utc)
                    };
                }).ToListAsync();
            }

            var calculateTaskTime = await _timeSheetsService.GetSpentTimeWithTypesById(taskId);
            model.CalculateTaskTime = new CalculateTaskTimeRootObject()
            {
                BillableDevelopmentTime = calculateTaskTime.BillableDevelopmentTime,
                NotBillableDevelopmentTime = calculateTaskTime.NotBillableDevelopmentTime,
                BillableQATime = calculateTaskTime.BillableQATime,
                NotBillableQATime = calculateTaskTime.NotBillableQATime,
                TotalDevelopmentTime = calculateTaskTime.TotalDevelopmentTime,
                TotalQATime = calculateTaskTime.TotalQATime,
                TotalBillableTime = calculateTaskTime.TotalBillableTime,
                TotalNotBillableTime = calculateTaskTime.TotalNotBillableTime,
                TotalSpentTime = calculateTaskTime.TotalSpentTime
            };

            int assignedTo = 0;
            bool isQa = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId);
            if (!isQa)
                assignedTo = trackerAPIResponseModel.EmployeeId;

            var projectTasks = await _trackerAPIService.GetProjectTasksByProjectIdAsync(projectId: projectTask.ProjectId,
                assignedTo: assignedTo, showHidden: true);
            taskDetails.AvailableParentTasks = projectTasks.Select(projectTask => new ParentTasksRootObject
            {
                Id = projectTask.Id,
                TaskTitle = projectTask.TaskTitle
            }).ToList();

            return Json(model);
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Update Task Details API Method

    [HttpPost]
    [Route("/api/update_task_details", Name = nameof(UpdateTaskDetails))]
    public virtual async Task<IActionResult> UpdateTaskDetails([FromBody] UpdateTaskDetailsParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = "/api/update_task_details",
                EndPoint = TrackerAPIDefaults.UpdateTaskDetailsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            var existingProjectTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);

            bool commentsRequired = await _trackerAPIService.GetProcessRulesByPreviousAndCurrentStatusIdAsync(existingProjectTask.StatusId,
                parameters.StatusId);
            if (commentsRequired && string.IsNullOrWhiteSpace(parameters.Comments))
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.Comments");

            var prevProjectTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);

            var newProjectTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);

            newProjectTask.TaskTitle = parameters.TaskTitle;
            newProjectTask.ParentTaskId = parameters.ParentTaskId;
            newProjectTask.AssignedTo = parameters.AssignedTo;
            newProjectTask.StatusId = parameters.StatusId;
            newProjectTask.Tasktypeid = parameters.TaskTypeId;
            if (parameters.DueDate != null)
                newProjectTask.DueDate = parameters.DueDate;
            newProjectTask.Description = parameters.TaskDescription;
            newProjectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(newProjectTask, prevProjectTask);
            await _trackerAPIService.UpdateProjectTaskAsync(newProjectTask);

            await _taskChangeLogService.InsertTaskChangeLogByUpdateTaskAsync(existingProjectTask, newProjectTask,
                trackerAPIResponseModel.EmployeeId);

            if (!string.IsNullOrWhiteSpace(parameters.Comments))
            {
                var taskComments = new TaskComments()
                {
                    EmployeeId = parameters.AssignedTo,
                    TaskId = parameters.TaskId,
                    StatusId = parameters.StatusId,
                    Description = parameters.Comments,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                };
                await _trackerAPIService.InsertTaskCommentsAsync(taskComments);

                await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
                    taskComments.EmployeeId, taskComments.TaskId, taskComments.Description);
            }

            parameters.Success = true;
            parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.TaskUpdatedSuccessfully");
            return Json(new
            {
                result = true,
                message = parameters.ResponseMessage
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Get Task Types And Process Workflow API Method

    [HttpGet]
    [Route("/api/task_types_and_process_workflow/{projectId}", Name = nameof(GetTaskTypesAndProcessWorkflow))]
    public virtual async Task<IActionResult> GetTaskTypesAndProcessWorkflow([FromRoute] int projectId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = $"/api/task_types_and_process_workflow/{projectId}",
                EndPoint = TrackerAPIDefaults.GetTaskTypesAndProcessWorkflowsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            var employeeTaskTypes = (await TaskTypeEnum.Select.ToSelectListAsync()).Select(taskType => new TaskTypesRootObject
            {
                Text = FormatEnumValue(taskType.Text),
                Value = taskType.Value
            }).ToList();

            var employeeProcessWorkflows = new List<ProcessWorkflowRootObject>();
            var existingProject = await _trackerAPIService.GetProjectByIdAsync(projectId);
            if (existingProject != null && !string.IsNullOrEmpty(existingProject.ProcessWorkflowIds))
            {
                var workflowIds = existingProject.ProcessWorkflowIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(id => int.TryParse(id, out var parsedId) ? parsedId : (int?)null)
                    .Where(id => id.HasValue).Select(id => id.Value).ToList();

                var processWorkflows = await _trackerAPIService.GetProcessWorkflowsByIdsAsync(workflowIds.ToArray());
                if (processWorkflows.Any())
                {
                    employeeProcessWorkflows = processWorkflows.Select(processWorkflow => new ProcessWorkflowRootObject
                    {
                        Text = processWorkflow.Name,
                        Value = processWorkflow.Id
                    }).ToList();
                }
            }
            employeeProcessWorkflows.Insert(0, new ProcessWorkflowRootObject
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.Select"),
                Value = 0
            });

            var parentTasks = new List<ParentTasksRootObject>();

            int assignedTo = 0;
            bool isQa = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId);
            if (!isQa)
                assignedTo = trackerAPIResponseModel.EmployeeId;

            var projectTasks = await _trackerAPIService.GetProjectTasksByProjectIdAsync(projectId: projectId, assignedTo: assignedTo,
                showHidden: true);
            parentTasks = projectTasks.Select(projectTask => new ParentTasksRootObject
            {
                Id = projectTask.Id,
                TaskTitle = projectTask.TaskTitle
            }).ToList();
            parentTasks.Insert(0, new ParentTasksRootObject
            {
                Id = 0,
                TaskTitle = await _localizationService.GetResourceAsync("Admin.Common.Select"),
            });

            var taskCategories = new List<TaskCategoriesRootObject>();
            var taskCategoryMappings = await _projectTaskCategoryMappingService.GetAllMappingsAsync(projectId: projectId);
            foreach (var taskCategoryMapping in taskCategoryMappings)
            {
                var existingTaskCategory = await _taskCategoryService.GetTaskCategoryByIdAsync(taskCategoryMapping.TaskCategoryId);
                if (existingTaskCategory == null)
                    continue;

                taskCategories.Add(new TaskCategoriesRootObject
                {
                    Id = existingTaskCategory.Id,
                    CategoryName = existingTaskCategory.DisplayName
                });
            }
            taskCategories.Insert(0, new TaskCategoriesRootObject
            {
                Id = 0,
                CategoryName = await _localizationService.GetResourceAsync("Admin.Common.Select")
            });

            var employeeDetails = new List<EmployeeDetailsRootObject>();
            var isLeaderOrManager = await _trackerAPIService.CheckIfLeaderandManagerOrNotAsync(trackerAPIResponseModel.EmployeeId);
            if (isLeaderOrManager)
            {
                var employees = await _trackerAPIService.GetAllEmployeesByProjectIdAsync(projectId);
                employeeDetails = employees.Select(employee => new EmployeeDetailsRootObject
                {
                    Id = employee.Id,
                    FullName = employee.FirstName + " " + employee.LastName
                }).ToList();
            }
            else
            {
                var employee = await _trackerAPIService.GetEmployeeByIdAsync(trackerAPIResponseModel.EmployeeId);
                employeeDetails.Add(new EmployeeDetailsRootObject
                {
                    Id = employee.Id,
                    FullName = employee.FirstName + " " + employee.LastName
                });
            }
            employeeDetails.Insert(0, new EmployeeDetailsRootObject
            {
                Id = 0,
                FullName = await _localizationService.GetResourceAsync("Admin.Common.Select")
            });

            return Json(new
            {
                result = true,
                parent_tasks = parentTasks,
                employees = employeeDetails,
                task_types = employeeTaskTypes,
                process_workflows = employeeProcessWorkflows,
                task_categories = taskCategories,
                project_name = existingProject.ProjectTitle
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #endregion

    #region Time Management API Method

    #region Manual Time Entry API Method

    [HttpPost]
    [Route("/api/manual_time_entry", Name = nameof(AddManualTimeEntry))]
    public virtual async Task<IActionResult> AddManualTimeEntry([FromBody] EmployeeManualTimeEntryParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.ManualTimeEntryEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.EmployeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            if (parameters.ProjectId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.ProjectId");

            if (parameters.TaskId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskId");

            int activityId = await InsertOrUpdateActivityAsync(employeeId: parameters.EmployeeId, taskId: parameters.TaskId, activityDescription: parameters.ActivityDescription,
                spentHours: parameters.SpentHours, spentMinutes: parameters.SpentMinutes, manualTimeEntry: true);
            await InsertTimeSheetEntryAsync(activityId: activityId, employeeId: parameters.EmployeeId, projectId: parameters.ProjectId, taskId: parameters.TaskId,
                billable: parameters.Billable, spentDate: parameters.SpentDate, spentHours: parameters.SpentHours, spentMinutes: parameters.SpentMinutes,
                manualTimeEntry: true);
            await UpdateTaskTimeAndStatusAsync(taskId: parameters.TaskId, spentHours: parameters.SpentHours, spentMinutes: parameters.SpentMinutes);
            await InsertOrUpdateAttendanceAsync(employeeId: parameters.EmployeeId, spentHours: parameters.SpentHours, spentMinutes: parameters.SpentMinutes);

            var successMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.ManualTimeEntryAddedSuccessfully");

            return Json(new
            {
                result = true,
                message = successMessage
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #region Start Tracking Methods

    [HttpPost]
    [Route("/api/start_track", Name = nameof(StartTrack))]
    public virtual async Task<IActionResult> StartTrack([FromBody] EmployeeStartTrackParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.StartTrackEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.EmployeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            if (parameters.ProjectId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.ProjectId");

            if (parameters.TaskId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskId");

            int activityId = await InsertOrUpdateActivityAsync(employeeId: parameters.EmployeeId, taskId: parameters.TaskId, activityDescription: parameters.ActivityDescription,
                spentHours: 0, spentMinutes: 0, manualTimeEntry: false);
            await InsertTimeSheetEntryAsync(activityId: activityId, employeeId: parameters.EmployeeId, projectId: parameters.ProjectId, taskId: parameters.TaskId,
                billable: parameters.Billable, spentDate: null, spentHours: 0, spentMinutes: 0, manualTimeEntry: false);

            int statusId = 0;
            var existingTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);
            statusId = existingTask.StatusId;
            bool isQa = await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId);

            if (existingTask != null)
            {
                var processRules = await _trackerAPIService.GetProcessRulesByStatusAndProcessWorkflowIdAsync(existingTask.StatusId, existingTask.ProcessWorkflowId);

                if (processRules.Any())
                {
                    var workflows = await Task.WhenAll(processRules.Select(pr => _trackerAPIService.GetWorkflowStatusByIdAsync(pr.ToStateId)));

                    var matchingRule = processRules.Zip(workflows, (rule, workflow) => new { rule, workflow })
                        .FirstOrDefault(x =>
                            (x.workflow.IsDefaultDeveloperStatus && !isQa || x.workflow.IsDefaultQAStatus && isQa) &&
                            existingTask.StatusId == x.rule.FromStateId);

                    if (matchingRule != null)
                        existingTask.StatusId = matchingRule.rule.ToStateId;
                }

                string prevStatus = (await _trackerAPIService.GetWorkflowStatusByIdAsync(statusId)).StatusName;
                string newStatus = (await _trackerAPIService.GetWorkflowStatusByIdAsync(existingTask.StatusId)).StatusName;

                var notes = string.Format(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.StatusChange"),
                    prevStatus, newStatus);

                if (statusId != existingTask.StatusId)
                {
                    var taskChangeLog = new TaskChangeLog()
                    {
                        EmployeeId = parameters.EmployeeId,
                        TaskId = parameters.TaskId,
                        StatusId = existingTask.StatusId,
                        AssignedTo = existingTask.AssignedTo,
                        LogTypeId = (int)LogTypeEnum.StatusChange,
                        Notes = notes,
                        CreatedOn = DateTime.UtcNow
                    };
                    await _trackerAPIService.InsertTaskChangeLogAsync(taskChangeLog);
                }

                await _trackerAPIService.UpdateProjectTaskAsync(existingTask);
            }

            var existingEmployee = await _trackerAPIService.GetEmployeeByIdAsync(trackerAPIResponseModel.EmployeeId);
            if (existingEmployee != null)
            {
                existingEmployee.ActivityTrackingStatusId = (int)ActivityTrackingEnum.Active;
                await _trackerAPIService.UpdateEmployeeAsync(existingEmployee);
            }

            var existingActivityTracking = await _trackerAPIService.GetActivityTrackingByEmployeeIdAsync(parameters.EmployeeId);
            if (existingActivityTracking != null)
            {
                var existingActivityTrackings = JsonConvert.DeserializeObject<List<EmployeeJsonActivityTrackingEventParametersModel>>
                    (existingActivityTracking.JsonString) ?? new List<EmployeeJsonActivityTrackingEventParametersModel>();
                var lastNodeOfActivityTracking = existingActivityTrackings.LastOrDefault();
                if (lastNodeOfActivityTracking.StatusId == (int)ActivityTrackingEnum.Stopped)
                {
                    int duration = (int)Math.Round((DateTime.UtcNow - lastNodeOfActivityTracking.CreatedOnUtc).TotalMinutes);
                    existingActivityTracking.StoppedDuration += duration;
                    lastNodeOfActivityTracking.Duration += duration;
                    lastNodeOfActivityTracking.CreatedOnUtc = DateTime.UtcNow;
                }

                existingActivityTracking.JsonString = JsonConvert.SerializeObject(existingActivityTrackings);
                existingActivityTracking.EndTime = DateTime.UtcNow;
                await _trackerAPIService.UpdateActivityTrackingAsync(existingActivityTracking);
            }

            var projects = await _trackerAPIService.GetProjectsByEmployeeIdAsync(trackerAPIResponseModel.EmployeeId);

            var lastTimeSheet = await _trackerAPIService.GetLastTimeSheetByEmployeeIdAsync(trackerAPIResponseModel.EmployeeId);

            Activity lastActivity = null;
            string activityName = string.Empty;

            if (lastTimeSheet.ActivityId != 0)
            {
                lastActivity = await _trackerAPIService.GetActivityByIdAsync(lastTimeSheet.ActivityId);
                activityName = lastActivity.ActivityName;
            }

            var existingProjectTask = await _trackerAPIService.GetProjectTaskByIdAsync(lastTimeSheet.TaskId);

            string estimationTime = await _trackerAPIService.ConvertToHHMMFormat(existingProjectTask.EstimatedTime);

            var model = new TaskAlertRootObject()
            {
                EmployeeId = lastTimeSheet.EmployeeId,
                ProjectId = lastTimeSheet.ProjectId,
                TaskId = existingProjectTask.Id,
                ActivityId = lastActivity?.Id ?? 0,
                TaskName = existingProjectTask.TaskTitle,
                ActivityName = activityName,
                EstimationTime = Convert.ToDecimal(estimationTime),
                Billable = lastTimeSheet.Billable,
                StatusId = existingProjectTask.StatusId
            };

            foreach (var project in projects)
            {
                var projectsRootObject = new ProjectRootObject()
                {
                    Id = project.Id,
                    ProjectName = project.ProjectTitle
                };

                int assignedTo = (await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId)) ? 0 : trackerAPIResponseModel.EmployeeId;

                var projectTasks = await _trackerAPIService.GetProjectTasksByProjectIdAsync(projectId: project.Id, assignedTo: assignedTo);
                foreach (var projectTask in projectTasks)
                {
                    string taskEstimationTime = await _trackerAPIService.ConvertToHHMMFormat(projectTask.EstimatedTime);

                    var tasks = new TaskRootObject()
                    {
                        Id = projectTask.Id,
                        TaskTitle = projectTask.TaskTitle,
                        EstimationTime = Convert.ToDecimal(taskEstimationTime),
                        SpentHours = projectTask.SpentHours,
                        SpentMinutes = projectTask.SpentMinutes
                    };

                    var activities = await _trackerAPIService.GetActivitiesByTaskIdAsync(projectTask.Id);
                    tasks.AvailableActivities = activities.Select(activity => new ActivityRootObject
                    {
                        Id = activity.Id,
                        ActivityName = activity.ActivityName
                    }).ToList();

                    projectsRootObject.AvailableTasks.Add(tasks);
                }

                model.AvailableProjects.Add(projectsRootObject);
            }

            var existingTaskAlertConfigurations = await _taskAlertService.GetAllTaskAlertConfigurationsAsync(showHidden: true);
            decimal calculateTaskEstimationTime = existingTask.EstimatedTime;

            if (isQa && existingTask.DeveloperId != parameters.EmployeeId)
                calculateTaskEstimationTime = await _trackerAPIService.CalculateQAEstimationTimeAsync(existingTask);

            foreach (var existingTaskAlertConfiguration in existingTaskAlertConfigurations)
            {
                decimal lastAlertLogPercentage = await _taskAlertService.GetTaskAlertLogByAlertIdAsync(employeeId: parameters.EmployeeId, taskId: existingTask.Id);

                if (lastAlertLogPercentage >= existingTaskAlertConfiguration.Percentage)
                    continue;

                string overdueTotalDuration = string.Empty; string followUpTotalDuration = string.Empty;
                int totalHours = 0; int totalMinutes = 0;
                decimal variation = (calculateTaskEstimationTime * (existingTaskAlertConfiguration.Percentage / 100m));

                overdueTotalDuration = await _trackerAPIService.ConvertToHHMMFormat(variation);
                string[] parts = overdueTotalDuration.Split('.');
                totalHours = int.Parse(parts[0]);
                totalMinutes = int.Parse(parts[1]);
                decimal totalTime = decimal.Parse($"{totalHours}.{totalMinutes:D2}");
                decimal spentTime = decimal.Parse($"{existingTask.SpentHours}.{existingTask.SpentMinutes:D2}");
                if (spentTime > totalTime)
                {
                    totalHours = existingTask.SpentHours;
                    totalMinutes = existingTask.SpentMinutes;
                }

                model.AvailableTaskAlertConfigurations.Add(new TaskAlertConfigurationRootObject
                {
                    Id = existingTaskAlertConfiguration.Id,
                    TaskAlertTypeId = existingTaskAlertConfiguration.TaskAlertTypeId,
                    Message = existingTaskAlertConfiguration.Message,
                    Percentage = existingTaskAlertConfiguration.Percentage,
                    CommentRequired = existingTaskAlertConfiguration.CommentRequired,
                    ReasonRequired = existingTaskAlertConfiguration.ReasonRequired,
                    NewETA = existingTaskAlertConfiguration.NewETA,
                    EnableOnTrack = existingTaskAlertConfiguration.EnableOnTrack,
                    TotalHours = totalHours,
                    TotalMinutes = totalMinutes
                });
            }

            model.AvailableTaskAlertConfigurations = model.AvailableTaskAlertConfigurations.GroupBy(x => new
            { x.TotalHours, x.TotalMinutes }).Select(g => g.OrderByDescending(x => x.Id).First()).ToList();

            parameters.Success = true;
            parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.ActivityStartedSuccessfully");
            return Json(new
            {
                result = true,
                message = parameters.ResponseMessage,
                billable = parameters.Billable,
                task_statuses = model,
                taskLink = (await _storeContext.GetCurrentStoreAsync()).Url + "ProjectTask/Edit?id=" + parameters.TaskId
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Activity Event Methods

    [HttpPost]
    [Route("/api/activity_event", Name = nameof(InsertActivityEvent))]
    public virtual async Task<IActionResult> InsertActivityEvent([FromBody] EmployeeActivityEventParametersModel parameters)
    {
        try
        {
            DateTime endTime = DateTime.UtcNow;
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = "/api/activity_event",
                EndPoint = TrackerAPIDefaults.ActivityEventEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.EmployeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            var timeSheet = await _trackerAPIService.GetTimeSheetByEmployeeIdAsync(parameters.EmployeeId);
            if (timeSheet != null)
            {
                DateTime currentDateTime = DateTime.UtcNow;
                TimeSpan? duration = null;
                if (timeSheet.EndTime == null)
                    duration = currentDateTime - timeSheet.StartTime;
                else
                    duration = currentDateTime - timeSheet.EndTime;
                int totalHours = (int)Math.Floor(duration.Value.TotalHours);
                int totalMinutes = (int)Math.Round(duration.Value.TotalMinutes % 60);
                timeSheet.EndTime = endTime;
                var calculateTimeSheetTime = await _timeSheetsService.AddSpentTimeAsync(timeSheet.SpentHours, timeSheet.SpentMinutes,
                    totalHours, totalMinutes);
                timeSheet.SpentHours = calculateTimeSheetTime.SpentHours;
                timeSheet.SpentMinutes = calculateTimeSheetTime.SpentMinutes;
                timeSheet.UpdateOnUtc = DateTime.UtcNow;
                await _trackerAPIService.UpdateTimeSheetAsync(timeSheet);

                await UpdateActivityTimeAsync(activityId: timeSheet.ActivityId, spentHours: totalHours, spentMinutes: totalMinutes);
                await UpdateTaskTimeAndStatusAsync(taskId: timeSheet.TaskId, spentHours: totalHours, spentMinutes: totalMinutes);
                await InsertOrUpdateAttendanceAsync(employeeId: timeSheet.EmployeeId, spentHours: totalHours, spentMinutes: totalMinutes);
            }

            var existingActivityEvent = await _trackerAPIService.GetActivityEventByTimeSheetIdAndEmployeeIdAsync(timeSheet.Id, parameters.EmployeeId);
            if (existingActivityEvent == null)
            {
                var activityEvent = new ActivityEvent()
                {
                    TimesheetId = timeSheet.Id,
                    KeyboardHits = parameters.KeyboardHits,
                    MouseHits = parameters.MouseHits,
                    EmployeeId = parameters.EmployeeId,
                    CreateOnUtc = DateTime.UtcNow
                };
                var jsonActivityEvent = new EmployeeJsonActivityEventParametersModel()
                {
                    KeyboardHits = parameters.KeyboardHits,
                    MouseHits = parameters.MouseHits,
                    ScreenshotUrl = parameters.ScreenShotUrl,
                    StatusId = (int)ActivityTrackingEnum.Active,
                    CreateOnUtc = DateTime.UtcNow
                };

                int keyboardMouseClick = parameters.KeyboardHits + parameters.MouseHits;
                if (keyboardMouseClick < _trackerAPISettings.MinimumKeyboardMouseClick)
                    jsonActivityEvent.StatusId = (int)ActivityTrackingEnum.Away;

                activityEvent.JsonString = JsonConvert.SerializeObject(new List<object> { jsonActivityEvent });
                var existingEmployee = await _trackerAPIService.GetEmployeeByIdAsync(trackerAPIResponseModel.EmployeeId);
                if (existingEmployee != null)
                {
                    existingEmployee.ActivityTrackingStatusId = jsonActivityEvent.StatusId;
                    await _trackerAPIService.UpdateEmployeeAsync(existingEmployee);
                }
                await _trackerAPIService.InsertActivityEventAsync(activityEvent);

                await InsertActivityTrackingAsync(jsonActivityEvent.StatusId, parameters.EmployeeId);

                parameters.Success = true;
                parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.ActivityEventAddedSuccessfully");
                return Json(new
                {
                    result = true,
                    message = parameters.ResponseMessage
                });
            }
            else
            {
                existingActivityEvent.KeyboardHits += parameters.KeyboardHits;
                existingActivityEvent.MouseHits += parameters.MouseHits;
                var jsonActivityEvent = new EmployeeJsonActivityEventParametersModel()
                {
                    KeyboardHits = parameters.KeyboardHits,
                    MouseHits = parameters.MouseHits,
                    ScreenshotUrl = parameters.ScreenShotUrl,
                    StatusId = (int)ActivityTrackingEnum.Active,
                    CreateOnUtc = DateTime.UtcNow
                };

                int keyboardMouseClick = parameters.KeyboardHits + parameters.MouseHits;
                if (keyboardMouseClick < _trackerAPISettings.MinimumKeyboardMouseClick)
                    jsonActivityEvent.StatusId = (int)ActivityTrackingEnum.Away;

                var activityEvents = JsonConvert.DeserializeObject<List<EmployeeJsonActivityEventParametersModel>>(existingActivityEvent.JsonString);
                var lastNode = activityEvents?.LastOrDefault();
                bool isLastNodeOffline = lastNode != null && lastNode.StatusId == (int)ActivityTrackingEnum.Offline;

                if (isLastNodeOffline && keyboardMouseClick < _trackerAPISettings.MinimumKeyboardMouseClick)
                    jsonActivityEvent.StatusId = (int)ActivityTrackingEnum.Offline;
                else
                {
                    int lastAwayCount = _trackerAPISettings.LastAwayCount;
                    bool areLastNodesAway = false;
                    if (activityEvents.Count >= lastAwayCount)
                    {
                        areLastNodesAway = activityEvents.Skip(activityEvents.Count - lastAwayCount).All(x => x.StatusId == (int)ActivityTrackingEnum.Away);
                        if (areLastNodesAway)
                            jsonActivityEvent.StatusId = (int)ActivityTrackingEnum.Offline;
                    }
                }

                var existingActivityEvents = JsonConvert.DeserializeObject<List<EmployeeJsonActivityEventParametersModel>>(existingActivityEvent.JsonString) ?? new List<EmployeeJsonActivityEventParametersModel>();
                existingActivityEvents.Add(jsonActivityEvent);
                existingActivityEvent.JsonString = JsonConvert.SerializeObject(existingActivityEvents);
                var existingEmployee = await _trackerAPIService.GetEmployeeByIdAsync(trackerAPIResponseModel.EmployeeId);
                if (existingEmployee != null)
                {
                    existingEmployee.ActivityTrackingStatusId = jsonActivityEvent.StatusId;
                    await _trackerAPIService.UpdateEmployeeAsync(existingEmployee);
                }
                await _trackerAPIService.UpdateActivityEventAsync(existingActivityEvent);

                await InsertActivityTrackingAsync(jsonActivityEvent.StatusId, parameters.EmployeeId);

                parameters.Success = true;
                parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.ActivityEventUpdatedSuccessfully");
                return Json(new
                {
                    result = true,
                    message = parameters.ResponseMessage
                });
            }
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Stop Tracking Methods

    [HttpPost]
    [Route("/api/stop_track", Name = nameof(StopTrack))]
    public virtual async Task<IActionResult> StopTrack([FromBody] EmployeeStopTrackParametersModel parameters)
    {
        try
        {
            DateTime endTime = DateTime.UtcNow;
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.StopTrackEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.EmployeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            var timeSheet = await _trackerAPIService.GetTimeSheetByEmployeeIdAsync(parameters.EmployeeId);

            if (timeSheet != null)
            {
                DateTime currentDateTime = DateTime.UtcNow;
                TimeSpan? duration = null;
                if (timeSheet.EndTime == null)
                    duration = currentDateTime - timeSheet.StartTime;
                else
                    duration = currentDateTime - timeSheet.EndTime;
                int totalHours = (int)Math.Floor(duration.Value.TotalHours);
                int totalMinutes = (int)Math.Round(duration.Value.TotalMinutes % 60);
                timeSheet.EndTime = endTime;
                var calculateTimeSheetTime = await _timeSheetsService.AddSpentTimeAsync(timeSheet.SpentHours, timeSheet.SpentMinutes,
                    totalHours, totalMinutes);
                timeSheet.SpentHours = calculateTimeSheetTime.SpentHours;
                timeSheet.SpentMinutes = calculateTimeSheetTime.SpentMinutes;
                timeSheet.UpdateOnUtc = DateTime.UtcNow;
                await _trackerAPIService.UpdateTimeSheetAsync(timeSheet);

                await UpdateActivityTimeAsync(activityId: timeSheet.ActivityId, spentHours: totalHours, spentMinutes: totalMinutes);
                await UpdateTaskTimeAndStatusAsync(taskId: timeSheet.TaskId, spentHours: totalHours, spentMinutes: totalMinutes);
                await InsertOrUpdateAttendanceAsync(employeeId: timeSheet.EmployeeId, spentHours: totalHours, spentMinutes: totalMinutes);

                var existingEmployee = await _trackerAPIService.GetEmployeeByIdAsync(parameters.EmployeeId);
                if (existingEmployee != null)
                {
                    existingEmployee.ActivityTrackingStatusId = (int)ActivityTrackingEnum.Stopped;
                    await _trackerAPIService.UpdateEmployeeAsync(existingEmployee);
                }

                var existingActivityTracking = await _trackerAPIService.GetActivityTrackingByEmployeeIdAsync(parameters.EmployeeId);
                if (existingActivityTracking == null)
                {
                    var activityTracking = new ActivityTracking()
                    {
                        EmployeeId = parameters.EmployeeId,
                        ActiveDuration = 0,
                        AwayDuration = 0,
                        OfflineDuration = 0,
                        StoppedDuration = 0,
                        StartTime = DateTime.UtcNow,
                        EndTime = DateTime.UtcNow
                    };

                    var jsonActivityTracking = new EmployeeJsonActivityTrackingEventParametersModel()
                    {
                        StatusId = (int)ActivityTrackingEnum.Stopped,
                        Duration = totalMinutes,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    int keyboardMouseClick = parameters.KeyboardHits + parameters.MouseHits;
                    if (keyboardMouseClick >= _trackerAPISettings.MinimumKeyboardMouseClick)
                    {
                        jsonActivityTracking.StatusId = (int)ActivityTrackingEnum.Active;
                        activityTracking.ActiveDuration += totalMinutes;
                    }
                    if (keyboardMouseClick < _trackerAPISettings.MinimumKeyboardMouseClick)
                    {
                        jsonActivityTracking.StatusId = (int)ActivityTrackingEnum.Away;
                        activityTracking.AwayDuration += totalMinutes;
                    }
                    activityTracking.JsonString = JsonConvert.SerializeObject(new List<object> { jsonActivityTracking });
                    activityTracking.TotalDuration += activityTracking.ActiveDuration + activityTracking.AwayDuration +
                            activityTracking.OfflineDuration + activityTracking.StoppedDuration;
                    await _trackerAPIService.InsertActivityTrackingAsync(activityTracking);

                    existingActivityTracking = await _trackerAPIService.GetActivityTrackingByEmployeeIdAsync(parameters.EmployeeId);
                    if (existingActivityTracking != null)
                    {
                        var existingActivityTrackings = JsonConvert.DeserializeObject<List<EmployeeJsonActivityTrackingEventParametersModel>>
                            (existingActivityTracking.JsonString) ?? new List<EmployeeJsonActivityTrackingEventParametersModel>();
                        existingActivityTrackings.Add(new EmployeeJsonActivityTrackingEventParametersModel
                        {
                            StatusId = (int)ActivityTrackingEnum.Stopped,
                            Duration = 0,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                        existingActivityTracking.JsonString = JsonConvert.SerializeObject(existingActivityTrackings);
                        existingActivityTracking.EndTime = DateTime.UtcNow;
                        await _trackerAPIService.UpdateActivityTrackingAsync(existingActivityTracking);
                    }
                }
                else
                {
                    int statusId = 0;
                    var existingActivityTrackings = JsonConvert.DeserializeObject<List<EmployeeJsonActivityTrackingEventParametersModel>>
                        (existingActivityTracking.JsonString) ?? new List<EmployeeJsonActivityTrackingEventParametersModel>();
                    var lastNodeOfActivityTracking = existingActivityTrackings?.LastOrDefault();
                    int keyboardMouseClick = parameters.KeyboardHits + parameters.MouseHits;
                    if (keyboardMouseClick >= _trackerAPISettings.MinimumKeyboardMouseClick)
                        statusId = (int)ActivityTrackingEnum.Active;
                    if (keyboardMouseClick < _trackerAPISettings.MinimumKeyboardMouseClick)
                        statusId = (int)ActivityTrackingEnum.Away;

                    var existingActivityEvent = await _trackerAPIService.GetActivityEventByTimeSheetIdAndEmployeeIdAsync(timeSheet.Id, parameters.EmployeeId);
                    if (existingActivityEvent != null)
                    {
                        var activityEvents = JsonConvert.DeserializeObject<List<EmployeeJsonActivityEventParametersModel>>(existingActivityEvent.JsonString);
                        bool areLastNodesAway = false;
                        if (activityEvents.Count >= _trackerAPISettings.LastAwayCount)
                        {
                            areLastNodesAway = activityEvents.Skip(activityEvents.Count - _trackerAPISettings.LastAwayCount)
                                .All(x => x.StatusId == (int)ActivityTrackingEnum.Away);
                            if (areLastNodesAway)
                                statusId = (int)ActivityTrackingEnum.Offline;
                        }
                    }

                    if (lastNodeOfActivityTracking != null && lastNodeOfActivityTracking.StatusId == statusId)
                    {
                        lastNodeOfActivityTracking.Duration += totalMinutes;
                        lastNodeOfActivityTracking.CreatedOnUtc = DateTime.UtcNow;
                    }
                    else
                    {
                        existingActivityTrackings.Add(new EmployeeJsonActivityTrackingEventParametersModel
                        {
                            StatusId = statusId,
                            Duration = totalMinutes,
                            CreatedOnUtc = DateTime.UtcNow
                        });
                    }

                    if (statusId == (int)ActivityTrackingEnum.Active)
                        existingActivityTracking.ActiveDuration += totalMinutes;
                    else if (statusId == (int)ActivityTrackingEnum.Away)
                        existingActivityTracking.AwayDuration += totalMinutes;
                    else if (statusId == (int)ActivityTrackingEnum.Offline)
                        existingActivityTracking.OfflineDuration += totalMinutes;
                    existingActivityTracking.JsonString = JsonConvert.SerializeObject(existingActivityTrackings);
                    existingActivityTracking.TotalDuration += totalMinutes;
                    existingActivityTracking.EndTime = DateTime.UtcNow;
                    await _trackerAPIService.UpdateActivityTrackingAsync(existingActivityTracking);

                    existingActivityTrackings.Add(new EmployeeJsonActivityTrackingEventParametersModel
                    {
                        StatusId = (int)ActivityTrackingEnum.Stopped,
                        Duration = 0,
                        CreatedOnUtc = DateTime.UtcNow
                    });
                    existingActivityTracking.JsonString = JsonConvert.SerializeObject(existingActivityTrackings);
                    existingActivityTracking.EndTime = DateTime.UtcNow;
                    await _trackerAPIService.UpdateActivityTrackingAsync(existingActivityTracking);
                }

                if (parameters.KeyboardHits != 0 || parameters.MouseHits != 0)
                {
                    var existingActivityEvent = await _trackerAPIService.GetActivityEventByTimeSheetIdAndEmployeeIdAsync(timeSheet.Id, parameters.EmployeeId);
                    if (existingActivityEvent == null)
                    {
                        var activityEvent = new ActivityEvent()
                        {
                            TimesheetId = timeSheet.Id,
                            KeyboardHits = parameters.KeyboardHits,
                            MouseHits = parameters.MouseHits,
                            EmployeeId = parameters.EmployeeId,
                            CreateOnUtc = DateTime.UtcNow
                        };

                        var jsonActivityEvent = new EmployeeJsonActivityEventParametersModel()
                        {
                            KeyboardHits = parameters.KeyboardHits,
                            MouseHits = parameters.MouseHits,
                            ScreenshotUrl = string.Empty,
                            StatusId = (int)ActivityTrackingEnum.Stopped,
                            CreateOnUtc = DateTime.UtcNow
                        };
                        activityEvent.JsonString = JsonConvert.SerializeObject(new List<object> { jsonActivityEvent });
                        await _trackerAPIService.InsertActivityEventAsync(activityEvent);
                    }
                    else
                    {
                        existingActivityEvent.KeyboardHits += parameters.KeyboardHits;
                        existingActivityEvent.MouseHits += parameters.MouseHits;
                        var jsonActivityEvent = new EmployeeJsonActivityEventParametersModel()
                        {
                            KeyboardHits = parameters.KeyboardHits,
                            MouseHits = parameters.MouseHits,
                            ScreenshotUrl = string.Empty,
                            StatusId = (int)ActivityTrackingEnum.Stopped,
                            CreateOnUtc = DateTime.UtcNow
                        };

                        var existingActivityEvents = JsonConvert.DeserializeObject<List<EmployeeJsonActivityEventParametersModel>>(existingActivityEvent.JsonString) ?? new List<EmployeeJsonActivityEventParametersModel>();
                        existingActivityEvents.Add(jsonActivityEvent);
                        existingActivityEvent.JsonString = JsonConvert.SerializeObject(existingActivityEvents);
                        await _trackerAPIService.UpdateActivityEventAsync(existingActivityEvent);
                    }
                }

                parameters.Success = true;
                parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.ActivityStoppedSuccessfully");
                return Json(new
                {
                    result = true,
                    message = parameters.ResponseMessage
                });
            }

            parameters.Success = false;
            parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.ThereIsNoStartActivityPresent");
            return Json(new
            {
                result = true,
                message = parameters.ResponseMessage
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #endregion

    #region Overdue Task & Switch Task & Task Follow Up & Task Reasons Methods

    #region Task Alert API Method

    [HttpGet]
    [Route("/api/task_alert/{employeeId}", Name = nameof(GetLastProjectTasksByEmployeeId))]
    public virtual async Task<IActionResult> GetLastProjectTasksByEmployeeId([FromRoute] int employeeId)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = $"/api/task_alert/{employeeId}",
                EndPoint = TrackerAPIDefaults.GetTaskAlertProjectTaskEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (employeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            var projects = await _trackerAPIService.GetProjectsByEmployeeIdAsync(employeeId);
            if (!projects.Any())
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.NoProjectsFound");

            var lastTimeSheet = await _trackerAPIService.GetLastTimeSheetByEmployeeIdAsync(employeeId);

            Activity lastActivity = null;
            string activityName = string.Empty;

            if (lastTimeSheet.ActivityId != 0)
            {
                lastActivity = await _trackerAPIService.GetActivityByIdAsync(lastTimeSheet.ActivityId);
                activityName = lastActivity.ActivityName;
            }

            var existingTask = await _trackerAPIService.GetProjectTaskByIdAsync(lastTimeSheet.TaskId);

            var model = new TaskAlertRootObject()
            {
                EmployeeId = lastTimeSheet.EmployeeId,
                ProjectId = lastTimeSheet.ProjectId,
                TaskId = existingTask.Id,
                ActivityId = lastActivity?.Id ?? 0,
                TaskName = existingTask.TaskTitle,
                ActivityName = activityName,
                EstimationTime = Convert.ToDecimal(await _trackerAPIService.ConvertToHHMMFormat(existingTask.EstimatedTime)),
                Billable = lastTimeSheet.Billable,
                StatusId = existingTask.StatusId
            };

            foreach (var project in projects)
            {
                var projectsRootObject = new ProjectRootObject()
                {
                    Id = project.Id,
                    ProjectName = project.ProjectTitle
                };

                int assignedTo = (await _trackerAPIService.CheckIfQaOrNotAsync(trackerAPIResponseModel.EmployeeId)) ? 0 : trackerAPIResponseModel.EmployeeId;

                var projectTasks = await _trackerAPIService.GetProjectTasksByProjectIdAsync(projectId: project.Id, assignedTo: assignedTo);
                foreach (var projectTask in projectTasks)
                {
                    string taskEstimationTime = await _trackerAPIService.ConvertToHHMMFormat(projectTask.EstimatedTime);

                    var tasks = new TaskRootObject()
                    {
                        Id = projectTask.Id,
                        TaskTitle = projectTask.TaskTitle,
                        EstimationTime = Convert.ToDecimal(taskEstimationTime),
                        SpentHours = projectTask.SpentHours,
                        SpentMinutes = projectTask.SpentMinutes
                    };

                    var activities = await _trackerAPIService.GetActivitiesByTaskIdAsync(projectTask.Id);
                    tasks.AvailableActivities = activities.Select(activity => new ActivityRootObject
                    {
                        Id = activity.Id,
                        ActivityName = activity.ActivityName
                    }).ToList();
                    projectsRootObject.AvailableTasks.Add(tasks);
                }

                model.AvailableProjects.Add(projectsRootObject);
            }

            trackerAPIResponseModel.Success = true;
            trackerAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

            return Json(model);
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Task Alert Reasons API Method

    [HttpGet]
    [Route("/api/task_alert_reasons", Name = nameof(TaskAlertReasons))]
    public virtual async Task<IActionResult> TaskAlertReasons()
    {
        try
        {
            var trackerAPILog = new TrackerAPILog()
            {
                RequestJson = "/api/task_alert_reasons",
                EndPoint = TrackerAPIDefaults.GetTaskAlertReasonsEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            var taskAlertReasons = await _taskAlertService.GetAllTaskAlertReasonsAsync(showHidden: true);
            var model = taskAlertReasons.Select(taskAlertReason => new SelectListItem
            {
                Text = taskAlertReason.Name,
                Value = taskAlertReason.Name
            }).ToList();

            return Json(new
            {
                result = true,
                task_alert_reasons = model
            });
        }
        catch (Exception exception)
        {
            return await HandleTrackerAPIExceptionAsync(exception);
        }
    }

    #endregion

    #region Send Overdue Task & Follow up Email Utiliities

    private async Task<(List<string>, string)> GetCcEmailsAsync(Employee employee, int projectId, TaskAlertConfiguration taskAlertConfig)
    {
        string leaderOrManagerName = string.Empty;
        var ccEmails = new List<string>();

        if (taskAlertConfig.EnableCoordinatorMail)
        {
            int projectCoorddinatorId = await _projectsService.GetProjectCoordinatorIdByIdAsync(projectId);
            var projectCoorddinator = await _employeeService.GetEmployeeByIdAsync(projectCoorddinatorId);
            if (projectCoorddinator != null && employee.OfficialEmail != projectCoorddinator.OfficialEmail)
            {
                leaderOrManagerName = projectCoorddinator.FirstName + " " + projectCoorddinator.LastName;
                ccEmails.Add(projectCoorddinator.OfficialEmail);
            }
        }

        if (taskAlertConfig.EnableLeaderMail && !ccEmails.Any())
        {
            int leaderId = await _projectsService.GetProjectLeaderIdByIdAsync(projectId);
            var leader = await _employeeService.GetEmployeeByIdAsync(leaderId);
            if (leader != null && employee.OfficialEmail != leader.OfficialEmail)
            {
                leaderOrManagerName = leader.FirstName + " " + leader.LastName;
                ccEmails.Add(leader.OfficialEmail);
            }
        }

        if (taskAlertConfig.EnableManagerMail)
        {
            int managerId = await _projectsService.GetProjectManagerIdByIdAsync(projectId);
            var manager = await _employeeService.GetEmployeeByIdAsync(managerId);
            if (manager != null && employee.OfficialEmail != manager.OfficialEmail)
            {
                leaderOrManagerName = manager.FirstName + " " + manager.LastName;
                ccEmails.Add(manager.OfficialEmail);
            }
        }

        return (ccEmails, leaderOrManagerName);
    }

    private async Task SendTaskOverdueFollowUpEmailAsync(int employeeId = 0, int alertId = 0, int projectId = 0, string spentTime = null,
        string reason = null, string comment = null, bool isOnTrack = false, string etaHours = null, ProjectTask projectTask = null,
        TaskAlertConfiguration taskAlertConfiguration = null)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
        var project = await _projectsService.GetProjectsByIdAsync(projectId);
        var (ccEmails, managerName) = await GetCcEmailsAsync(employee, projectId, taskAlertConfiguration);

        string taskEstimationTime = await _trackerAPIService.ConvertToHHMMFormat(projectTask.EstimatedTime);

        if (string.IsNullOrWhiteSpace(etaHours))
            taskAlertConfiguration.NewETA = false;
        if (string.IsNullOrWhiteSpace(reason))
            taskAlertConfiguration.ReasonRequired = false;
        if (string.IsNullOrWhiteSpace(comment))
            taskAlertConfiguration.CommentRequired = false;

        string alertType = Enum.GetName(typeof(TaskAlertsEnum), taskAlertConfiguration.TaskAlertTypeId);

        await _workflowMessageService.SendTaskOverdueFollowUpEmailAsync(employee, ccEmails, projectTask.Id, managerName, project.ProjectTitle, alertType, projectTask.TaskTitle,
            taskEstimationTime.Replace(".", ":"), spentTime, reason, comment, taskAlertConfiguration.Percentage, taskAlertConfiguration.ReasonRequired,
            taskAlertConfiguration.CommentRequired, taskAlertConfiguration.NewETA, etaHours, isOnTrack,
            (await _workContext.GetWorkingLanguageAsync()).Id);

        await LogTaskAlertAsync(employeeId, projectTask.Id, alertId, reason, comment, taskAlertConfiguration.Percentage, isOnTrack, etaHours);
    }

    private async Task LogTaskAlertAsync(int employeeId, int taskId, int alertId, string reason, string comment, decimal percentage,
        bool isOnTrack, string etaHours)
    {
        TaskAlertReason existingReason = null;
        if (!string.IsNullOrWhiteSpace(reason))
            existingReason = await _taskAlertService.GetTaskAlertReasonByNameAsync(reason);

        var existingProjectTask = await _trackerAPIService.GetProjectTaskByIdAsync(taskId);

        DateTime? nextFollowupDateTime = null;

        var existingAlertConfiguration =
            await _taskAlertService.GetTaskAlertConfigurationByIdAsync(alertId);

        if (existingAlertConfiguration != null)
        {
            var existingNextAlertConfiguration = await _taskAlertService.GetNextTaskAlertConfigurationAsync(existingAlertConfiguration.Percentage);

            if (existingNextAlertConfiguration != null)
            {
                decimal percentageDifference = existingNextAlertConfiguration.Percentage - existingAlertConfiguration.Percentage;
                int estimatedMinutes = await _trackerAPIService.ConvertHoursToMinutes(existingProjectTask.EstimatedTime);
                int minutesToAdd = (int)Math.Round(estimatedMinutes * (percentageDifference / 100m));
                TimeZoneInfo officeTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
                DateTime officeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, officeTimeZone);
                DateTime adjustedOfficeTime = AdjustToOfficeHours(officeNow, minutesToAdd, officeTimeZone);
                nextFollowupDateTime = TimeZoneInfo.ConvertTimeToUtc(adjustedOfficeTime, officeTimeZone);
            }
        }

        int reviewerId = await _projectsService.GetProjectCoordinatorIdByIdAsync(existingProjectTask.ProjectId);
        if (reviewerId == 0)
            reviewerId = await _projectsService.GetProjectLeaderIdByIdAsync(existingProjectTask.ProjectId);
        if (reviewerId == 0)
            reviewerId = await _projectsService.GetProjectManagerIdByIdAsync(existingProjectTask.ProjectId);

        FollowUpTask followUpTask;

        var existingFollowupTask = await _followUpTaskService.GetFollowUpTaskByTaskIdAsync(taskId);
        if (existingFollowupTask != null)
        {
            bool isManualEntryExist = await _followUpTaskService.CheckIfManaualFollowupExistsAsync(taskId);
            if (!isManualEntryExist)
            {
                existingFollowupTask.AlertId = alertId;
                existingFollowupTask.OnTrack = isOnTrack;
                existingFollowupTask.ReasonId = isOnTrack ? 0 : (existingReason?.Id ?? 0);
                existingFollowupTask.ETAHours = isOnTrack ? string.Empty : etaHours;
                existingFollowupTask.ReviewerId = reviewerId;
                existingFollowupTask.LastFollowupDateTime = DateTime.UtcNow;
                existingFollowupTask.NextFollowupDateTime = nextFollowupDateTime;
                existingFollowupTask.LastComment = isOnTrack ? string.Empty : comment;
                await _followUpTaskService.UpdateFollowUpTaskAsync(existingFollowupTask);
            }
            followUpTask = existingFollowupTask;
        }
        else
        {
            followUpTask = new FollowUpTask
            {
                TaskId = taskId,
                AlertId = alertId,
                OnTrack = isOnTrack,
                ReasonId = isOnTrack ? 0 : (existingReason?.Id ?? 0),
                ETAHours = isOnTrack ? string.Empty : etaHours,
                ReviewerId = reviewerId,
                LastFollowupDateTime = DateTime.UtcNow,
                NextFollowupDateTime = nextFollowupDateTime,
                LastComment = isOnTrack ? string.Empty : comment,
                IsCompleted = false
            };
            await _followUpTaskService.InsertFollowUpTaskAsync(followUpTask);
        }

        var taskAlertLog = new TaskAlertLog
        {
            EmployeeId = employeeId,
            TaskId = taskId,
            AlertId = alertId,
            Variation = percentage,
            MailSent = true,
            OnTrack = isOnTrack,
            ReasonId = isOnTrack ? 0 : (existingReason?.Id ?? 0),
            Comment = isOnTrack ? string.Empty : comment,
            ETAHours = isOnTrack ? string.Empty : etaHours,
            FollowUpTaskId = followUpTask.Id,
            ReviewerId = reviewerId,
            NextFollowupDateTime = nextFollowupDateTime,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow
        };

        await _taskAlertService.InsertTaskAlertLogAsync(taskAlertLog);
    }

    private static bool IsWeekend(DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
    }

    private static DateTime GetNextWorkingDay(DateTime date)
    {
        do
        {
            date = date.AddDays(1);
        }
        while (IsWeekend(date));

        return date;
    }

    private static DateTime AdjustToOfficeHours(DateTime startTime, int minutesToAdd, TimeZoneInfo officeTimeZone)
    {
        DateTime officeStart = startTime.Date.AddHours(10);
        DateTime officeEnd = startTime.Date.AddHours(19);

        if (startTime < officeStart)
            startTime = officeStart;

        if (startTime >= officeEnd)
            startTime = GetNextWorkingDay(startTime).Date.AddHours(10);

        while (minutesToAdd > 0)
        {
            officeEnd = startTime.Date.AddHours(19);

            int availableMinutes =
                (int)(officeEnd - startTime).TotalMinutes;

            if (minutesToAdd <= availableMinutes)
            {
                startTime = startTime.AddMinutes(minutesToAdd);
                minutesToAdd = 0;
            }
            else
            {
                minutesToAdd -= availableMinutes;
                startTime = GetNextWorkingDay(startTime).Date.AddHours(10);
            }
        }

        return startTime;
    }

    #endregion

    #region Send Overdue Task & Follow up Email API Method

    [HttpPost]
    [Route("/api/send_task_alert", Name = nameof(SendTaskAlertEmail))]
    public virtual async Task<IActionResult> SendTaskAlertEmail([FromBody] EmployeeTaskAlertParametersModel parameters)
    {
        try
        {
            DateTime endTime = DateTime.UtcNow;
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.SendTaskAlertEmailEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.TaskId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskId");

            var projectTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);
            var alertConfig = await _taskAlertService.GetTaskAlertConfigurationByIdAsync(parameters.AlertId);

            if (projectTask != null && alertConfig != null)
            {
                var timeSheet = await _trackerAPIService.GetTimeSheetByEmployeeIdAsync(parameters.EmployeeId);

                DateTime currentDateTime = DateTime.UtcNow;
                TimeSpan? duration = null;
                if (timeSheet.EndTime == null)
                    duration = currentDateTime - timeSheet.StartTime;
                else
                    duration = currentDateTime - timeSheet.EndTime;
                int totalHours = (int)Math.Floor(duration.Value.TotalHours);
                int totalMinutes = (int)Math.Round(duration.Value.TotalMinutes % 60);
                timeSheet.EndTime = endTime;
                var calculateTimeSheetTime = await _timeSheetsService.AddSpentTimeAsync(timeSheet.SpentHours, timeSheet.SpentMinutes,
                    totalHours, totalMinutes);
                timeSheet.SpentHours = calculateTimeSheetTime.SpentHours;
                timeSheet.SpentMinutes = calculateTimeSheetTime.SpentMinutes;
                timeSheet.UpdateOnUtc = DateTime.UtcNow;
                await _trackerAPIService.UpdateTimeSheetAsync(timeSheet);

                await UpdateActivityTimeAsync(activityId: timeSheet.ActivityId, spentHours: totalHours, spentMinutes: totalMinutes);
                await UpdateTaskTimeAndStatusAsync(taskId: timeSheet.TaskId, spentHours: totalHours, spentMinutes: totalMinutes);
                await InsertOrUpdateAttendanceAsync(employeeId: timeSheet.EmployeeId, spentHours: totalHours, spentMinutes: totalMinutes);

                await SendTaskOverdueFollowUpEmailAsync(parameters.EmployeeId, parameters.AlertId, projectTask.ProjectId, parameters.SpentTime,
                    parameters.Reason, parameters.Comment, parameters.IsOnTrack, parameters.NewETA, projectTask, alertConfig);

                parameters.Success = true;
                parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.EmailSentSuccessfully");

                return Json(new
                {
                    result = true,
                    message = parameters.ResponseMessage
                });
            }

            parameters.Success = false;
            await LogTrackerAPICallAsync(parameters, trackerAPILog);

            return Json(new
            {
                result = false,
                message = parameters.ResponseMessage
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #endregion

    #region Task Statuses Methods

    #region Task Statuses Utitlities

    private async Task<string> ValidateChecklistItemsAsync(int taskCategoryId, int statusId, IEnumerable<EmployeeTaskChecklistItemParametersModel> checklistItems)
    {
        foreach (var item in checklistItems)
        {
            var mapping = (await _checkListMappingService.GetAllCheckListMappingsAsync(taskCategoryId, statusId, item.ChecklistId)).FirstOrDefault();
            if (mapping?.IsMandatory == true && !item.IsChecked)
            {
                var checklist = await _checkListMasterService.GetCheckListByIdAsync(item.ChecklistId);
                return string.Format(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.TaskCheckList.Required"), checklist?.Title ?? "Checklist Item");
            }
        }
        return null;
    }

    private async Task<string> BuildChecklistNotesAsync(IEnumerable<EmployeeTaskChecklistItemParametersModel> checklistItems)
    {
        var sb = new StringBuilder();
        foreach (var item in checklistItems)
        {
            var checklist = await _checkListMasterService.GetCheckListByIdAsync(item.ChecklistId);
            if (checklist != null)
            {
                var emoji = item.IsChecked ? "✅" : "❌";
                sb.Append($"• {checklist.Title} {emoji}<br>");
            }
        }
        return sb.ToString();
    }

    #endregion

    #region Task Statuses API Method

    [HttpGet]
    [Route("/api/task_statuses", Name = nameof(GetTaskStatuses))]
    public virtual async Task<IActionResult> GetTaskStatuses([FromQuery] int? taskId = null, [FromQuery] int? processWorkflowId = null)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = $"taskId={taskId}&processWorkflowId={processWorkflowId}",
                EndPoint = TrackerAPIDefaults.GetTaskStatusesEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            List<TaskStatusesRootObject> taskStatuses = new();

            if (taskId.HasValue)
            {
                var existingProjectTask = await _trackerAPIService.GetProjectTaskByIdAsync(taskId.Value);
                var processRules = await _trackerAPIService.GetProcessRulesByStatusAndProcessWorkflowIdAsync(
                    existingProjectTask.StatusId, existingProjectTask.ProcessWorkflowId);

                var currentWorkflow = await _trackerAPIService.GetWorkflowStatusByIdAsync(existingProjectTask.StatusId);
                if (currentWorkflow != null)
                {
                    taskStatuses.Add(new TaskStatusesRootObject
                    {
                        Text = currentWorkflow.StatusName,
                        Value = currentWorkflow.Id.ToString(),
                        DisplayOrder = currentWorkflow.DisplayOrder,
                        ColorCode = currentWorkflow.ColorCode,
                        Selected = true
                    });
                }

                var addedStatusIds = new HashSet<int> { existingProjectTask.StatusId };
                foreach (var processRule in processRules)
                {
                    if (addedStatusIds.Add(processRule.ToStateId))
                    {
                        var workflow = await _trackerAPIService.GetWorkflowStatusByIdAsync(processRule.ToStateId);
                        if (workflow != null)
                        {
                            taskStatuses.Add(new TaskStatusesRootObject
                            {
                                Text = workflow.StatusName,
                                Value = workflow.Id.ToString(),
                                DisplayOrder = workflow.DisplayOrder,
                                ColorCode = workflow.ColorCode,
                                Selected = false
                            });
                        }
                    }
                }
                taskStatuses = taskStatuses.OrderBy(s => s.DisplayOrder).ToList();
            }
            else if (processWorkflowId.HasValue)
            {
                var workflowStatuses = await _trackerAPIService.GetWorkflowStatusesByProcessWorkflowIdAsync(processWorkflowId.Value)
                                       ?? new List<WorkflowStatus>();

                var localizedAllText = await _localizationService.GetResourceAsync("Admin.Common.All");

                taskStatuses = workflowStatuses.Select(status => new TaskStatusesRootObject
                {
                    Text = status.StatusName,
                    Value = status.Id.ToString(),
                    ColorCode = status.ColorCode
                }).ToList();

                taskStatuses.Insert(0, new TaskStatusesRootObject
                {
                    Text = localizedAllText,
                    Value = "0",
                    ColorCode = string.Empty
                });
            }

            return Json(new
            {
                result = true,
                task_statuses = taskStatuses
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #region Save Task Status API Method

    [HttpPost]
    [Route("/api/task_status", Name = nameof(TaskStatus))]
    public virtual async Task<IActionResult> TaskStatus([FromBody] EmployeeTaskStatusChangeParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = "/api/task_status",
                EndPoint = TrackerAPIDefaults.TaskStatusChangeEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.EmployeeId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.EmployeeId");

            var existingTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);
            bool commentsRequired = await _trackerAPIService.GetProcessRulesByPreviousAndCurrentStatusIdAsync(existingTask.StatusId, parameters.StatusId);
            if (commentsRequired && string.IsNullOrWhiteSpace(parameters.Comments))
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.Comments");

            var prevStatus = await _trackerAPIService.GetWorkflowStatusByIdAsync(existingTask.StatusId);
            var newStatus = await _trackerAPIService.GetWorkflowStatusByIdAsync(parameters.StatusId);
            var notes = string.Format(await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.StatusChange"),
                prevStatus.StatusName, newStatus.StatusName);

            if (parameters.AvailableTaskChecklistItems?.Any() == true)
            {
                var validationError = await ValidateChecklistItemsAsync(existingTask.TaskCategoryId, parameters.StatusId, parameters.AvailableTaskChecklistItems);
                if (!string.IsNullOrEmpty(validationError))
                {
                    parameters.Success = false;
                    parameters.ResponseMessage = validationError;
                    await LogTrackerAPICallAsync(trackerAPIResponseModel, trackerAPILog);

                    return Json(new { result = false, message = validationError });
                }

                notes += "<br>" + await BuildChecklistNotesAsync(parameters.AvailableTaskChecklistItems);
            }

            if (existingTask.StatusId != parameters.StatusId)
            {
                var taskChangeLog = new TaskChangeLog
                {
                    EmployeeId = parameters.EmployeeId,
                    TaskId = parameters.TaskId,
                    StatusId = parameters.StatusId,
                    AssignedTo = existingTask.AssignedTo,
                    LogTypeId = (int)LogTypeEnum.StatusChange,
                    Notes = notes,
                    CreatedOn = DateTime.UtcNow
                };
                await _trackerAPIService.InsertTaskChangeLogAsync(taskChangeLog);
            }

            if (!string.IsNullOrWhiteSpace(parameters.Comments))
            {
                var taskComments = new TaskComments
                {
                    EmployeeId = parameters.EmployeeId,
                    TaskId = parameters.TaskId,
                    StatusId = parameters.StatusId,
                    Description = parameters.Comments,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
                };
                await _trackerAPIService.InsertTaskCommentsAsync(taskComments);

                await _workflowMessageService.SendEmployeeMentionMessageAsync(
                    (await _workContext.GetWorkingLanguageAsync()).Id,
                    taskComments.EmployeeId, taskComments.TaskId, taskComments.Description);
            }

            if (parameters.AvailableTaskChecklistItems?.Any() == true)
            {
                var taskCheckListEntry = new TaskCheckListEntry
                {
                    TaskId = parameters.TaskId,
                    StatusId = parameters.StatusId,
                    CheckListJson = JsonConvert.SerializeObject(parameters.AvailableTaskChecklistItems),
                    CheckedBy = parameters.EmployeeId,
                    CreatedOn = DateTime.UtcNow,
                };
                await _taskCheckListEntryService.InsertEntryAsync(taskCheckListEntry);
            }

            var existingProject = await _trackerAPIService.GetProjectByIdAsync(existingTask.ProjectId);
            int employeeId = await GetEmployeedIdBasedOnStatusAsync(newStatus.StatusName, existingTask, existingProject);
            if (employeeId > 0)
                existingTask.AssignedTo = employeeId;
            existingTask.StatusId = parameters.StatusId;
            await _trackerAPIService.UpdateProjectTaskAsync(existingTask);

            parameters.Success = true;
            parameters.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.TrackerAPI.Common.TaskStatusChangedSuccessfully");
            return Json(new { result = true, message = parameters.ResponseMessage });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #endregion

    #region Auto Tag Comment Methods

    #region Auto Tag Comment Utilities

    private async Task<string> GetProcessRuleCommentAsync(int previousStatusId, int currentStatusId)
    {
        var comment = await _trackerAPIService.GetProcessRuleCommentByPreviousAndCurrentStatusIdAsync(previousStatusId, currentStatusId);
        return string.IsNullOrWhiteSpace(comment) ? string.Empty : Regex.Replace(comment, "<.*?>", string.Empty);
    }

    private async Task<string> GetEmployeeNameForStatusAsync(string status, ProjectTask task, Project project)
    {
        int employeeId = await GetEmployeedIdBasedOnStatusAsync(status, task, project);

        if (employeeId > 0)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            return $"{employee.FirstName} {employee.LastName}";
        }

        return string.Empty;
    }

    private async Task<List<TaskStatusChecklistsRootObject>> GetTaskStatusChecklistsAsync(int taskCategoryId, int statusId)
    {
        var checklistMappings = await _checkListMappingService.GetAllCheckListMappingsAsync(taskCategoryId, statusId);
        var checklists = new List<TaskStatusChecklistsRootObject>();

        foreach (var mapping in checklistMappings)
        {
            var checklist = await _checkListMasterService.GetCheckListByIdAsync(mapping.CheckListId);
            if (checklist == null) continue;

            checklists.Add(new TaskStatusChecklistsRootObject
            {
                Text = checklist.Title,
                Value = checklist.Id,
                Selected = mapping.IsMandatory
            });
        }

        if (checklistMappings.Any() && _projectTaskSetting.IsShowSelctAllCheckList)
        {
            checklists.Insert(0, new TaskStatusChecklistsRootObject
            {
                Text = "Select All",
                Value = 0
            });
        }

        return checklists;
    }

    #endregion

    #region Auto Tag Comment API Method

    [HttpPost]
    [Route("/api/auto_tag", Name = nameof(EmployeeAutoTag))]
    public virtual async Task<IActionResult> EmployeeAutoTag([FromBody] EmployeeAutoTagParametersModel parameters)
    {
        try
        {
            var trackerAPILog = new TrackerAPILog
            {
                RequestJson = JsonConvert.SerializeObject(parameters),
                EndPoint = TrackerAPIDefaults.EmployeeAutoTagEndPoint
            };

            var trackerAPIResponseModel = await CheckIfAuthenticated(new TrackerAPIResponseModel());
            if (!trackerAPIResponseModel.Success)
                return await HandleTrackerAPIFailureAsync(trackerAPIResponseModel, trackerAPILog);

            if (parameters.TaskId <= 0)
                return await HandleTrackerAPIErrorAsync(trackerAPIResponseModel, trackerAPILog, "Satyanam.Plugin.Misc.TrackerAPI.Common.TaskId");

            var existingTask = await _trackerAPIService.GetProjectTaskByIdAsync(parameters.TaskId);
            if (existingTask == null || await _trackerAPIService.GetProjectByIdAsync(existingTask.ProjectId) is not { } existingProject)
            {
                parameters.Success = false;
                await LogTrackerAPICallAsync(parameters, trackerAPILog);
                return Json(new
                {
                    result = false,
                    message = parameters.ResponseMessage
                });
            }

            var processRuleComment = await GetProcessRuleCommentAsync(existingTask.StatusId, parameters.StatusId);
            var employeeName = await GetEmployeeNameForStatusAsync(parameters.Status, existingTask, existingProject);
            var taskStatusChecklists = await GetTaskStatusChecklistsAsync(existingTask.TaskCategoryId, parameters.StatusId);

            return Json(new
            {
                result = true,
                employee_name = employeeName,
                comment = processRuleComment,
                task_status_checklists = taskStatusChecklists
            });
        }
        catch (Exception ex)
        {
            return await HandleTrackerAPIExceptionAsync(ex);
        }
    }

    #endregion

    #endregion
}
