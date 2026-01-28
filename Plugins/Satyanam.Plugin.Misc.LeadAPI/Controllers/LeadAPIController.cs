using App.Core;
using App.Services.Authentication;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.LeadAPI.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.LeadAPI.Controllers;

public partial class LeadAPIController : BasePluginController
{
    #region Fields

    protected readonly IAuthenticationService _authenticationService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly ILeadAPIService _leadAPIService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly LeadAPISettings _leadAPISettings;
    protected readonly ILeadService _leadService;
    #endregion

    #region Ctor

    public LeadAPIController(IAuthenticationService authenticationService,
        ILocalizationService localizationService,
        ILogger logger,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LeadAPISettings leadAPISettings,
        ILeadAPIService leadAPIService,
        ILeadService leadService)
    {
        _authenticationService = authenticationService;
        _localizationService = localizationService;
        _logger = logger;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _leadAPISettings = leadAPISettings;
        _leadAPIService = leadAPIService;
        _leadService = leadService;
    }

    #endregion

    #region Utilities

    protected virtual async Task<LeadAPIResponseModel> CheckIfAuthenticated(LeadAPIResponseModel leadAPIResponseModel, bool isLoginAPI = false)
    {
        bool ValidateApiKeyAndSecret(string apiKey, string apiSecret)
        {
            return apiKey != null && apiSecret != null && apiKey == _leadAPISettings.APIKey && apiSecret == _leadAPISettings.APISecretKey;
        }

        string unauthorizedMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.UnauthorizedAccess");

        if (Request.Headers.TryGetValue(LeadAPIDefaults.APIKeyHeader, out var apiKeyValues) &&
            Request.Headers.TryGetValue(LeadAPIDefaults.APISecretKeyHeader, out var apiSecretValues))
        {
            var apiKey = apiKeyValues.FirstOrDefault();
            var apiSecret = apiSecretValues.FirstOrDefault();

            leadAPIResponseModel.Success = ValidateApiKeyAndSecret(apiKey, apiSecret);
            leadAPIResponseModel.ResponseMessage = leadAPIResponseModel.Success ? null : unauthorizedMessage;

            return leadAPIResponseModel;
        }

        leadAPIResponseModel.Success = false;
        leadAPIResponseModel.ResponseMessage = unauthorizedMessage;
        return leadAPIResponseModel;
    }
    protected virtual async Task<string> GetRequestBodyAsync()
    {
        try
        {
            Request.EnableBuffering();
            Request.Body.Position = 0;

            using var reader = new StreamReader(Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            Request.Body.Position = 0;
            return body;
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Error reading request body", ex);
            return string.Empty;
        }
    }



    protected virtual async Task LogLeadAPICallAsync<TModel>(TModel responseModel, LeadAPILog leadAPILog, object requestModel = null)
    {
        ArgumentNullException.ThrowIfNull(nameof(responseModel));
        _leadAPISettings.EnableLogging = true;
        if (_leadAPISettings.EnableLogging)
        {
            leadAPILog.Success = (bool)typeof(TModel).GetProperty("Success")?.GetValue(responseModel, null);
            leadAPILog.ResponseMessage = typeof(TModel).GetProperty("ResponseMessage")?.GetValue(responseModel, null)?.ToString();
            leadAPILog.ResponseJson = JsonConvert.SerializeObject(responseModel);

            if (requestModel != null)
                leadAPILog.RequestJson = JsonConvert.SerializeObject(requestModel);
            leadAPILog.EndPoint = $"{Request.Method} {Request.Path}";
            leadAPILog.CreatedOnUtc = DateTime.UtcNow;

            await _leadAPIService.InsertLeadAPILogAsync(leadAPILog);
        }
    }

    #endregion

    #region Lead Method
    [HttpPost]

    public virtual async Task<IActionResult> CreateLead([FromBody] LeadAPIModel model)
    {
        var leadAPIResponseModel = new LeadAPIResponseModel();
        var leadAPILogs = new LeadAPILog();

        try
        {
            leadAPIResponseModel = await CheckIfAuthenticated(leadAPIResponseModel);

            if (!leadAPIResponseModel.Success)
            {
                leadAPIResponseModel.ResponseMessage = await _localizationService
                    .GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.UnauthorizedAccess");

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            if (string.IsNullOrWhiteSpace(_leadAPISettings.APIKey) || string.IsNullOrWhiteSpace(_leadAPISettings.APISecretKey))
            {
                leadAPIResponseModel.Success = false;
                leadAPIResponseModel.ResponseMessage = "Invalid API Key";

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);
                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            if (string.IsNullOrWhiteSpace(model.Name) ||
                string.IsNullOrWhiteSpace(model.CompanyName) ||
                string.IsNullOrWhiteSpace(model.Email))
            {
                leadAPIResponseModel.Success = false;
                leadAPIResponseModel.ResponseMessage = await _localizationService
                    .GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.MissingRequiredFields");

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);
                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            string firstName = model.Name;
            string lastName = "";

            if (!string.IsNullOrWhiteSpace(model.Name))
            {
                var nameParts = model.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                firstName = nameParts[0];
                if (nameParts.Length > 1)
                    lastName = nameParts[1];
            }

            var lead = new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                CompanyName = model.CompanyName,
                Email = model.Email,
                Phone = model.MobileNo,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _leadService.InsertLeadAsync(lead);

            leadAPIResponseModel.Success = true;
            leadAPIResponseModel.ResponseMessage = "Lead created successfully";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs,model);

            return Json(new { result = true, message = leadAPIResponseModel.ResponseMessage });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Error in CreateLead API: " + ex.Message, ex);

            leadAPIResponseModel.Success = false;
            leadAPIResponseModel.ResponseMessage = "An error occurred while processing your request.";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

            return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
        }
    }


    #endregion
}
