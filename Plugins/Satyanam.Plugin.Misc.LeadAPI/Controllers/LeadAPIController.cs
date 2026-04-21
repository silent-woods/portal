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
using DocumentFormat.OpenXml.Wordprocessing;
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
    protected readonly IIndustryService _industryService;
    protected readonly ILeadAPIService _leadAPIService;
    protected readonly ILeadService _leadService;
    protected readonly ILeadStatusService _leadStatusService;
    protected readonly ILinkedInMessagesService _linkedInMessagesService;
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
        IIndustryService industryService,
        ILeadAPIService leadAPIService,
        ILeadService leadService,
        ILeadStatusService leadStatusService,
        ILinkedInMessagesService linkedInMessagesService,
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
        _industryService = industryService;
        _leadAPIService = leadAPIService;
        _leadService = leadService;
        _leadStatusService = leadStatusService;
        _linkedInMessagesService = linkedInMessagesService;
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

            var existingLead = await _leadService.GetExistingLeadByLinkedinUrlAsync(linkedinURL: model.LinkedInUrl);
            if (existingLead != null)
            {
                leadAPIResponseModel.Success = false;
                leadAPIResponseModel.ResponseMessage = await _localizationService
                    .GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.AlreadyExists");

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);
                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            var existingLeadStatus = await _leadStatusService.GetLeadStatusByNameAsync(leadStatusName: LeadAPIDefaults.NewLeadStatus);

            var lead = new Lead
            {
                FirstName = firstName,
                LastName = lastName,
                CompanyName = model.CompanyName,
                Email = model.Email,
                Phone = model.MobileNo,
                Description = model.Summary,
                LinkedinUrl = model.LinkedInUrl,
                IndustryId = model.IndustryId,
                EmailOptOut = true,
                CreatedOnUtc = DateTime.UtcNow
            };

            if (!string.IsNullOrEmpty(model.Address1) || !string.IsNullOrEmpty(model.Address2) || !string.IsNullOrEmpty(model.ZipCode) || !string.IsNullOrEmpty(model.City) ||
                !string.IsNullOrEmpty(model.State) || !string.IsNullOrEmpty(model.Country))
            {
                var existingCountry = await _countryService.GetCountryByNameAsync(model.Country);
                var existingStateProvince = await _stateProvinceService.GetStateProvinceByNameAsync(model.State);

                var address = new Address
                {
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    City = model.City,
                    ZipPostalCode = model.ZipCode,
                };
                if (existingCountry != null)
                    address.CountryId = existingCountry.Id;
                if (existingStateProvince != null)
                    address.StateProvinceId = existingStateProvince.Id;
                await _addressService.InsertAddressAsync(address);
                lead.AddressId = address.Id;
            }

            if (existingLeadStatus != null)
                lead.LeadStatusId = existingLeadStatus.Id;
            await _leadService.InsertLeadAsync(lead);

            foreach (var tag in model.Tags)
            {
                var leadTags = new LeadTags()
                {
                    LeadId = lead.Id,
                    TagsId = tag
                };
                await _leadService.InsertLeadTagsAsync(leadTags);
            }

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

    [HttpPost]
    public virtual async Task<IActionResult> CreateMultipleLead([FromBody] List<LeadAPIModel> multipleLeads)
    {
        var leadAPIResponseModel = new LeadAPIResponseModel();
        var leadAPILogs = new LeadAPILog();

        try
        {
            leadAPIResponseModel = await CheckIfAuthenticated(leadAPIResponseModel);

            if (!leadAPIResponseModel.Success)
            {
                leadAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.LeadAPI.Common.UnauthorizedAccess");
                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, multipleLeads);

                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            if (string.IsNullOrWhiteSpace(_leadAPISettings.APIKey) || string.IsNullOrWhiteSpace(_leadAPISettings.APISecretKey))
            {
                leadAPIResponseModel.Success = false;
                leadAPIResponseModel.ResponseMessage = "Invalid API Key";

                await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, multipleLeads);
                return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
            }

            foreach (var multipleLead in multipleLeads)
            {
                var existingLead = await _leadService.GetExistingLeadByLinkedinUrlAsync(linkedinURL: multipleLead.LinkedInUrl);
                if (existingLead != null)
                    continue;

                if (string.IsNullOrWhiteSpace(multipleLead.Name) || string.IsNullOrWhiteSpace(multipleLead.CompanyName))
                    continue;

                string firstName = multipleLead.Name;
                string lastName = "";

                if (!string.IsNullOrWhiteSpace(multipleLead.Name))
                {
                    var nameParts = multipleLead.Name.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    firstName = nameParts[0];
                    if (nameParts.Length > 1)
                        lastName = nameParts[1];
                }

                var existingLeadStatus = await _leadStatusService.GetLeadStatusByNameAsync(leadStatusName: LeadAPIDefaults.NewLeadStatus);

                var lead = new Lead
                {
                    FirstName = firstName,
                    LastName = lastName,
                    CompanyName = multipleLead.CompanyName,
                    Email = multipleLead.Email,
                    Phone = multipleLead.MobileNo,
                    Description = multipleLead.Summary,
                    LinkedinUrl = multipleLead.LinkedInUrl,
                    IndustryId = multipleLead.IndustryId,
                    EmailOptOut = true,
                    CreatedOnUtc = DateTime.UtcNow
                };

                if (!string.IsNullOrEmpty(multipleLead.Address1) || !string.IsNullOrEmpty(multipleLead.Address2) || !string.IsNullOrEmpty(multipleLead.ZipCode) || !string.IsNullOrEmpty(multipleLead.City) ||
                    !string.IsNullOrEmpty(multipleLead.State) || !string.IsNullOrEmpty(multipleLead.Country))
                {
                    var existingCountry = await _countryService.GetCountryByNameAsync(multipleLead.Country);
                    var existingStateProvince = await _stateProvinceService.GetStateProvinceByNameAsync(multipleLead.State);

                    var address = new Address
                    {
                        Address1 = multipleLead.Address1,
                        Address2 = multipleLead.Address2,
                        City = multipleLead.City,
                        ZipPostalCode = multipleLead.ZipCode,
                    };
                    if (existingCountry != null)
                        address.CountryId = existingCountry.Id;
                    if (existingStateProvince != null)
                        address.StateProvinceId = existingStateProvince.Id;
                    await _addressService.InsertAddressAsync(address);
                    lead.AddressId = address.Id;
                }

                if (existingLeadStatus != null)
                    lead.LeadStatusId = existingLeadStatus.Id;
                await _leadService.InsertLeadAsync(lead);

                foreach (var tag in multipleLead.Tags)
                {
                    var leadTags = new LeadTags()
                    {
                        LeadId = lead.Id,
                        TagsId = tag
                    };
                    await _leadService.InsertLeadTagsAsync(leadTags);
                }
            }

            leadAPIResponseModel.Success = true;
            leadAPIResponseModel.ResponseMessage = "Lead created successfully";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, multipleLeads);

            return Json(new { result = true, message = leadAPIResponseModel.ResponseMessage });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Error in CreateLead API: " + ex.Message, ex);

            leadAPIResponseModel.Success = false;
            leadAPIResponseModel.ResponseMessage = "An error occurred while processing your request.";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, multipleLeads);

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

    [HttpGet]
    public virtual async Task<IActionResult> GetAllAvailableIndustries()
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

            var model = new List<IndustriesModel>();
            var existingAvailableIndustries = (await _industryService.GetAllIndustryAsync(name: string.Empty)).ToList();
            foreach (var existingAvailableIndustry in existingAvailableIndustries)
            {
                var industriesModel = new IndustriesModel()
                {
                    Id = existingAvailableIndustry.Id,
                    Name = existingAvailableIndustry.Name,
                };
                model.Add(industriesModel);
            }

            leadAPIResponseModel.Success = true;
            leadAPIResponseModel.ResponseMessage = JsonConvert.SerializeObject(model);
            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

            return Json(new
            {
                result = true,
                industries = model
            });

        }
        catch (Exception exception)
        {
            await _logger.ErrorAsync("Error in GetAllAvailableIndustries API: " + exception.Message, exception);

            leadAPIResponseModel.Success = false;
            leadAPIResponseModel.ResponseMessage = "An error occurred while processing your request.";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, null);

            return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
        }
    }

    [HttpPost]
    public virtual async Task<IActionResult> SyncLinkedInMessage([FromBody] LinkedinMessagingModel model)
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

            var existingLead = await _leadService.GetExistingLeadByLinkedinUrlAsync(linkedinURL: model.ReceiverProfile);

            var existingLeadStatus = await _leadStatusService.GetLeadStatusByNameAsync(leadStatusName: LeadAPIDefaults.NewLeadStatus);

            var linkedInMessages = new LinkedInMessages
            {
                Message = model.Message,
                ReceiverName = model.ReceiverName,
                ReceiverProfile = model.ReceiverProfile,
                ConversationId = model.ConversationId,
                Direction = model.Direction,
                CreatedOnUtc = DateTime.UtcNow
            };
            if (existingLead != null)
                linkedInMessages.LeadId = existingLead.Id;
            await _linkedInMessagesService.InsertLinkedinMessageAsync(linkedInMessages);

            leadAPIResponseModel.Success = true;
            leadAPIResponseModel.ResponseMessage = "Linkedin message synced successfully";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

            return Json(new { result = true, message = leadAPIResponseModel.ResponseMessage });
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync("Error in SyncLinkedInMessage API: " + ex.Message, ex);

            leadAPIResponseModel.Success = false;
            leadAPIResponseModel.ResponseMessage = "An error occurred while processing your request.";

            await LogLeadAPICallAsync(leadAPIResponseModel, leadAPILogs, model);

            return Json(new { result = false, message = leadAPIResponseModel.ResponseMessage });
        }
    }

    #endregion
}
