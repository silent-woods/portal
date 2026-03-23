using App.Core.Domain.Media;
using App.Data;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Services.Logging;
using App.Web.Framework.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Inquiries;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.JobPostingApiModels;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.SatyanamAPIResponses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
        private readonly IJobPostingService _jobPostingService;
        private readonly ICandidatesService _candidatesService;
        private readonly IRepository<Download> _downloadRepository;

        #endregion

        #region Ctor 

        public SatyanamAPIController(ILocalizationService localizationService, ILogger logger, SatyanamAPISettings satyanamAPISettings, IInquiryService inquiryService, IWorkflowMessageCRMService workflowMessageCRMService, IJobPostingService jobPostingService, ICandidatesService candidatesService, IRepository<Download> downloadRepository)
        {
            _localizationService = localizationService;
            _logger = logger;
            _satyanamAPISettings = satyanamAPISettings;
            _inquiryService = inquiryService;
            _workflowMessageCRMService = workflowMessageCRMService;
            _jobPostingService = jobPostingService;
            _candidatesService = candidatesService;
            _downloadRepository = downloadRepository;
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
        private string GetEnumDescription(Enum value)
        {
            var field = value.GetType().GetField(value.ToString());

            var attribute = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                            as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
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

                if (parameters.SourceId == 0)
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

        [HttpGet]
        [Route("api/careers/jobs", Name = "GetCareersJobs")]
        public virtual async Task<IActionResult> GetCareersJobs()
        {
            var satyanamAPIResponseModel = new SatyanamAPIResponseModel();

            try
            {
                // Allowed domain validation
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
                        {
                            return StatusCode(StatusCodes.Status403Forbidden, new
                            {
                                result = false,
                                message = await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.AccessDenied")
                            });
                        }
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

                // API authentication
                satyanamAPIResponseModel = await CheckIfAuthenticated(satyanamAPIResponseModel);

                if (!satyanamAPIResponseModel.Success)
                {
                    return Unauthorized(new
                    {
                        result = false,
                        message = satyanamAPIResponseModel.ResponseMessage
                    });
                }

                // Get job postings
                var jobs = await _jobPostingService.GetAllJobPostingAsync("", 0, 0, int.MaxValue, false, null);

                var publishedJobs = jobs
                    .Where(x => x.Publish)
                    .Select(x => new JobPostingApiModel
                    {
                        Id = x.Id,
                        Title = x.Title,
                        Description = x.Description,
                        CandidateTypeId = x.CandidateTypeId,
                        PositionId = x.PositionId,

                    })
                    .ToList();
                var candidateTypes = Enum.GetValues(typeof(CandidateTypeEnum))
            .Cast<CandidateTypeEnum>()
            .Where(e => e != CandidateTypeEnum.Select)
            .Select(e => new
            {
                Id = (int)e,
                Name = e.ToString()
            })
            .ToList();
                var noticePeriods = Enum.GetValues(typeof(NoticePeriodDaysEnum))
    .Cast<NoticePeriodDaysEnum>()
    .Where(e => e != NoticePeriodDaysEnum.Select)
    .Select(e => new
    {
        Id = (int)e,
        Name = GetEnumDescription(e)
    })
    .ToList();

                //  Rate Types (for Freelancer)
                var rateTypes = Enum.GetValues(typeof(RateTypeEnum))
                    .Cast<RateTypeEnum>()
                    .Where(e => e != RateTypeEnum.Select)
                    .Select(e => new
                    {
                        Id = (int)e,
                        Name = e.ToString()
                    })
                    .ToList();

                //  FINAL RESPONSE
                return Ok(new
                {
                    result = true,
                    data = publishedJobs,
                    dropdowns = new
                    {
                        candidateTypes,
                        noticePeriods,
                        rateTypes
                    }
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error in Get Careers Jobs API: " + ex.Message, ex);

                satyanamAPIResponseModel.Success = false;
                satyanamAPIResponseModel.ResponseMessage =
                    await _localizationService.GetResourceAsync("Satyanam.Plugin.Misc.SatyanamCRM.API.Error");

                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    result = false,
                    message = satyanamAPIResponseModel.ResponseMessage
                });
            }
        }

        [HttpPost]
        [Route("api/careers/apply", Name = "ApplyCareerJob")]
        public async Task<IActionResult> ApplyCareerJob([FromForm] ApplyJobApiModel model)
        {
            var response = new SatyanamAPIResponseModel();

            try
            {
                response = await CheckIfAuthenticated(response);

                if (!response.Success)
                {
                    return Unauthorized(new
                    {
                        result = false,
                        message = response.ResponseMessage
                    });
                }

                var job = await _jobPostingService.GetJobPostingByIdAsync(model.JobPostingId);

                if (job == null || !job.Publish)
                {
                    return BadRequest(new
                    {
                        result = false,
                        message = "Invalid job"
                    });
                }

                int downloadId = 0;

                #region Resume Upload

                if (model.ResumeFile != null)
                {
                    byte[] fileBytes;

                    using (var ms = new MemoryStream())
                    {
                        await model.ResumeFile.CopyToAsync(ms);
                        fileBytes = ms.ToArray();
                    }

                    var download = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = false,
                        DownloadBinary = fileBytes,
                        ContentType = model.ResumeFile.ContentType,
                        Filename = model.ResumeFile.FileName,
                        Extension = Path.GetExtension(model.ResumeFile.FileName),
                        IsNew = true
                    };

                    await _downloadRepository.InsertAsync(download);

                    downloadId = download.Id;
                }

                #endregion


                #region Create or Update Candidate

                var candidate = await _candidatesService.GetCandidateByEmailAsync(model.Email);

                if (candidate == null)
                {
                    candidate = new Candidate
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Email = model.Email,
                        Phone = model.Phone,
                        CandidateTypeId = job.CandidateTypeId,
                        SourceTypeId = (int)SourceTypeEnum.Website,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _candidatesService.InsertCandidateAsync(candidate);
                }
                else
                {
                    candidate.FirstName = model.FirstName;
                    candidate.LastName = model.LastName;
                    candidate.Phone = model.Phone;
                    candidate.UpdatedOnUtc = DateTime.UtcNow;

                    await _candidatesService.UpdateCandidateAsync(candidate);
                }

                #endregion


                #region Insert Job Application

                var jobApplication = new JobApplication
                {
                    CandidateId = candidate.Id,
                    JobPostingId = model.JobPostingId,
                    StatusId = (int)CandidateStatusEnum.Applied,
                    AppliedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    ResumeDownloadId = downloadId,
                    PositionApplied = job.Title,
                    PositionId = job.PositionId,
                    AdditionalInformation = model.AdditionalInformation,
                    City = model.City
                };

                if (job.CandidateTypeId == (int)CandidateTypeEnum.Freelancer)
                {
                    jobApplication.RateTypeId = model.RateTypeId;
                    jobApplication.Amount = model.Amount;
                    jobApplication.CoverLetter = model.CoverLetter;
                }
                else
                {
                    jobApplication.ExperienceYears = model.ExperienceYears;
                    jobApplication.CurrentCompany = model.CurrentCompany;
                    jobApplication.CurrentCTC = model.CurrentCTC;
                    jobApplication.ExpectedCTC = model.ExpectedCTC;
                    jobApplication.NoticePeriodId = model.NoticePeriodId;
                    jobApplication.Skills = model.Skills;
                }

                await _candidatesService.InsertJobApplicationAsync(jobApplication);

                #endregion


                return Ok(new
                {
                    result = true,
                    message = "Application submitted successfully"
                });
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("Error in ApplyCareerJob API", ex);

                return StatusCode(500, new
                {
                    result = false,
                    message = "Internal server error"
                });
            }
        }
        #endregion
    }
}