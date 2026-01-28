using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectIntegrations;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Core.Events;
using App.Services.Configuration;
using App.Services.Employees;
using App.Services.Events;
using App.Services.Logging;
using App.Services.ProjectIntegrations;
using App.Services.Projects;
using App.Services.ProjectTasks;
using Azure.Core;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.Identity.Client;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.AzureIntegration.Domain;
using Satyanam.Plugin.Misc.AzureIntegration.Models.AzureResponse;
using Satyanam.Plugin.Misc.AzureIntegration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AzureIntegration.Events;

public partial class AzureIntegrationEvent : IConsumer<EntityInsertedEvent<ProjectTask>>,
    IConsumer<EntityUpdatedEvent<ProjectTask>>, IConsumer<EntityInsertedEvent<TaskComments>>
{
    #region Fields

    protected readonly HttpClient _httpClient;
    protected readonly IAzureIntegrationService _azureIntegrationService;
    protected readonly IEmployeeService _employeeService;
    protected readonly ILogger _logger;
    protected readonly IProjectIntegrationService _projectIntegrationService;
    protected readonly IProjectsService _projectsService;
    protected readonly IProjectTaskService _projectTaskService;
	protected readonly ISettingService _settingService;
    protected readonly IWorkflowStatusService _workflowStatusService;
    protected readonly IFollowUpTaskService _followUpTaskService;

    #endregion

    #region Ctor

    public AzureIntegrationEvent(HttpClient httpClient,
        IAzureIntegrationService azureIntegrationService,
        IEmployeeService employeeService,
        ILogger logger,
        IProjectIntegrationService projectIntegrationService,
        IProjectsService projectsService,
        IProjectTaskService projectTaskService,
        ISettingService settingService,
        IWorkflowStatusService workflowStatusService,
        IFollowUpTaskService followUpTaskService)
    {
        _httpClient = httpClient;
        _azureIntegrationService = azureIntegrationService;
        _employeeService = employeeService;
        _logger = logger;
        _projectIntegrationService = projectIntegrationService;
        _projectsService = projectsService;
        _projectTaskService = projectTaskService;
        _settingService = settingService;
        _workflowStatusService = workflowStatusService;
        _followUpTaskService = followUpTaskService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<(string, string, string)> GenerateAccessTokenAsync(int projectId = 0)
    {
        var projectIntegrationSettings = await _projectIntegrationService.GetProjectIntegrationSettingsByProjectIdAsync(projectId);
        if (!projectIntegrationSettings.Any())
            return (string.Empty, string.Empty, string.Empty);

        string organizationName = string.Empty; string projectName = string.Empty; string clientId = string.Empty; 
        string clientSecret = string.Empty; string tenantId = string.Empty; string userId = string.Empty; string accessToken = string.Empty;
        foreach (var projectIntegrationSetting in projectIntegrationSettings)
        {
            if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzureOrganizationName)
                organizationName = projectIntegrationSetting.KeyValue;
            else if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzureProjectName)
                projectName = projectIntegrationSetting.KeyValue;
            else if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzureClientId)
                clientId = projectIntegrationSetting.KeyValue;
            else if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzureClientSecret)
                clientSecret = projectIntegrationSetting.KeyValue;
            else if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzureTenantId)
                tenantId = projectIntegrationSetting.KeyValue;
            else if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzureUserId)
                userId = projectIntegrationSetting.KeyValue;
            else if (projectIntegrationSetting.KeyName == ProjectIntegrationDefaults.AzurePersonalAccessToken)
                accessToken = projectIntegrationSetting.KeyValue;
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            var authority = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";

            var app = ConfidentialClientApplicationBuilder.Create(clientId).WithClientSecret(clientSecret)
                .WithAuthority(authority).Build();

            var scopes = new[] { AzureIntegrationDefaults.AzureSyncAPIScope };

            accessToken = (await app.AcquireTokenForClient(scopes).ExecuteAsync()).AccessToken;
        }

        return (accessToken, organizationName, projectName);
    }

    protected virtual async Task<string?> GetDescriptorByEmailAsync(string email, string token, string organization)
    {
        var request = new HttpRequestMessage(HttpMethod.Get,
            $"https://vssps.dev.azure.com/{organization}/_apis/graph/users?api-version=7.1-preview.1");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        foreach (var user in doc.RootElement.GetProperty("value").EnumerateArray())
        {
            if (user.TryGetProperty("mailAddress", out var mailProp) && mailProp.GetString()?.Equals(email, StringComparison.OrdinalIgnoreCase) == true)
                return user.GetProperty("descriptor").GetString();
        }

        return string.Empty;
    }

    protected virtual async Task InsertLogAsync(int taskId, string endpoint, string message)
    {
        var azureSyncLog = new AzureSyncLog()
        {
            TaskId = taskId,
            APIEndPoint = endpoint,
            Exception = message,
            CreatedOnUtc = DateTime.UtcNow
        };
        await _azureIntegrationService.InsertIAzureSyncLogAsync(azureSyncLog);
    }

    protected virtual async Task<List<object>> PreparePatchDocumentAsync(ProjectTask projectTask, Employee employee, 
        string safeInitialState, decimal estimationTime, string organizationName, string projectName)
    {
        var patchDocument = new List<object>
        {
            new { op = "add", path = "/fields/System.Title", value = projectTask.TaskTitle },
            new { op = "add", path = "/fields/System.Description", value = projectTask.Description },
            new { op = "add", path = "/fields/System.AssignedTo", value = employee.OfficialEmail },
            new { op = "add", path = "/fields/System.State", value = safeInitialState },
            new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.OriginalEstimate", value = estimationTime }
        };

        if (projectTask.Tasktypeid == (int)TaskTypeEnum.Bug)
            patchDocument.Add(new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = projectTask.Description });

        if (projectTask.SpentHours > 0 || projectTask.SpentMinutes > 0 || projectTask.WorkItemId > 0)
        {
            decimal completedWork = projectTask.SpentHours + (projectTask.SpentMinutes / 60.0m);
            decimal remainingWork = Math.Max(estimationTime - completedWork, 0);

            patchDocument.Add(new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.RemainingWork", value = Math.Round(remainingWork, 2) });
            patchDocument.Add(new { op = "add", path = "/fields/Microsoft.VSTS.Scheduling.CompletedWork", value = Math.Round(completedWork, 2) });
        }

        if (projectTask.ParentTaskId > 0)
        {
            var existingParentTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(projectTask.ParentTaskId);
            if (existingParentTask?.WorkItemId > 0)
            {
                var parentWorkItemUrl = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workItems/{existingParentTask.WorkItemId}";

                patchDocument.Add(new
                {
                    op = "add",
                    path = "/relations/-",
                    value = new
                    {
                        rel = "System.LinkTypes.Hierarchy-Reverse",
                        url = parentWorkItemUrl,
                        attributes = new { comment = string.Empty }
                    }
                });
            }
        }

        return patchDocument;
    }

    protected virtual async Task<int?> GetParentRelationIndexAsync(string workItemUrl, string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.GetAsync(workItemUrl);
        response.EnsureSuccessStatusCode();

        var workItemJson = await response.Content.ReadAsStringAsync();
        dynamic workItem = Newtonsoft.Json.JsonConvert.DeserializeObject(workItemJson);

        for (int i = 0; i < workItem.relations?.Count; i++)
        {
            var relation = workItem.relations[i];
            if ((string)relation.rel == "System.LinkTypes.Hierarchy-Reverse")
                return i;
        }

        return null;
    }

    protected virtual async Task<(string formattedText, List<object> mentions)> FormatRichMentionsAsync(string rawComment, 
        string accessToken, string organizationName)
    {
        var mentionMatches = Regex.Matches(rawComment, @"@<([^>]+)>");
        var nameToEmail = new Dictionary<string, string>();
        var mentionsList = new List<object>();

        foreach (Match match in mentionMatches)
        {
            string localName = match.Groups[1].Value;

            var employee = await _employeeService.GetEmployeeByEmployeeName(localName);

            nameToEmail[localName] = employee.OfficialEmail;

            if (nameToEmail.TryGetValue(localName, out string email))
            {
                string? descriptor = await GetDescriptorByEmailAsync(email, accessToken, organizationName);
                if (!string.IsNullOrWhiteSpace(descriptor))
                {
                    string mentionTag = $"<v id=\"{descriptor}\">@{localName}</v>";
                    rawComment = rawComment.Replace(match.Value, mentionTag);

                    mentionsList.Add(new
                    {
                        id = descriptor,
                        name = localName,
                        url = $"https://vssps.dev.azure.com/{organizationName}/_apis/Identities/{descriptor}",
                        descriptor = descriptor
                    });
                }
            }
        }

        return (rawComment, mentionsList);
    }

    protected virtual async Task InsertTaskIntoAzureAsync(ProjectTask projectTask, Project project, Employee employee, WorkflowStatus workflowStatus,
        string accessToken, string organizationName, string projectName)
    {
        string taskType = Enum.GetName(typeof(TaskTypeEnum), projectTask.Tasktypeid);
        if (taskType == AzureIntegrationDefaults.UserStoryWithoutSpace)
            taskType = AzureIntegrationDefaults.UserStoryWithSpace;
        if (projectTask.Tasktypeid == (int)TaskTypeEnum.ChangeRequest)
            taskType = Enum.GetName(typeof(TaskTypeEnum), (int)TaskTypeEnum.Task);

        string url = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workitems/${taskType}?api-version=7.1-preview.3";

        decimal estimationTime = Math.Round(projectTask.EstimatedTime, 2);
        string safeInitialState = string.Empty;
        if (workflowStatus.StatusName != AzureIntegrationDefaults.New)
            safeInitialState = AzureIntegrationDefaults.New;
        else
            safeInitialState = AzureIntegrationDefaults.New;

        var patchDocument = await PreparePatchDocumentAsync(projectTask, employee, safeInitialState, estimationTime, organizationName, 
            projectName);

        var content = new StringContent(JsonSerializer.Serialize(patchDocument));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.PostAsync(url, content);
        string result = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var workItem = JsonSerializer.Deserialize<WorkItem>(result, options);

            projectTask.WorkItemId = workItem?.Id ?? 0;

            if (workflowStatus.StatusName != AzureIntegrationDefaults.New)
            {
                var transitionPatch = new List<object>
                {
                    new { op = "replace", path = "/fields/System.State", value = workflowStatus.StatusName }
                };

                var transitionContent = new StringContent(JsonSerializer.Serialize(transitionPatch));
                transitionContent.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

                var transitionUrl = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workitems/{projectTask.WorkItemId}?api-version=7.1-preview.3";
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                response = await _httpClient.PatchAsync(transitionUrl, transitionContent);
            }

            var taskComments = await _azureIntegrationService.GetAllTaskCommentsByTaskIdAsync(projectTask.Id);

            foreach (var taskComment in taskComments.OrderBy(tc => tc.Id))
                await AddCommentsToWorkItemAsync(projectTask.WorkItemId, projectTask.Id, taskComment.Description, accessToken, organizationName, projectName);

            await _projectTaskService.UpdateProjectTaskAsync(projectTask);
        }
        else
            await InsertLogAsync(projectTask.Id, AzureIntegrationDefaults.ProjectTaskInsertAPI, result);
    }

    protected virtual async Task UpdateTaskIntoAzureAsync(ProjectTask projectTask, Project project, Employee employee, WorkflowStatus workflowStatus,
        string accessToken, string organizationName, string projectName)
    {
        string workItemUrl = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workitems/{projectTask.WorkItemId}?api-version=7.1-preview.3&$expand=relations";
        string updateUrl = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workitems/{projectTask.WorkItemId}?api-version=7.1-preview.3";

        decimal estimationTime = Math.Round(projectTask.EstimatedTime, 2);
        var patchDocument = await PreparePatchDocumentAsync(projectTask, employee, workflowStatus.StatusName, estimationTime, organizationName, 
            projectName);

        int? parentRelationIndex = await GetParentRelationIndexAsync(workItemUrl, accessToken);
        if (parentRelationIndex.HasValue)
            patchDocument.Add(new { op = "remove", path = $"/relations/{parentRelationIndex.Value}" });

        var content = new StringContent(JsonSerializer.Serialize(patchDocument));
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _httpClient.PatchAsync(updateUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            await InsertLogAsync(projectTask.Id, AzureIntegrationDefaults.ProjectTaskUpdateAPI, result);
        }
    }

    protected virtual async Task AddCommentsToWorkItemAsync(int workItemId, int taskId, string commentText, string accessToken,
        string organizationName, string projectName)
    {
        if (string.IsNullOrWhiteSpace(commentText))
            return;

        try
        {
            var (formattedText, mentions) = await FormatRichMentionsAsync(commentText, accessToken, organizationName);

            var commentUrl = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workItems/{workItemId}/comments?api-version=7.1-preview.3";

            var commentPayload = new
            {
                text = formattedText,
                mentions = mentions
            };

            var content = new StringContent(JsonSerializer.Serialize(commentPayload));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PostAsync(commentUrl, content);
            
            string result = string.Empty;
            if (response.IsSuccessStatusCode)
                result = await response.Content.ReadAsStringAsync();
            else
            {
                result = await response.Content.ReadAsStringAsync();
                await InsertLogAsync(taskId, AzureIntegrationDefaults.TaskCommentInsertAPI, result);
            }
        }
        catch (Exception ex)
        {
            await InsertLogAsync(taskId, AzureIntegrationDefaults.TaskCommentInsertAPI, ex.Message);
        }
    }

    #endregion

    #region Project Task Insert Event Methods

    public async Task HandleEventAsync(EntityInsertedEvent<ProjectTask> projectTaskInsertedEvent)
	{
		try
		{
            if (projectTaskInsertedEvent.Entity is ProjectTask projectTask)
            {
                await _followUpTaskService.InsertFollowupTaskByTask(projectTask);

                if (!projectTask.IsSync)
                    return;

                var project = await _projectsService.GetProjectsByIdAsync(projectTask.ProjectId);

                if (project == null)
                    return;

                var employee = await _employeeService.GetEmployeeByIdAsync(projectTask.AssignedTo);

                if (employee == null)
                    return;

                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(projectTask.StatusId);

                if (workflowStatus == null)
                    return;

                var (accessToken, organizationName, projectName) = await GenerateAccessTokenAsync(projectTask.ProjectId);

                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                await InsertTaskIntoAzureAsync(projectTask, project, employee, workflowStatus, accessToken, organizationName, projectName);

              

            }
        }
		catch (Exception exception)
		{
			await _logger.ErrorAsync(exception.Message, exception);
            await InsertLogAsync(projectTaskInsertedEvent.Entity.Id, AzureIntegrationDefaults.ProjectTaskInsertAPI, exception.Message);
        }
    }

    #endregion

    #region Project Task Update Event Methods

    public virtual async Task<string> GetWorkItemTypeAsync(int workItemId, string accessToken, string organizationName, string projectName)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var getUrl = $"https://dev.azure.com/{organizationName}/{projectName}/_apis/wit/workitems/{workItemId}?$expand=all&api-version=7.1-preview.3";
        var response = await _httpClient.GetAsync(getUrl);
        var sourceResult = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            var sourceItem = JsonDocument.Parse(sourceResult).RootElement;
            var fields = sourceItem.GetProperty("fields");

            if (fields.TryGetProperty("System.WorkItemType", out var workItemTypeElement))
            {
                var workItemType = workItemTypeElement.GetString();
                return workItemType;
            }
        }

        return string.Empty;
    }

    public async Task HandleEventAsync(EntityUpdatedEvent<ProjectTask> projectTaskUpdatedEvent)
    {
        try
        {
            if (projectTaskUpdatedEvent.Entity is ProjectTask projectTask)
            {
                if (!projectTask.IsSync)
                    return;

                var employee = await _employeeService.GetEmployeeByIdAsync(projectTask.AssignedTo);

                if (employee == null)
                    return;

                var workflowStatus = await _workflowStatusService.GetWorkflowStatusByIdAsync(projectTask.StatusId);

                if (workflowStatus == null)
                    return;

                var project = await _projectsService.GetProjectsByIdAsync(projectTask.ProjectId);

                if (project == null)
                    return;

                var (accessToken, organizationName, projectName) = await GenerateAccessTokenAsync(projectTask.ProjectId);

                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                if (projectTask.IsSync && projectTask.WorkItemId == 0)
                    await InsertTaskIntoAzureAsync(projectTask, project, employee, workflowStatus, accessToken, organizationName, projectName);

                string workItemType = string.Empty;
                if (projectTask.WorkItemId != 0)
                    workItemType = await GetWorkItemTypeAsync(projectTask.WorkItemId, accessToken, organizationName, projectName);

                string taskType = Enum.GetName(typeof(TaskTypeEnum), projectTask.Tasktypeid);
                if (taskType == AzureIntegrationDefaults.UserStoryWithoutSpace)
                    taskType = AzureIntegrationDefaults.UserStoryWithSpace;
                if (projectTask.Tasktypeid == (int)TaskTypeEnum.ChangeRequest)
                    taskType = Enum.GetName(typeof(TaskTypeEnum), (int)TaskTypeEnum.Task);
                if (workItemType != taskType && !string.IsNullOrWhiteSpace(workItemType))
                    await InsertTaskIntoAzureAsync(projectTask, project, employee, workflowStatus, accessToken, organizationName, projectName);

                await UpdateTaskIntoAzureAsync(projectTask, project, employee, workflowStatus, accessToken, organizationName, projectName);
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception);
            await InsertLogAsync(projectTaskUpdatedEvent.Entity.Id, AzureIntegrationDefaults.ProjectTaskUpdateAPI, exception.Message);
        }
    }

    #endregion

    #region Project Task Comment Event Methods

    public async Task HandleEventAsync(EntityInsertedEvent<TaskComments> taskCommentsInsertedEvent)
    {
        try
        {
            if (taskCommentsInsertedEvent.Entity is TaskComments taskComments)
            {
                var projectTask = await _projectTaskService.GetProjectTasksWithoutCacheByIdAsync(taskComments.TaskId);

                if (projectTask == null)
                    return;

                if (!projectTask.IsSync)
                    return;

                if (projectTask.WorkItemId == 0)
                    return;

                var (accessToken, organizationName, projectName) = await GenerateAccessTokenAsync(projectTask.ProjectId);

                if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(organizationName) || string.IsNullOrWhiteSpace(projectName))
                    return;

                await AddCommentsToWorkItemAsync(projectTask.WorkItemId, projectTask.Id, taskComments.Description, accessToken, organizationName, projectName);
            }
        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync(exception.Message, exception);
            await InsertLogAsync(taskCommentsInsertedEvent.Entity.TaskId, AzureIntegrationDefaults.TaskCommentInsertAPI, exception.Message);
        }
    }

    #endregion
}
