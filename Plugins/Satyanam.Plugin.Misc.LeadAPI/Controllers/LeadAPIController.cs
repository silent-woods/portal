using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Common;
using App.Services.Authentication;
using App.Services.Common;
using App.Services.Directory;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Web.Framework.Controllers;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Plugin.Misc.LeadAPI.Models;

namespace Satyanam.Plugin.Misc.LeadAPI.Controllers;

public partial class LeadAPIController : BaseController
{
    #region Fields

    protected readonly IAddressService _addressService;
    protected readonly IAuthenticationService _authenticationService;
    protected readonly ICountryService _countryService;
    protected readonly ILeadAPIService _leadAPIService;
    protected readonly ILeadService _leadService;
    protected readonly ILeadStatusService _leadStatusService;
    protected readonly ILocalizationService _localizationService;
    protected readonly ILogger _logger;
    protected readonly IStateProvinceService _stateProvinceService;
    protected readonly ITagsService _tagsService;
    protected readonly IWorkContext _workContext;
    protected readonly IWorkflowMessageService _workflowMessageService;
    protected readonly LeadAPISettings _leadAPISettings;

    #endregion

    #region Ctor

    public LeadAPIController(IAddressService addressService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        ILeadAPIService leadAPIService,
        ILeadService leadService,
        ILeadStatusService leadStatusService,
        ILocalizationService localizationService,
        ILogger logger,
        IStateProvinceService stateProvinceService,
        ITagsService tagsService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LeadAPISettings leadAPISettings)
    {
        _addressService = addressService;
        _authenticationService = authenticationService;
        _countryService = countryService;
        _leadAPIService = leadAPIService;
        _leadService = leadService;
        _leadStatusService = leadStatusService;
        _localizationService = localizationService;
        _logger = logger;
        _stateProvinceService = stateProvinceService;
        _tagsService = tagsService;
        _workContext = workContext;
        _workflowMessageService = workflowMessageService;
        _leadAPISettings = leadAPISettings;
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
                leadAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.UnauthorizedAccess");
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
                string.IsNullOrWhiteSpace(model.CompanyName))
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

            var existingLead = await _leadService.GetExistingLeadByEmailAndFirstNameAndLastNameAsync(firstName, lastName, model.Email);
            if (existingLead != null)
            {
                leadAPIResponseModel.Success = false;
                leadAPIResponseModel.ResponseMessage = await _localizationService
                    .GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.AlreadyExists");

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);
                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            var existingLeadSource = await _leadStatusService.GetLeadStatusByNameAsync(leadStatusName: LeadAPIDefaults.NewLeadStatus);

            var lead = new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                CompanyName = model.CompanyName,
                Email = model.Email,
                Phone = model.MobileNo,
                Description = model.Summary,
                LinkedinUrl = model.LinkedInUrl,
                EmailOptOut = true,
                CreatedOnUtc = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(model.Address1) && !string.IsNullOrEmpty(model.Address2) && !string.IsNullOrEmpty(model.ZipCode) && !string.IsNullOrEmpty(model.City) && !string.IsNullOrEmpty(model.State) &&
                !string.IsNullOrEmpty(model.Country))
            {
                var existingCountry = await _countryService.GetCountryByNameAsync(model.Country);
                var existingStateProvince = await _stateProvinceService.GetStateProvinceByNameAsync(model.State);

                if (existingCountry != null && existingStateProvince != null)
                {
                    var address = new Address
                    {
                        Address1 = model.Address1,
                        Address2 = model.Address2,
                        City = model.City,
                        StateProvinceId = existingStateProvince.Id,
                        CountryId = existingCountry.Id,
                        ZipPostalCode = model.ZipCode,
                    };
                    await _addressService.InsertAddressAsync(address);
                    lead.AddressId = address.Id;
                }
            }

            if (existingLeadSource != null)
                lead.LeadSourceId = existingLeadSource.Id;

            await _leadService.InsertLeadAsync(lead);

            leadAPIResponseModel.Success = true;
            leadAPIResponseModel.ResponseMessage = "Lead created successfully";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

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

    [HttpGet]
    public virtual async Task<IActionResult> GetAllAvailableTags()
    {
        var leadAPIResponseModel = new LeadAPIResponseModel();
        var leadAPILogs = new LeadAPILog();

        try
        {
            leadAPIResponseModel = await CheckIfAuthenticated(leadAPIResponseModel);

            if (!leadAPIResponseModel.Success)
            {
                leadAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.UnauthorizedAccess");
                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, null);

                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            if (string.IsNullOrWhiteSpace(_leadAPISettings.APIKey) || string.IsNullOrWhiteSpace(_leadAPISettings.APISecretKey))
            {
                leadAPIResponseModel.Success = false;
                leadAPIResponseModel.ResponseMessage = "Invalid API Key";

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, null);
                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            var model = new List<TagsModel>();
            var existingAvailableTags = (await _tagsService.GetAllTagsAsync(name: string.Empty)).ToList();
            foreach (var existingAvailableTag in existingAvailableTags)
            {
                var tagsModel = new TagsModel()
                {
                    Id = existingAvailableTag.Id,
                    Name = existingAvailableTag.Name,
                };
                model.Add(tagsModel);
            }

            leadAPIResponseModel.Success = true;
            leadAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

            return Json(new
            {
                result = true,
                tags = model
            });

        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Error in GetAllAvailbleTags API: " + exception.Message, exception);

            leadAPIResponseModel.Success = false;
            leadAPIResponseModel.ResponseMessage = "An error occurred while processing your request.";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, null);

            return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
        }
    }


    #endregion
}
