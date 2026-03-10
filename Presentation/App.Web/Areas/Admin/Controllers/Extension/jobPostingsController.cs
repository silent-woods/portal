using App.Core;
using App.Core.Domain.JobPostings;
using App.Core.Domain.Security;
using App.Core.Domain.Messages;
using App.Data;
using App.Data.Extensions;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.JobPostings;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class JobPostingsController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly IJobPostingsModelFactory _jobPostingsModelFactory;
        private readonly IJobPostingService _jobPostingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICandidatesService _candidatesService;
        private readonly ICandidateModelFactory _candidateModelFactory;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ITechnologySkillMappingService _technologySkillMappingService;
        private readonly ISkillSetService _skillSetService;
        private readonly ITechnologyService _technologyService;
        private readonly IJobPostingSkillMappingService _jobPostingSkillMappingService;
        private readonly IJobPostingTechnologyMappingService _jobPostingTechnologyMappingService;
        private readonly IRepository<JobPostingSkillMapping> _jobPostingSkillMappingRepository;
        private readonly IRepository<SkillSet> _skillSetRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStoreContext _storeContext;
        private readonly IRepository<JobApplication> _jobApplicationRepository;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<JobPosting> _jobPostingRepository;

        #endregion

        #region Ctor

        public JobPostingsController(IPermissionService permissionService,
           IJobPostingsModelFactory jobPostingsModelFactory,
        IJobPostingService jobPostingService,
        INotificationService notificationService,
            ILocalizationService localizationService
,
            ICandidatesService candidatesService,
            ICandidateModelFactory candidateModelFactory,
            IRepository<Candidate> candidateRepository,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            ITechnologySkillMappingService technologySkillMappingService,
            ISkillSetService skillSetService,
            ITechnologyService technologyService,
            IJobPostingSkillMappingService jobPostingSkillMappingService,
            IJobPostingTechnologyMappingService jobPostingTechnologyMappingService,
            IRepository<JobPostingSkillMapping> jobPostingSkillMappingRepository,
            IRepository<SkillSet> skillSetRepository,
            IHttpContextAccessor httpContextAccessor,
            IStoreContext storeContext,
            IRepository<JobApplication> jobApplicationRepository,
            IDownloadService downloadService,
            IRepository<JobPosting> jobPostingRepository)
        {
            _permissionService = permissionService;
            _jobPostingsModelFactory = jobPostingsModelFactory;
            _jobPostingService = jobPostingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _candidatesService = candidatesService;
            _candidateModelFactory = candidateModelFactory;
            _candidateRepository = candidateRepository;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _technologySkillMappingService = technologySkillMappingService;
            _skillSetService = skillSetService;
            _technologyService = technologyService;
            _jobPostingSkillMappingService = jobPostingSkillMappingService;
            _jobPostingTechnologyMappingService = jobPostingTechnologyMappingService;
            _jobPostingSkillMappingRepository = jobPostingSkillMappingRepository;
            _skillSetRepository = skillSetRepository;
            _httpContextAccessor = httpContextAccessor;
            _storeContext = storeContext;
            _jobApplicationRepository = jobApplicationRepository;
            _downloadService = downloadService;
            _jobPostingRepository = jobPostingRepository;
        }

        #endregion

        #region Utilities
        public virtual async Task<JobPostingCandidateListModel> PrepareJobPostingCandidatesListModelAsync(JobPostingCandidateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Join Candidate + JobApplication
            var query =
                from candidate in _candidateRepository.Table
                join jobApp in _jobApplicationRepository.Table
                    on candidate.Id equals jobApp.CandidateId
                where jobApp.JobPostingId == searchModel.JobPostingId
                select new
                {
                    Candidate = candidate,
                    JobApplication = jobApp
                };

            // Name filter
            if (!string.IsNullOrWhiteSpace(searchModel.Name))
            {
                query = query.Where(x =>
                    (x.Candidate.FirstName + " " + x.Candidate.LastName)
                    .Contains(searchModel.Name));
            }

            // Email filter
            if (!string.IsNullOrWhiteSpace(searchModel.Email))
            {
                query = query.Where(x =>
                    x.Candidate.Email.Contains(searchModel.Email));
            }

            // Candidate Type filter
            if (searchModel.CandidateTypeId.GetValueOrDefault() > 0)
            {
                query = query.Where(x =>
                    x.Candidate.CandidateTypeId == searchModel.CandidateTypeId.Value);
            }

            // Status filter (IMPORTANT → from JobApplication)
            if (searchModel.StatusId.GetValueOrDefault() > 0)
            {
                query = query.Where(x =>
                    x.JobApplication.StatusId == searchModel.StatusId.Value);
            }

            // Paging
            var pagedList = await query
                .OrderByDescending(x => x.JobApplication.AppliedOnUtc)
                .ToPagedListAsync(searchModel.Page - 1, searchModel.PageSize);

            // Grid mapping
            var model = new JobPostingCandidateListModel().PrepareToGrid(
                searchModel,
                pagedList,
                () =>
                {
                    return pagedList.Select(x => new JobPostingCandidateModel
                    {
                        Id = x.Candidate.Id,

                        Name = x.Candidate.FirstName + " " + x.Candidate.LastName,

                        Email = x.Candidate.Email,

                        CandidateTypeName = Enum.GetName(
                            typeof(CandidateTypeEnum),
                            x.Candidate.CandidateTypeId),

                        // FIXED: Status from JobApplication
                        Status = Enum.GetName(
                            typeof(CandidateStatusEnum),
                            x.JobApplication.StatusId),
                        JobPostingId = x.JobApplication.JobPostingId,

                        //  FIXED: Resume from JobApplication NOT Candidate
                        ResumeDownloadId = x.JobApplication.ResumeDownloadId,

                        // FIXED: Date from JobApplication
                        CreatedOnUtc = x.JobApplication.AppliedOnUtc
                    });
                });

            return model;
        }

        private async Task SendInvitationEmailAsync(Candidate candidate, JobPosting jobPosting)
        {
            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null) return;
            var request = _httpContextAccessor.HttpContext?.Request;
            string baseUrl = request != null
        ? $"{request.Scheme}://{request.Host}"
        : (await _storeContext.GetCurrentStoreAsync()).Url;

            // STEP 2: Create Apply Link
            var applyLink = $"{baseUrl}/CandidatePublic/Apply?jobId={jobPosting.Id}&candidateId={candidate.Id}";
            var subject = $"We are hiring for {jobPosting.Title}";
            var body = $@"
                        <p>Dear {candidate.FirstName},</p>
                        <p>
                            We found your profile suitable for the position 
                            <strong>{jobPosting.Title}</strong>.
                        </p>
                        <p>
                            Click the link below to apply for this job:
                        </p>
                        <p>
                            <a href='{applyLink}' 
                               style='background-color:#28a745;
                                      color:white;
                                      padding:10px 20px;
                                      text-decoration:none;
                                      border-radius:5px;
                                      display:inline-block;'>
                                Apply Now
                            </a>
                        </p>
                        <br/>
                        <p>Best Regards,<br/>HR Team</p>";

            var queuedEmail = new QueuedEmail
            {
                Priority = QueuedEmailPriority.High,
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = candidate.Email,
                ToName = candidate.FirstName,
                Subject = subject,
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id
            };

            await _queuedEmailService.InsertQueuedEmailAsync(queuedEmail);
        }

        #endregion

        #region Job Posting List/Create/Edit/Delete
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsSearchModelAsync(new JobPostingSearchModel());
            return View("/Areas/Admin/Views/Extension/jobPosting/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(JobPostingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(new JobPostingModel(), null);

            return View("/Areas/Admin/Views/Extension/JobPosting/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(JobPostingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Add))
                return AccessDeniedView();

            var timeSheet = model.ToEntity<JobPosting>();
            model.AvailablePosition.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.Position.select"),
                Value = "0"
            });
            model.AvailableCandidateTypes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Extension.Jobposting.Fields.CandidateType.select"),
                Value = "0"
            });
            ModelState.Remove("CandidateModel.FirstName");
            ModelState.Remove("CandidateModel.LastName");
            ModelState.Remove("CandidateModel.Email");
            ModelState.Remove("CandidateModel.PositionId");
            ModelState.Remove("CandidateModel.CandidateTypeId");
            if (ModelState.IsValid)
            {
                timeSheet.CreatedOn = DateTime.UtcNow;
                timeSheet.UpdatedOn = DateTime.UtcNow;

                await _jobPostingService.InsertJobPostingAsync(timeSheet);
                if (!string.IsNullOrEmpty(Request.Form["NewTechnologyNames"]))
                {
                    var newTechNames = Request.Form["NewTechnologyNames"]
                        .ToString()
                        .Split(",", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var techName in newTechNames)
                    {
                        var tech = new Technology
                        {
                            Name = techName,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        await _technologyService.InsertTechnologyAsync(tech);

                        model.SelectedTechnologyIds.Add(tech.Id);
                    }
                }
                if (!string.IsNullOrEmpty(Request.Form["NewSkillNames"]))
                {
                    var newSkillNames = Request.Form["NewSkillNames"]
                        .ToString()
                        .Split(",", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var skillName in newSkillNames)
                    {
                        var skill = new SkillSet
                        {
                            Name = skillName,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        await _skillSetService.InsertSkillAsync(skill);

                        if (model.SelectedSkillIds == null)
                            model.SelectedSkillIds = new List<int>();

                        model.SelectedSkillIds.Add(skill.Id);
                    }
                }
                await _jobPostingTechnologyMappingService
            .DeleteByJobPostingIdAsync(timeSheet.Id);

                if (model.SelectedTechnologyIds != null)
                {
                    foreach (var techId in model.SelectedTechnologyIds)
                    {
                        await _jobPostingTechnologyMappingService.InsertAsync(
                            new JobPostingTechnologyMapping
                            {
                                JobPostingId = timeSheet.Id,
                                TechnologyId = techId
                            });
                    }
                }
                // ⭐ STEP 5.2 — Save Skills mapping
                if (model.SelectedSkillIds != null)
                {
                    foreach (var skillId in model.SelectedSkillIds)
                    {
                        await _jobPostingSkillMappingService.InsertAsync(
                            new JobPostingSkillMapping
                            {
                                JobPostingId = timeSheet.Id,
                                SkillSetId = skillId
                            });
                    }
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.JobPosting.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }
            //prepare model
            model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(model, timeSheet, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/JobPosting/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Edit))
                return AccessDeniedView();

            var jobposting = await _jobPostingService.GetJobPostingByIdAsync(id);
            if (jobposting == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(null, jobposting);

            return View("/Areas/Admin/Views/Extension/JobPosting/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(JobPostingModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(model.Id);
            if (jobPosting == null)
                return RedirectToAction("List");
            ModelState.Remove("CandidateModel.FirstName");
            ModelState.Remove("CandidateModel.LastName");
            ModelState.Remove("CandidateModel.Email");
            ModelState.Remove("CandidateModel.PositionId");
            ModelState.Remove("CandidateModel.CandidateTypeId");

            if (ModelState.IsValid)
            {
                jobPosting = model.ToEntity(jobPosting);
                jobPosting.UpdatedOn = DateTime.UtcNow;

                await _jobPostingService.UpdateJobPostingAsync(jobPosting);
                await _jobPostingSkillMappingService.DeleteByJobPostingIdAsync(jobPosting.Id);
                if (!string.IsNullOrEmpty(Request.Form["NewTechnologyNames"]))
                {
                    var newTechNames = Request.Form["NewTechnologyNames"]
                        .ToString()
                        .Split(",", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var techName in newTechNames)
                    {
                        var tech = new Technology
                        {
                            Name = techName,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        await _technologyService.InsertTechnologyAsync(tech);

                        model.SelectedTechnologyIds.Add(tech.Id);
                    }
                }
                if (!string.IsNullOrEmpty(Request.Form["NewSkillNames"]))
                {
                    var newSkillNames = Request.Form["NewSkillNames"]
                        .ToString()
                        .Split(",", StringSplitOptions.RemoveEmptyEntries);

                    foreach (var skillName in newSkillNames)
                    {
                        var skill = new SkillSet
                        {
                            Name = skillName,
                            Published = true,
                            CreatedOnUtc = DateTime.UtcNow
                        };

                        await _skillSetService.InsertSkillAsync(skill);

                        if (model.SelectedSkillIds == null)
                            model.SelectedSkillIds = new List<int>();

                        model.SelectedSkillIds.Add(skill.Id);

                        // IMPORTANT: map skill to selected technologies
                        if (model.SelectedTechnologyIds != null)
                        {
                            foreach (var techId in model.SelectedTechnologyIds)
                            {
                                await _technologySkillMappingService.InsertAsync(new TechnologySkillMapping
                                {
                                    TechnologyId = techId,
                                    SkillSetId = skill.Id
                                });
                            }
                        }
                    }
                }
                await _jobPostingTechnologyMappingService
            .DeleteByJobPostingIdAsync(jobPosting.Id);

                if (model.SelectedTechnologyIds != null)
                {
                    foreach (var techId in model.SelectedTechnologyIds)
                    {
                        await _jobPostingTechnologyMappingService.InsertAsync(
                            new JobPostingTechnologyMapping
                            {
                                JobPostingId = jobPosting.Id,
                                TechnologyId = techId
                            });
                    }
                }
                await _jobPostingSkillMappingService
           .DeleteByJobPostingIdAsync(jobPosting.Id);
                // INSERT NEW MAPPINGS
                if (model.SelectedSkillIds != null && model.SelectedSkillIds.Any())
                {
                    foreach (var skillId in model.SelectedSkillIds)
                    {
                        await _jobPostingSkillMappingService.InsertAsync(
                            new JobPostingSkillMapping
                            {
                                JobPostingId = jobPosting.Id,
                                SkillSetId = skillId
                            });
                    }
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.JobPosting.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = jobPosting.Id });
            }
            model = await _jobPostingsModelFactory.PrepareJobPostingsModelAsync(model, jobPosting, true);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/JobPosting/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(id);
            if (jobPosting == null)
                return RedirectToAction("List");

            await _jobPostingService.DeleteJobPostingAsync(jobPosting);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.JobPosting.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _jobPostingService.GetJobPostingByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _jobPostingService.DeleteJobPostingAsync(item);
            }
            return Json(new { Result = true });
        }
        public async Task<IActionResult> DownloadResume(int candidateId, int jobPostingId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            var application = await _candidatesService
        .GetJobApplicationAsync(candidateId, jobPostingId);

            if (application?.ResumeDownloadId == null)
                return NotFound();

            var download = await _downloadService
                .GetDownloadByIdAsync(application.ResumeDownloadId.Value);

            return File(download.DownloadBinary, download.ContentType, download.Filename);
        }
        [HttpGet]
        public async Task<IActionResult> GetJobPostingCandidateSummary(int jobPostingId)
        {
            var jobApplications = await _jobApplicationRepository.Table
        .Where(x => x.JobPostingId == jobPostingId)
        .ToListAsync();

            var candidateIds = jobApplications.Select(x => x.CandidateId).ToList();

            var candidates = await _candidatesService.GetCandidateByIdsAsync(candidateIds.ToArray());

            // STATUS SUMMARY
            var statusSummary = Enum.GetValues(typeof(CandidateStatusEnum))
                .Cast<CandidateStatusEnum>()
                .Where(e => e != CandidateStatusEnum.Select)
                .Select(status => new
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    Count = jobApplications.Count(x => x.StatusId == (int)status),
                    Type = "Status"
                });

            // TYPE SUMMARY
            var typeSummary = Enum.GetValues(typeof(CandidateTypeEnum))
                .Cast<CandidateTypeEnum>()
                .Where(e => e != CandidateTypeEnum.Select)
                .Select(type => new
                {
                    Id = (int)type,
                    Name = type.ToString(),
                    Count = candidates.Count(c => c.CandidateTypeId == (int)type),
                    Type = "CandidateType"
                });

            return Json(statusSummary.Concat(typeSummary));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateCandidateStatus(int[] candidateIds, int statusId, int jobPostingId)
        {
            if (candidateIds == null || candidateIds.Length == 0)
                return BadRequest("No candidate selected");

            var jobApplications = await _jobApplicationRepository.Table
                .Where(x => candidateIds.Contains(x.CandidateId)
                         && x.JobPostingId == jobPostingId)
                .ToListAsync();

            foreach (var jobApp in jobApplications)
            {
                jobApp.StatusId = statusId;
                jobApp.UpdatedOnUtc = DateTime.UtcNow;

                await _jobApplicationRepository.UpdateAsync(jobApp);
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> JobPostingCandidates(JobPostingCandidateSearchModel searchModel)
        {
            var model = await PrepareJobPostingCandidatesListModelAsync(searchModel);
            return Json(model);
        }
        public async Task<IActionResult> InvitationCandidatesPopup(int jobPostingId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            var searchModel = new InvitationCandidateSearchModel
            {
                JobPostingId = jobPostingId
            };
            //CANDIDATE TYPE ENUM 

            searchModel.AvailableCandidateTypeName = Enum.GetValues(typeof(CandidateTypeEnum))
                .Cast<CandidateTypeEnum>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                }).ToList();

            searchModel.AvailableCandidateTypeName.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = ""
            });

            // STATUS ENUM

            searchModel.AvailableStatus = Enum.GetValues(typeof(CandidateStatusEnum))
                .Cast<CandidateStatusEnum>()
                .Select(e => new SelectListItem
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                }).ToList();

            searchModel.AvailableStatus.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = ""
            });

            var model = new JobPostingModel
            {
                Id = jobPostingId,
                InvitationCandidateSearchModel = searchModel
            };

            return View("/Areas/Admin/Views/Extension/JobPosting/SendInvitation.cshtml", model);
        }

        public async Task<IActionResult> InvitationCandidates(InvitationCandidateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(searchModel.JobPostingId);

            if (jobPosting == null)
                return Json(new InvitationCandidateListModel());

            // Get Job Skills
            var jobSkillNames = await (
                from mapping in _jobPostingSkillMappingRepository.Table
                join skill in _skillSetRepository.Table
                    on mapping.SkillSetId equals skill.Id
                where mapping.JobPostingId == searchModel.JobPostingId
                select skill.Name.ToLower()
            ).ToListAsync();

            // Exclude candidates already applied
            var appliedCandidateIds = await _jobApplicationRepository.Table
                .Where(x => x.JobPostingId == searchModel.JobPostingId)
                .Select(x => x.CandidateId)
                .ToListAsync();

            var candidateQuery = _candidateRepository.Table
                .Where(c =>
                    c.CandidateTypeId == jobPosting.CandidateTypeId &&
                    !appliedCandidateIds.Contains(c.Id)
                );

            // Skill matching
            if (jobSkillNames.Any())
            {
                candidateQuery = candidateQuery.Where(c =>
                    _jobApplicationRepository.Table.Any(app =>
                        app.CandidateId == c.Id &&
                        app.Skills != null &&
                        jobSkillNames.Any(skill =>
                            app.Skills.ToLower().Contains(skill)
                        )
                    )
                );
            }

            // Filters
            if (!string.IsNullOrWhiteSpace(searchModel.SearchName))
                candidateQuery = candidateQuery.Where(c =>
                    (c.FirstName + " " + c.LastName)
                    .Contains(searchModel.SearchName));

            if (!string.IsNullOrWhiteSpace(searchModel.SearchEmail))
                candidateQuery = candidateQuery.Where(c =>
                    c.Email.Contains(searchModel.SearchEmail));

            if (searchModel.SearchCandidateTypeId.HasValue)
                candidateQuery = candidateQuery.Where(c =>
                    c.CandidateTypeId == searchModel.SearchCandidateTypeId.Value);

            var totalCount = await candidateQuery.CountAsync();

            var candidates = await candidateQuery
                .OrderByDescending(c => c.CreatedOnUtc)
                .Skip(searchModel.Start)
                .Take(searchModel.Length)
                .ToListAsync();

            var candidateIds = candidates.Select(c => c.Id).ToList();

            // FIXED SECTION (No EF translation issue)
            var latestApplications = await _jobApplicationRepository.Table
                .Where(a => candidateIds.Contains(a.CandidateId))
                .OrderByDescending(a => a.AppliedOnUtc)
                .ToListAsync();

            latestApplications = latestApplications
                .GroupBy(a => a.CandidateId)
                .Select(g => g.First())
                .ToList();

            var model = new InvitationCandidateListModel
            {
                Data = candidates.Select(c =>
                {
                    var latestApp = latestApplications
                        .FirstOrDefault(a => a.CandidateId == c.Id);

                    return new InvitationCandidateModel
                    {
                        Id = c.Id,
                        Name = c.FirstName + " " + c.LastName,
                        Email = c.Email,
                        CandidateTypeName = Enum.GetName(
                            typeof(CandidateTypeEnum),
                            c.CandidateTypeId),

                        Status = latestApp != null
                            ? ((CandidateStatusEnum)latestApp.StatusId).ToString()
                            : "",

                        CreatedOnUtc = c.CreatedOnUtc
                    };
                }),

                Draw = searchModel.Draw,
                RecordsTotal = totalCount,
                RecordsFiltered = totalCount
            };

            return Json(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendInvitationToSelected(int[] selectedIds, int jobPostingId)
        {
            if (selectedIds == null || !selectedIds.Any())
                return Json(new { success = false, message = "No candidates selected." });

            var jobPosting = await _jobPostingService.GetJobPostingByIdAsync(jobPostingId);
            if (jobPosting == null)
                return Json(new { success = false, message = "Job posting not found." });

            foreach (var candidateId in selectedIds)
            {
                var candidate = await _candidatesService.GetCandidateByIdAsync(candidateId);
                if (candidate == null)
                    continue;

                await SendInvitationEmailAsync(candidate, jobPosting);
            }

            return Json(new { success = true, message = "Invitation sent successfully." });
        }
        [Area("Admin")]
        [HttpPost]
        public async Task<IActionResult> GetSkillsByTechnologies(int[] technologyIds)
        {
            if (technologyIds == null || technologyIds.Length == 0)
                return Json(new List<SelectListItem>());

            var skillIds = new List<int>();

            foreach (var techId in technologyIds)
            {
                var mappings = await _technologySkillMappingService.GetByTechnologyIdAsync(techId);

                skillIds.AddRange(mappings.Select(x => x.SkillSetId));
            }

            skillIds = skillIds.Distinct().ToList();

            var skills = await _skillSetService.GetSkillByIdsAsync(skillIds.ToArray());

            var result = skills.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> GetCandidateHistoryInvitionPopupTooltip(int candidateId)
        {
            var applications = await _jobApplicationRepository.Table
                .Where(x => x.CandidateId == candidateId)
                .OrderByDescending(x => x.AppliedOnUtc)
                .ToListAsync();

            if (!applications.Any())
                return Content("");

            var html = "";

            foreach (var app in applications)
            {
                var job = await _jobPostingRepository.GetByIdAsync(app.JobPostingId);
                if (job == null) continue;

                var status = ((CandidateStatusEnum)app.StatusId).ToString();

                html += $@"
        <div style='margin-bottom:6px'>
            <b>Applied on {job.Title}</b><br/>
            Status: {status}<br/>
            Applied: {app.AppliedOnUtc.ToLocalTime():dd MMM yyyy hh:mm tt}
        </div>
        <hr style='margin:4px 0;' />";
            }

            return Content(html);
        }
        [HttpGet]
        public async Task<IActionResult> GetCandidateHistoryTooltip(int candidateId)
        {
            var applications = await (
    from app in _jobApplicationRepository.Table
    join job in _jobPostingRepository.Table
        on app.JobPostingId equals job.Id into jobGroup
    from job in jobGroup.DefaultIfEmpty()
    where app.CandidateId == candidateId
    orderby app.AppliedOnUtc descending
    select new
    {
        Title = job != null ? job.Title : app.PositionApplied,
        app.StatusId,
        app.AppliedOnUtc
    }).ToListAsync();

            if (!applications.Any())
                return Content("");

            var html = "";

            foreach (var app in applications)
            {
                var status = Enum.GetName(typeof(CandidateStatusEnum), app.StatusId);

                html += $@"
        <div style='margin-bottom:6px'>
            <b>Applied on {app.Title}</b><br/>
            Status: {status}<br/>
            Applied: {app.AppliedOnUtc.ToLocalTime():dd MMM yyyy hh:mm tt}
        </div>
        <hr style='margin:4px 0;' />";
            }

            return Content(html);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTechnology(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return Json(null);

            var existing = (await _technologyService.GetAllPublishedTechnologiesAsync())
                .FirstOrDefault(x => x.Name.ToLower() == name.ToLower());

            if (existing != null)
            {
                return Json(new
                {
                    id = existing.Id,
                    text = existing.Name
                });
            }

            var tech = new Technology
            {
                Name = name,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _technologyService.InsertTechnologyAsync(tech);

            return Json(new
            {
                id = tech.Id,
                text = tech.Name
            });
        }
        [HttpPost]
        public async Task<IActionResult> CreateSkill(string name)
        {
            var skill = new SkillSet
            {
                Name = name,
                DisplayOrder = 0,
                Published = true,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _skillSetService.InsertSkillAsync(skill);

            return Json(new
            {
                id = skill.Id,
                text = skill.Name
            });
        }
        #endregion
    }
}