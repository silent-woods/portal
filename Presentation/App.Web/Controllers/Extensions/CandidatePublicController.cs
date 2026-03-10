using App.Core;
using App.Core.Domain.Media;
using App.Data;
using App.Services;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Controllers;
using App.Web.Models.Extensions.Candidate;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public partial class CandidatePublicController : BasePublicController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<Download> _downloadRepository;
        private readonly ICandidatesService _candidateService;
        private readonly IJobPostingService _jobPostingService;
        #endregion

        #region Ctor

        public CandidatePublicController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IDownloadService downloadService,
            IRepository<Download> downloadRepository,
            ICandidatesService candidateService,
            IJobPostingService jobPostingService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _downloadService = downloadService;
            _downloadRepository = downloadRepository;
            _candidateService = candidateService;
            _jobPostingService = jobPostingService;
        }

        #endregion

        #region Utilities



        #endregion

        #region Actions
        public async Task<IActionResult> Apply(int jobId, int? candidateId)
        {
            var model = new CandidatePublicModel();

            model.JobPostingId = jobId;

            var job = await _jobPostingService.GetJobPostingByIdAsync(jobId);

            if (job == null || !job.Publish)
                return RedirectToRoute("HomePage");

            model.PositionApplied = job.Title;
            model.CandidateTypeId = job.CandidateTypeId;
            model.IsFreelancer = job.CandidateTypeId == (int)CandidateTypeEnum.Freelancer;

            // NEW: Load candidate data if invitation link
            if (candidateId.HasValue)
            {
                var candidate = await _candidateService.GetCandidateByIdAsync(candidateId.Value);

                if (candidate != null)
                {
                    model.FirstName = candidate.FirstName;
                    model.LastName = candidate.LastName;
                    model.Email = candidate.Email;
                    model.Phone = candidate.Phone;
                }
            }

            // dropdowns
            model.AvailableRateTypes = await RateTypeEnum.Select.ToSelectListAsync();
            model.AvailableNoticePeriods = await NoticePeriodDaysEnum.Select.ToSelectListAsync();

            return View("~/Themes/DefaultClean/Views/Extension/Candidates/Apply.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Apply(CandidatePublicModel model)
        {
            var job = await _jobPostingService.GetJobPostingByIdAsync(model.JobPostingId);

            if (job == null || !job.Publish)
                return RedirectToRoute("HomePage");

            model.CandidateTypeId = job.CandidateTypeId;
            model.IsFreelancer = job.CandidateTypeId == (int)CandidateTypeEnum.Freelancer;

            if (!ModelState.IsValid)
            {
                model.AvailableRateTypes = await RateTypeEnum.Select.ToSelectListAsync();
                model.AvailableNoticePeriods = await NoticePeriodDaysEnum.Select.ToSelectListAsync();

                return View("~/Themes/DefaultClean/Views/Extension/Candidates/Apply.cshtml", model);
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


            #region STEP 1: Create or Get Candidate (PERSON ONLY)

            var candidate = await _candidateService.GetCandidateByEmailAsync(model.Email);

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

                await _candidateService.InsertCandidateAsync(candidate);
            }
            else
            {
                // update only basic info (NOT job specific)
                candidate.FirstName = model.FirstName;
                candidate.LastName = model.LastName;
                candidate.Phone = model.Phone;
                candidate.UpdatedOnUtc = DateTime.UtcNow;

                await _candidateService.UpdateCandidateAsync(candidate);
            }

            #endregion


            #region STEP 2: Insert JobApplication (JOB SPECIFIC DATA)

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
                City = model.City,
                HrNotes = null
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

            await _candidateService.InsertJobApplicationAsync(jobApplication);

            #endregion


            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Public.Candidate.Submitted"));

            return RedirectToAction("Apply", new { jobId = model.JobPostingId });
        }



        #endregion
    }
}