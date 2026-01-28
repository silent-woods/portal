//using App.Core.Domain.Employees;
//using App.Core.Domain.Extension.ProjectIntegrations;
//using App.Core.Domain.Extension.ProjectTasks;
//using App.Core.Domain.Projects;
//using App.Core.Domain.ProjectTasks;
//using App.Core.Domain.TimeSheets;
//using App.Core.Events;
//using App.Services.Configuration;
//using App.Services.Employees;
//using App.Services.Events;
//using App.Services.Logging;
//using App.Services.ProjectIntegrations;
//using App.Services.Projects;
//using App.Services.ProjectTasks;
//using App.Services.TimeSheets;
//using AutoMapper.Configuration.Annotations;
//using Microsoft.Identity.Client;
//using Satyanam.Nop.Core.Domains;
//using Satyanam.Nop.Core.Services;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text.Json;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;

//namespace Satyanam.Plugin.Misc.AzureIntegration.Events;

//public partial class TimesheetEvent : IConsumer<EntityInsertedEvent<TimeSheet>>, IConsumer<EntityUpdatedEvent<TimeSheet>>


//{
//    #region Fields

//    protected readonly HttpClient _httpClient;
   
//    protected readonly IEmployeeService _employeeService;
//    protected readonly ILogger _logger;
//    protected readonly IProjectIntegrationService _projectIntegrationService;
//    protected readonly IProjectsService _projectsService;
//    protected readonly IProjectTaskService _projectTaskService;
//	protected readonly ISettingService _settingService;
//    protected readonly IWorkflowStatusService _workflowStatusService;
//    protected readonly ITimeSheetsService _timeSheetsService;
//    #endregion

//    #region Ctor

//    public TimesheetEvent(HttpClient httpClient,

//        IEmployeeService employeeService,
//        ILogger logger,
//        IProjectIntegrationService projectIntegrationService,
//        IProjectsService projectsService,
//        IProjectTaskService projectTaskService,
//        ISettingService settingService,
//        IWorkflowStatusService workflowStatusService,
//        ITimeSheetsService timeSheetsService)
//    {
//        _httpClient = httpClient;

//        _employeeService = employeeService;
//        _logger = logger;
//        _projectIntegrationService = projectIntegrationService;
//        _projectsService = projectsService;
//        _projectTaskService = projectTaskService;
//        _settingService = settingService;
//        _workflowStatusService = workflowStatusService;
//        _timeSheetsService = timeSheetsService;
//    }

//    #endregion

//    #region Utilities




//    #endregion

//    #region Timesheet Insert Event Methods

//    public async Task HandleEventAsync(EntityInsertedEvent<TimeSheet> timesheetInsertedEvent)
//	{
//		try
//		{
//            if (timesheetInsertedEvent.Entity is TimeSheet timeSheet)
//            {
               
                
//                var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(timeSheet.TaskId);
//                if (projectTask.Tasktypeid == 3 && projectTask.ParentTaskId != 0) {
//                    var parentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(projectTask.ParentTaskId);
//                    var workQuality = await _projectTaskService.CalculateWorkQualityAsync(projectTask.ParentTaskId);
//                    if (workQuality != null)
//                    {
//                        parentTask.WorkQuality = workQuality.Value;
//                        await _projectTaskService.UpdateProjectTaskAsync(parentTask);

//                    }
//                }


    

//            }
//        }
//		catch (Exception exception)
//		{
//			await _logger.ErrorAsync(exception.Message, exception);
          
//        }
//    }

//    #endregion

//    //#region Project Task Update Event Methods

//    public async Task HandleEventAsync(EntityUpdatedEvent<TimeSheet> projectTaskUpdatedEvent)
//    {
//        try
//        {
//            if (projectTaskUpdatedEvent.Entity is TimeSheet timeSheet)
//            {
//                var oldTimeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(timeSheet.Id);
//            }
//        }
//        catch (Exception exception)
//        {
//            await _logger.ErrorAsync(exception.Message, exception);
            
//        }
//    }

//    //#endregion

//    //#region Project Task Comment Event Methods

//    //public async Task HandleEventAsync(EntityInsertedEvent<TaskComments> taskCommentsInsertedEvent)
//    //{
//    //    try
//    //    {
//    //        if (taskCommentsInsertedEvent.Entity is TaskComments taskComments)
//    //        {
//    //            var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskComments.TaskId);

//    //            if (projectTask == null)
//    //                return;

//    //            if (!projectTask.IsSync)
//    //                return;

//    //            if (projectTask.WorkItemId == 0)
//    //                return;

//    //            var (accessToken, organizationName, projectName) = await GenerateAccessTokenAsync(projectTask.ProjectId);

//    //            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(projectName))
//    //                return;

//    //            accessToken = "6SMQI1ldC8QLTqeFjmV3kMIfaK8y4lxPHBkrcdnF0jv0lw5WkmODJQQJ99BFACAAAAANGCBqAAASAZDO1P3E";

//    //            await AddCommentsToWorkItemAsync(projectTask.WorkItemId, projectTask.Id, taskComments.Description, accessToken, organizationName, projectName);
//    //        }
//    //    }
//    //    catch (Exception exception)
//    //    {
//    //        await _logger.ErrorAsync(exception.Message, exception);
//    //        await InsertLogAsync(taskCommentsInsertedEvent.Entity.TaskId, AzureIntegrationDefaults.TaskCommentInsertAPI, exception.Message);
//    //    }
//    //}

//    //#endregion
//}
