using App.Core;
using App.Core.Domain.JobPostings;
using App.Data;
using App.Data.Extensions;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Extension.Candidates;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc.Filters;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class CandidateController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICandidateModelFactory _candidateModelFactory;
        private readonly ICandidatesService _candidatesService;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<JobApplication> _jobApplicationRepository;
        private readonly IJobPostingService _jobPostingService;
        private readonly IRepository<JobPosting> _jobPostingRepository;
        #endregion

        #region Ctor

        public CandidateController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICandidateModelFactory candidateModelFactory,
            ICandidatesService candidatesService,
            IDownloadService downloadService,
            IRepository<Candidate> candidateRepository,
            IRepository<JobApplication> jobApplicationRepository,
            IJobPostingService jobPostingService,
            IRepository<JobPosting> jobPostingRepository)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _candidateModelFactory = candidateModelFactory;
            _candidatesService = candidatesService;
            _downloadService = downloadService;
            _candidateRepository = candidateRepository;
            _jobApplicationRepository = jobApplicationRepository;
            _jobPostingService = jobPostingService;
            _jobPostingRepository = jobPostingRepository;
        }

        #endregion

        #region Utilities

        public async Task<byte[]> ExportCandidatesToExcelAsync(List<CandidateDto> list)
        {
            await using var stream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Candidates");

                //Column Widths
                sheet.Column("A").Width = 15;
                sheet.Column("B").Width = 15;
                sheet.Column("C").Width = 25;
                sheet.Column("D").Width = 15;
                sheet.Column("E").Width = 20;
                sheet.Column("F").Width = 20;
                sheet.Column("G").Width = 15;
                sheet.Column("H").Width = 18;
                sheet.Column("I").Width = 18;
                sheet.Column("J").Width = 18;
                sheet.Column("K").Width = 15;
                sheet.Column("L").Width = 15;
                sheet.Column("M").Width = 25;
                sheet.Column("N").Width = 15;
                sheet.Column("O").Width = 20;
                sheet.Column("P").Width = 15;
                sheet.Column("Q").Width = 15;
                sheet.Column("R").Width = 18;
                sheet.Column("S").Width = 25;
                sheet.Column("T").Width = 25;
                sheet.Column("U").Width = 20;
                sheet.Column("V").Width = 20;
                sheet.Column("W").Width = 20;

                //Page Setup
                sheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                sheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
                sheet.PageSetup.CenterHorizontally = true;

                int row = 1;

                //Headers
                string[] headers =
                {
            "First Name","Last Name","Email","Phone",
            "Position Applied","Position",
            "Experience Years",
            "Source","Status","Candidate Type",
            "Rate Type","Link Type",
            "Url","Amount",
            "Current Company","Current CTC","Expected CTC",
            "Notice Period",
            "Additional Info","HR Notes",
            "City",
            "Created On","Updated On"
        };

                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cell(row, i + 1).Value = headers[i];
                    sheet.Cell(row, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // Header Style
                sheet.Range($"A{row}:W{row}").Style.Font.Bold = true;
                sheet.Range($"A{row}:W{row}").Style.Fill.BackgroundColor =
                    XLColor.FromTheme(XLThemeColor.Accent1, 0.75);

                row++;

                //Data Rows
                foreach (var item in list)
                {
                    sheet.Cell(row, 1).Value = item.FirstName;
                    sheet.Cell(row, 2).Value = item.LastName;
                    sheet.Cell(row, 3).Value = item.Email;
                    sheet.Cell(row, 4).Value = item.Phone;

                    sheet.Cell(row, 5).Value = item.PositionApplied;
                    sheet.Cell(row, 6).Value = item.Position;

                    sheet.Cell(row, 7).Value = item.ExperienceYears;

                    sheet.Cell(row, 8).Value = item.Source;
                    sheet.Cell(row, 9).Value = item.Status;
                    sheet.Cell(row, 10).Value = item.CandidateType;

                    sheet.Cell(row, 11).Value = item.RateType;
                    sheet.Cell(row, 12).Value = item.LinkType;

                    sheet.Cell(row, 13).Value = item.Url;
                    sheet.Cell(row, 14).Value = item.Amount;

                    sheet.Cell(row, 15).Value = item.CurrentCompany;
                    sheet.Cell(row, 16).Value = item.CurrentCTC;
                    sheet.Cell(row, 17).Value = item.ExpectedCTC;

                    sheet.Cell(row, 18).Value = item.NoticePeriod;

                    sheet.Cell(row, 19).Value = item.AdditionalInformation;
                    sheet.Cell(row, 20).Value = item.HrNotes;
                    sheet.Cell(row, 21).Value = item.City;

                    if (item.CreatedOnUtc != DateTime.MinValue)
                    {
                        sheet.Cell(row, 22).Value = item.CreatedOnUtc;
                        sheet.Cell(row, 22).Style.DateFormat.Format = "MM/dd/yyyy h:mm:ss AM/PM";
                    }

                    if (item.UpdatedOnUtc != DateTime.MinValue)
                    {
                        sheet.Cell(row, 23).Value = item.UpdatedOnUtc;
                        sheet.Cell(row, 23).Style.DateFormat.Format = "MM/dd/yyyy h:mm:ss AM/PM";
                    }

                    // Wrap text
                    sheet.Range($"A{row}:W{row}").Style.Alignment.WrapText = true;

                    row++;
                }

                workbook.SaveAs(stream);
            }

            return stream.ToArray();
        }

        private void ParseEnum<TEnum>(string text, Action<int> setValue) where TEnum : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(text))
                return;

            var cleaned = text.Trim();

            if (Enum.TryParse<TEnum>(cleaned, true, out var enumValue))
            {
                setValue(Convert.ToInt32(enumValue));
            }
        }

        public async Task ImportCandidatesFromExcelAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Excel file is empty");

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var sheet = workbook.Worksheet(1);

            var rows = sheet.RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                var email = row.Cell(3).GetString()?.Trim();

                if (string.IsNullOrWhiteSpace(email))
                    continue;

                //CREATE CANDIDATE

                var candidate = new Candidate
                {
                    FirstName = row.Cell(1).GetString()?.Trim(),
                    LastName = row.Cell(2).GetString()?.Trim(),
                    Email = email,
                    Phone = row.Cell(4).GetString()?.Trim(),

                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                // Only fields that belong to Candidate table
                ParseEnum<SourceTypeEnum>(row.Cell(8).GetString(),
                    value => candidate.SourceTypeId = value);

                ParseEnum<CandidateTypeEnum>(row.Cell(10).GetString(),
                    value => candidate.CandidateTypeId = value);

                await _candidatesService.InsertCandidateAsync(candidate);

                //CREATE JOB APPLICATION

                var application = new JobApplication
                {
                    CandidateId = candidate.Id,

                    PositionApplied = row.Cell(5).GetString()?.Trim(),
                    ExperienceYears = row.Cell(7).GetString()?.Trim(),

                    Url = row.Cell(13).GetString()?.Trim(),
                    CurrentCompany = row.Cell(15).GetString()?.Trim(),

                    AdditionalInformation = row.Cell(19).GetString()?.Trim(),
                    HrNotes = row.Cell(20).GetString()?.Trim(),
                    City = row.Cell(21).GetString()?.Trim(),
                    AppliedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                // ENUMS for JobApplication
                ParseEnum<JobPositionEnum>(row.Cell(6).GetString(),
                    value => application.PositionId = value);

                ParseEnum<CandidateStatusEnum>(row.Cell(9).GetString(),
                    value => application.StatusId = value);

                ParseEnum<RateTypeEnum>(row.Cell(11).GetString(),
                    value => application.RateTypeId = value);

                ParseEnum<LinkTypeEnum>(row.Cell(12).GetString(),
                    value => application.LinkTypeId = value);

                ParseEnum<NoticePeriodDaysEnum>(row.Cell(18).GetString(),
                    value => application.NoticePeriodId = value);

                // Decimal safe parsing
                if (decimal.TryParse(row.Cell(14).GetString(), out var amount))
                    application.Amount = amount;

                if (decimal.TryParse(row.Cell(16).GetString(), out var currentCtc))
                    application.CurrentCTC = currentCtc;

                if (decimal.TryParse(row.Cell(17).GetString(), out var expectedCtc))
                    application.ExpectedCTC = expectedCtc;

                await _candidatesService.InsertJobApplicationAsync(application);
            }
        }

        #endregion

        #region Candidate List/Create/Edit/Delete
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();
            var model = await _candidateModelFactory.PrepareCandidateSearchModelAsync(new CandidateSearchModel());

            return View("/Areas/Admin/Views/Extension/Candidate/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CandidateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            //prepare model
            var model = await _candidateModelFactory.PrepareCandidateListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            //prepare model
            var model = await _candidateModelFactory.PrepareCandidateModelAsync(new CandidateModel(), null);

            return View("/Areas/Admin/Views/Extension/Candidate/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CandidateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var entity = new Candidate
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone,
                    SourceTypeId = model.SourceTypeId,
                    CandidateTypeId = model.CandidateTypeId,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };


                await _candidatesService.InsertCandidateAsync(entity);
                var job = await _jobPostingService.GetJobPostingByIdAsync(model.JobPostingId);
                string positionApplied;
                int positionId;

                if (!string.IsNullOrEmpty(model.PositionApplied) && model.PositionId > 0)
                {
                    // Manual entry case
                    positionApplied = model.PositionApplied;
                    positionId = model.PositionId;
                }
                else
                {
                    // Job posting selected case
                    positionApplied = job?.Title;
                    positionId = job?.PositionId ?? 0;
                }
                var application = new JobApplication
                {
                    CandidateId = entity.Id,
                    JobPostingId = model.JobPostingId,

                    StatusId = model.StatusId > 0
                        ? model.StatusId
                        : (int)CandidateStatusEnum.Applied,

                    HrNotes = model.HrNotes,

                    PositionApplied = positionApplied,
                    PositionId = positionId,

                    ResumeDownloadId = model.ResumeDownloadId,

                    ExperienceYears = model.ExperienceYears,
                    CurrentCompany = model.CurrentCompany,
                    CurrentCTC = model.CurrentCTC,
                    ExpectedCTC = model.ExpectedCTC,
                    NoticePeriodId = model.NoticePeriodId,

                    RateTypeId = model.RateTypeId,
                    LinkTypeId = model.LinkTypeId,
                    Amount = model.Amount,
                    Url = model.Url,
                    City = model.City,
                    CoverLetter = model.CoverLetter,
                    Skills = model.Skills,
                    AdditionalInformation = model.AdditionalInformation,

                    AppliedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _candidatesService.InsertJobApplicationAsync(application);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Candidate.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }
            //prepare model
            model = await _candidateModelFactory.PrepareCandidateModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Candidate/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id, int jobPostingId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            var activity = await _candidatesService.GetCandidateByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            var model = new CandidateModel
            {
                JobPostingId = jobPostingId
            };

            //  STEP 2: Load correct job application inside factory
            model = await _candidateModelFactory.PrepareCandidateModelAsync(model, activity);
            return View("/Areas/Admin/Views/Extension/Candidate/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CandidateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            //try to get a candidate with the specified id
            var entity = await _candidatesService.GetCandidateByIdAsync(model.Id);

            if (entity == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                entity.FirstName = model.FirstName;
                entity.LastName = model.LastName;
                entity.Email = model.Email;
                entity.Phone = model.Phone;
                entity.SourceTypeId = model.SourceTypeId;
                entity.CandidateTypeId = model.CandidateTypeId;
                entity.CreatedOnUtc = model.CreatedOnUtc;
                entity.UpdatedOnUtc = DateTime.UtcNow;

                await _candidatesService.UpdateCandidateAsync(entity);
                var application = await _candidatesService
            .GetJobApplicationAsync(model.Id, model.JobPostingId);

                if (application != null)
                {
                    application.StatusId = model.StatusId;
                    application.HrNotes = model.HrNotes;
                    application.PositionApplied = model.PositionApplied;
                    application.PositionId = model.PositionId;
                    application.ResumeDownloadId = model.ResumeDownloadId;
                    application.ExperienceYears = model.ExperienceYears;
                    application.CurrentCompany = model.CurrentCompany;
                    application.CurrentCTC = model.CurrentCTC;
                    application.ExpectedCTC = model.ExpectedCTC;
                    application.NoticePeriodId = model.NoticePeriodId;

                    application.RateTypeId = model.RateTypeId;
                    application.LinkTypeId = model.LinkTypeId;
                    application.Amount = model.Amount;
                    application.Url = model.Url;
                    application.City = model.City;
                    application.CoverLetter = model.CoverLetter;
                    application.Skills = model.Skills;
                    application.AdditionalInformation = model.AdditionalInformation;

                    application.UpdatedOnUtc = DateTime.UtcNow;

                    await _candidatesService.UpdateJobApplicationAsync(application);
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Candidate.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }
            //prepare model
            model = await _candidateModelFactory.PrepareCandidateModelAsync(model, null, true);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Candidate/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var activity = await _candidatesService.GetCandidateByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            await _candidatesService.DeleteCandidateAsync(activity);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Candidate.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var candidates = await _candidatesService.GetCandidateByIdsAsync(selectedIds.ToArray());

            foreach (var candidate in candidates)
            {
                // DELETE JOB APPLICATIONS FIRST
                var applications = _jobApplicationRepository.Table
                    .Where(x => x.CandidateId == candidate.Id);

                foreach (var app in applications)
                {
                    await _jobApplicationRepository.DeleteAsync(app);
                }

                // THEN DELETE CANDIDATE
                await _candidatesService.DeleteCandidateAsync(candidate);
            }

            return Json(new { Result = true });
        }
        public async Task<IActionResult> DownloadResumes(int resumeDownloadId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandidates))
                return AccessDeniedView();

            if (resumeDownloadId <= 0)
                return NotFound();

            var download = await _downloadService.GetDownloadByIdAsync(resumeDownloadId);

            if (download == null)
                return NotFound();

            return File(download.DownloadBinary, download.ContentType, download.Filename);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkUpdateStatus(int[] candidateIds, int statusId)
        {
            if (candidateIds == null || candidateIds.Length == 0)
                return BadRequest();

            var jobApplications = await _jobApplicationRepository.Table
                .Where(x => candidateIds.Contains(x.CandidateId))
                .ToListAsync();

            foreach (var jobApp in jobApplications)
            {
                jobApp.StatusId = statusId;
                jobApp.UpdatedOnUtc = DateTime.UtcNow;

                await _jobApplicationRepository.UpdateAsync(jobApp);
            }

            return Json(new { success = true });
        }
        [HttpGet]
        public async Task<IActionResult> GetCandidateHistoryTooltip(int candidateId, int currentJobPostingId)
        {
            // Step 1: Get all applications for candidate
            var applications = await _jobApplicationRepository.Table
                .Where(x => x.CandidateId == candidateId)
                .OrderByDescending(x => x.AppliedOnUtc)
                .ToListAsync();

            if (!applications.Any())
                return Content("");

            // Step 2: Get latest application per JobPosting (in memory - SAFE)
            var latestApplications = applications
                .GroupBy(x => x.JobPostingId)
                .Select(g => g.First())   // already ordered desc
                .Where(x => x.JobPostingId != currentJobPostingId)
                .ToList();

            if (!latestApplications.Any())
                return Content("");

            var html = "";

            foreach (var app in latestApplications)
            {
                var job = await _jobPostingRepository.GetByIdAsync(app.JobPostingId);
                if (job == null) continue;

                var status = ((CandidateStatusEnum)app.StatusId).ToString();

                html += $@"
        <div style='margin-bottom:6px'>
            <b>Already applied on {job.Title}</b><br/>
            Status: {status}<br/>
            Applied: {app.AppliedOnUtc.ToLocalTime():dd MMM yyyy hh:mm tt}
        </div>
        <hr style='margin:4px 0;' />";
            }

            return Content(html);
        }
        [HttpGet]
        public async Task<IActionResult> GetCandidateSummary()
        {
            // STEP 1: Get latest application per candidate
            var latestApplications = await (
                from ja in _jobApplicationRepository.Table
                join c in _candidateRepository.Table
                    on ja.CandidateId equals c.Id
                group ja by ja.CandidateId into g
                select g.OrderByDescending(x => x.Id).FirstOrDefault()
            ).ToListAsync();

            // STEP 2: Status Summary (only valid candidates)
            var statusSummary = Enum.GetValues(typeof(CandidateStatusEnum))
                .Cast<CandidateStatusEnum>()
                .Where(e => e != CandidateStatusEnum.Select)
                .Select(status => new
                {
                    Id = (int)status,
                    Name = status.ToString(),
                    Count = latestApplications.Count(x => x.StatusId == (int)status),
                    Type = "Status"
                });

            // STEP 3: Candidate Type Summary
            var candidates = await _candidateRepository.Table.ToListAsync();

            var candidateTypeSummary = Enum.GetValues(typeof(CandidateTypeEnum))
                .Cast<CandidateTypeEnum>()
                .Where(e => e != CandidateTypeEnum.Select)
                .Select(type => new
                {
                    Id = (int)type,
                    Name = type.ToString(),
                    Count = candidates.Count(x => x.CandidateTypeId == (int)type),
                    Type = "CandidateType"
                });

            return Json(statusSummary.Concat(candidateTypeSummary));
        }
        private string GetEnumNameSafe<TEnum>(int? value) where TEnum : struct, Enum
        {
            if (!value.HasValue || value.Value == 0)
                return string.Empty;

            return Enum.IsDefined(typeof(TEnum), value.Value)
                ? Enum.GetName(typeof(TEnum), value.Value)
                : string.Empty;
        }

        [HttpPost, ActionName("ExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        public async Task<IActionResult> ExportToExcel(CandidateSearchModel searchModel)
        {
            var candidates = await _candidatesService.GetAllCandidatesAsync(
                searchModel.SearchName,
                searchModel.SearchEmail,
                searchModel.SearchPositionApplied,
                searchModel.SearchCandidateTypeId,
                searchModel.SearchStatusId,
                0, int.MaxValue);

            var dtoList = new List<CandidateDto>();

            foreach (var candidate in candidates)
            {
                var application = await _jobApplicationRepository.Table
                    .Where(x => x.CandidateId == candidate.Id)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                dtoList.Add(new CandidateDto
                {
                    FirstName = candidate.FirstName,
                    LastName = candidate.LastName,
                    Email = candidate.Email,
                    Phone = candidate.Phone,

                    Source = GetEnumNameSafe<SourceTypeEnum>(candidate.SourceTypeId),
                    CandidateType = GetEnumNameSafe<CandidateTypeEnum>(candidate.CandidateTypeId),

                    PositionApplied = application?.PositionApplied,
                    Position = GetEnumNameSafe<JobPositionEnum>(application.PositionId),

                    ExperienceYears = application?.ExperienceYears,

                    Status = GetEnumNameSafe<CandidateStatusEnum>(application.StatusId),
                    RateType = GetEnumNameSafe<RateTypeEnum>(application.RateTypeId ?? 0),
                    LinkType = GetEnumNameSafe<LinkTypeEnum>(application.LinkTypeId),

                    Url = application?.Url,
                    Amount = application?.Amount,
                    CurrentCompany = application?.CurrentCompany,
                    CurrentCTC = application?.CurrentCTC,
                    ExpectedCTC = application?.ExpectedCTC,
                    City = application.City,
                    NoticePeriod = GetEnumNameSafe<NoticePeriodDaysEnum>(application.NoticePeriodId ?? 0),

                    AdditionalInformation = application?.AdditionalInformation,
                    HrNotes = application?.HrNotes,

                    CreatedOnUtc = candidate.CreatedOnUtc,
                    UpdatedOnUtc = candidate.UpdatedOnUtc
                });
            }

            var bytes = await ExportCandidatesToExcelAsync(dtoList);

            return File(bytes, MimeTypes.TextXlsx, "Candidates.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> SelectedExportToExcel(List<int> selectedIds)
        {
            var candidates = await _candidateRepository.Table
                .Where(x => selectedIds.Contains(x.Id))
                .ToListAsync();

            var dtoList = new List<CandidateDto>();

            foreach (var candidate in candidates)
            {
                var application = await _jobApplicationRepository.Table
                    .Where(x => x.CandidateId == candidate.Id)
                    .OrderByDescending(x => x.Id)
                    .FirstOrDefaultAsync();

                dtoList.Add(new CandidateDto
                {
                    FirstName = candidate.FirstName,
                    LastName = candidate.LastName,
                    Email = candidate.Email,
                    Phone = candidate.Phone,

                    Source = GetEnumNameSafe<SourceTypeEnum>(candidate.SourceTypeId),
                    CandidateType = GetEnumNameSafe<CandidateTypeEnum>(candidate.CandidateTypeId),

                    PositionApplied = application?.PositionApplied,
                    Position = GetEnumNameSafe<JobPositionEnum>(application.PositionId),

                    ExperienceYears = application?.ExperienceYears,

                    Status = GetEnumNameSafe<CandidateStatusEnum>(application.StatusId),
                    RateType = GetEnumNameSafe<RateTypeEnum>(application.RateTypeId ?? 0),
                    LinkType = GetEnumNameSafe<LinkTypeEnum>(application.LinkTypeId),

                    Url = application?.Url,
                    Amount = application?.Amount,
                    CurrentCompany = application?.CurrentCompany,
                    CurrentCTC = application?.CurrentCTC,
                    ExpectedCTC = application?.ExpectedCTC,

                    NoticePeriod = GetEnumNameSafe<NoticePeriodDaysEnum>(application.NoticePeriodId ?? 0),

                    AdditionalInformation = application?.AdditionalInformation,
                    HrNotes = application?.HrNotes,
                    City = application.City,
                    CreatedOnUtc = candidate.CreatedOnUtc,
                    UpdatedOnUtc = candidate.UpdatedOnUtc
                });
            }

            var bytes = await ExportCandidatesToExcelAsync(dtoList);

            return File(bytes, MimeTypes.TextXlsx, "Selected Candidates.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile importFile)
        {
            await ImportCandidatesFromExcelAsync(importFile);

            _notificationService.SuccessNotification("Candidates imported successfully");

            return RedirectToAction("List");
        }

        #endregion
    }
}