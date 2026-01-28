using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.ProjectTasks;
using App.Services.Configuration;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Newtonsoft.Json;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Inquiries;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.SatyanamAPIResponses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.SatyanamCRM.Controllers
{

    public partial class SatyanamAPIController : BaseController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly SatyanamAPISettings _satyanamAPISettings;
        private readonly IInquiryService _inquiryService;
        private readonly IWorkflowMessageCRMService _workflowMessageCRMService;

        #endregion

        #region Ctor 

        public SatyanamAPIController(ILocalizationService localizationService, ILogger logger, SatyanamAPISettings satyanamAPISettings, IInquiryService inquiryService, IWorkflowMessageCRMService workflowMessageCRMService)
        {
            _localizationService = localizationService;
            _logger = logger;
            _satyanamAPISettings = satyanamAPISettings;
            _inquiryService = inquiryService;
            _workflowMessageCRMService = workflowMessageCRMService;
        }

        #endregion

        #region Utilities
        protected virtual async Task<SatyanamAPIResponseModel> CheckIfAuthenticated(SatyanamAPIResponseModel satyanamAPIResponseModel)
        {
            bool ValidateApiKeyAndSecret(string apiKey, string apiSecret)
            {
                return apiKey != null && apiSecret != null && apiKey == _satyanamAPISettings.APIKey && apiSecret == _satyanamAPISettings.APISecret;
            }

            string unauthorizedMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.UnauthorizedAccess");

            if (Request.Headers.TryGetValue(SatyanamAPIDefaults.APIKeyHeader, out var apiKeyValues) &&
                Request.Headers.TryGetValue(SatyanamAPIDefaults.APISecretKeyHeader, out var apiSecretValues))
            {
                var apiKey = apiKeyValues.FirstOrDefault();
                var apiSecret = apiSecretValues.FirstOrDefault();

                satyanamAPIResponseModel.Success = ValidateApiKeyAndSecret(apiKey, apiSecret);
                satyanamAPIResponseModel.ResponseMessage = satyanamAPIResponseModel.Success ? null : unauthorizedMessage;

                return satyanamAPIResponseModel;
            }

            satyanamAPIResponseModel.Success = false;
            satyanamAPIResponseModel.ResponseMessage = unauthorizedMessage;
            return satyanamAPIResponseModel;
        }
        #endregion

        #region Methods

        [HttpPost]
        [Route("/api/inquiry/insert", Name = nameof(InsertInquiry))]
        public virtual async Task<IActionResult> InsertInquiry([FromBody] InquiryModel parameters)
        {

            var satyanamAPIResponseModel = new SatyanamAPIResponseModel();

            try
            {
                var allowedDomainsSetting = _satyanamAPISettings.AllowedDomains;
                var allowedDomains = allowedDomainsSetting?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(d => d.Trim())
                    .ToList() ?? new List<string>();
                var origin = Request.Headers["Origin"].ToString();
                if (allowedDomains.Any())
                {
                    if (!string.IsNullOrEmpty(origin))
                    {
                        var isAllowed = allowedDomains.Any(domain =>
                            domain.Equals(origin, StringComparison.OrdinalIgnoreCase));

                        if (!isAllowed)
                            return StatusCode(StatusCodes.Status403Forbidden, new
                            {
                                result = false,
                                message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.AccessDenied")
                            });
                    }
                    else
                    {
                        return BadRequest(new
                        {
                            result = false,
                            message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.MissingHeader")
                        });
                    }
                }

                satyanamAPIResponseModel = await CheckIfAuthenticated(satyanamAPIResponseModel);

                if (!satyanamAPIResponseModel.Success)
                {
                    return Unauthorized(new
                    {
                        result = false,
                        message = satyanamAPIResponseModel.ResponseMessage
                    });
                }

                if (!string.IsNullOrWhiteSpace(parameters.FullName))
                {
                    var nameParts = parameters.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    parameters.FirstName = nameParts.Length > 0 ? nameParts[0] : null;
                    parameters.LastName = nameParts.Length > 1 ? string.Join(" ", nameParts.Skip(1)) : null;
                }

                if (!string.IsNullOrWhiteSpace(parameters.Source) && Enum.TryParse(parameters.Source, true, out InquirySourceType parsedSource))
                     parameters.SourceId = (int)parsedSource;
                
                if(parameters.SourceId == 0)
                {
                    return BadRequest(new
                    {
                        result = false,
                        message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.InvalidSource")
                    });
                }

                var inquiry = new Inquiry
                {
                    SourceId = parameters.SourceId,
                    FirstName = parameters.FirstName,
                    LastName = parameters.LastName,
                    Email = parameters.Email,
                    ContactNo = parameters.ContactNo,
                    Company = parameters.Company,
                    WantToHire = parameters.WantToHire,
                    ProjectType = parameters.ProjectType,
                    EngagementDuration = parameters.EngagementDuration,
                    TimeCommitment = parameters.TimeCommitment,
                    Budget = parameters.Budget,
                    Describe = parameters.Describe,
                    CreatedOnUtc = DateTime.UtcNow
                };

                await _inquiryService.InsertInquiryAsync(inquiry);
                await _workflowMessageCRMService.SendNewInquiryNotificationAsync(inquiry);
                satyanamAPIResponseModel.Success = true;
                satyanamAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.Success");

                return Ok(new
                {
                    result = true,
                    message = satyanamAPIResponseModel.ResponseMessage
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error in Insert Inquiry Quote API: " + ex.Message, ex);

                satyanamAPIResponseModel.Success = false;
                satyanamAPIResponseModel.ResponseMessage = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.Error");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    result = false,
                    message = satyanamAPIResponseModel.ResponseMessage
                });
            }
        }

        #endregion
    }
}
