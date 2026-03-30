using App.Core.Domain.JobPostings;
using App.Data;
using App.Data.Extensions;
using App.Services;
using App.Services.Helpers;
using App.Services.JobPostings;
using App.Web.Areas.Admin.Models.Extension.Candidates;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Candidate model factory implementation
    /// </summary>
    public partial class CandidateModelFactory : ICandidateModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICandidatesService _candidatesService;
        private readonly IJobPostingService _jobPostingService;
        private readonly IRepository<Candidate> _candidateRepository;
        private readonly IRepository<JobApplication> _jobApplicationRepository;
        #endregion

        #region Ctor

        public CandidateModelFactory(IDateTimeHelper dateTimeHelper,
            ICandidatesService candidatesService,
            IJobPostingService jobPostingService,
            IRepository<Candidate> candidateRepository,
            IRepository<JobApplication> jobApplicationRepository)
        {
            _dateTimeHelper = dateTimeHelper;
            _candidatesService = candidatesService;
            _jobPostingService = jobPostingService;
            _candidateRepository = candidateRepository;
            _jobApplicationRepository = jobApplicationRepository;
        }

        #endregion

        #region Methods
        private string GetEnumDescription(Enum value)
        {
            FieldInfo field = value.GetType().GetField(value.ToString());

            DescriptionAttribute attribute =
                Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute))
                as DescriptionAttribute;

            return attribute == null ? value.ToString() : attribute.Description;
        }
        public Task<CandidateSearchModel> PrepareCandidateSearchModelAsync(CandidateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Status = Enum.GetValues(typeof(CandidateStatusEnum))
                                .Cast<CandidateStatusEnum>()
                                .Select(x => new SelectListItem
                                {
                                    Text = x.ToString(),
                                    Value = ((int)x).ToString()
                                }).ToList();
            searchModel.CandidateTypes = Enum.GetValues(typeof(CandidateTypeEnum))
                                        .Cast<CandidateTypeEnum>()
                                        .Select(x => new SelectListItem
                                        {
                                            Text = x.ToString(),
                                            Value = ((int)x).ToString()
                                        }).ToList();

            searchModel.SetGridPageSize();
            return Task.FromResult(searchModel);
        }

        public virtual async Task<CandidateListModel> PrepareCandidateListModelAsync(CandidateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get paged data using search filters
            var candidates = await _candidatesService.GetAllCandidatesAsync(searchModel.SearchName, searchModel.SearchEmail,
                searchModel.SearchPositionApplied, searchModel.SearchCandidateTypeId, searchModel.SearchStatusId,
                searchModel.Page - 1, searchModel.PageSize);

            var model = await ModelExtensions.PrepareToGridAsync<CandidateListModel, CandidateModel, Candidate>
                (new CandidateListModel(), searchModel, candidates, () =>
                {
                    return candidates.SelectAwait(async candidate =>
                    {
                        // get latest job application
                        var application = await _jobApplicationRepository.Table
                            .Where(x => x.CandidateId == candidate.Id)
                            .OrderBy(x => x.Id)
                            .FirstOrDefaultAsync();

                        string positionName = "";

                        if (application != null && application.PositionId > 0)
                            positionName = Enum.GetName(typeof(JobPositionEnum), application.PositionId);

                        return new CandidateModel
                        {
                            Id = candidate.Id,

                            FirstName = candidate.FirstName,
                            LastName = candidate.LastName,
                            Name = candidate.FirstName + " " + candidate.LastName,

                            Email = candidate.Email,
                            Phone = candidate.Phone,

                            // safe null handling
                            PositionApplied = application?.PositionApplied,
                            PositionId = application?.PositionId ?? 0,
                            Position = positionName,

                            ResumeDownloadId = application?.ResumeDownloadId ?? 0,

                            ExperienceYears = application?.ExperienceYears,
                            CurrentCompany = application?.CurrentCompany,
                            CurrentCTC = application?.CurrentCTC,
                            ExpectedCTC = application?.ExpectedCTC,

                            NoticePeriodId = application?.NoticePeriodId ?? 0,
                            RateTypeId = application?.RateTypeId ?? 0,

                            LinkTypeId = application?.LinkTypeId ?? 0,
                            Amount = application?.Amount,
                            Url = application?.Url,

                            CoverLetter = application?.CoverLetter,
                            Skills = application?.Skills,
                            AdditionalInformation = application?.AdditionalInformation,

                            StatusId = application?.StatusId ?? 0,
                            Status = application != null
                                ? Enum.GetName(typeof(CandidateStatusEnum), application.StatusId)
                                : "",

                            CandidateTypeId = candidate.CandidateTypeId,
                            CandidateTypeName = Enum.GetName(typeof(CandidateTypeEnum), candidate.CandidateTypeId),

                            SourceTypeId = candidate.SourceTypeId,
                            SourceTypeName = Enum.GetName(typeof(SourceTypeEnum), candidate.SourceTypeId),

                            HrNotes = application?.HrNotes,

                            JobPostingId = application?.JobPostingId ?? 0,
                            City = application.City,
                            CreatedOnUtc = application != null
                                ? await _dateTimeHelper.ConvertToUserTimeAsync(application.AppliedOnUtc, DateTimeKind.Utc)
                                : DateTime.MinValue,

                            UpdatedOnUtc = application != null
                                ? await _dateTimeHelper.ConvertToUserTimeAsync(application.UpdatedOnUtc, DateTimeKind.Utc)
                                : DateTime.MinValue
                        };
                    });
                });

            return model;
        }

        public async Task<CandidateModel> PrepareCandidateModelAsync(CandidateModel model, Candidate candidate, bool excludeProperties = false)
        {
            var candidatesStatus = await CandidateStatusEnum.Select.ToSelectListAsync();
            var candidatesSource = await SourceTypeEnum.Select.ToSelectListAsync();
            var candidatesLinkType = await LinkTypeEnum.Select.ToSelectListAsync();
            var candidatesRateType = await RateTypeEnum.Select.ToSelectListAsync();
            var candidatesTypeName = await CandidateTypeEnum.Select.ToSelectListAsync();
            var candidateNoticePeriod = Enum.GetValues(typeof(NoticePeriodDaysEnum))
    .Cast<NoticePeriodDaysEnum>()
    .Select(x => new SelectListItem
    {
        Value = ((int)x).ToString(),
        Text = GetEnumDescription(x)
    }).ToList();
            var candidatePosition = await JobPositionEnum.Select.ToSelectListAsync();
            if (candidate != null)
            {
                model ??= new CandidateModel();

                model.Id = candidate.Id;
                model.FirstName = candidate.FirstName;
                model.LastName = candidate.LastName;
                model.Name = candidate.FirstName + " " + candidate.LastName;
                model.Email = candidate.Email;
                model.Phone = candidate.Phone;

                JobApplication application = null;

                // ✅ CASE 1: When JobPostingId is passed (from Job Posting page)
                if (model.JobPostingId > 0)
                {
                    application = await _jobApplicationRepository.Table
                        .FirstOrDefaultAsync(x =>
                            x.CandidateId == candidate.Id &&
                            x.JobPostingId == model.JobPostingId);
                }
                else
                {
                    // ✅ CASE 2: From Candidate list (jobPostingId = 0)
                    // fallback → get latest application
                    application = await _jobApplicationRepository.Table
                        .Where(x => x.CandidateId == candidate.Id)
                        .OrderByDescending(x => x.AppliedOnUtc)
                        .FirstOrDefaultAsync();
                }

                if (application != null)
                {
                    model.StatusId = application.StatusId;
                    model.JobPostingId = application.JobPostingId;
                    model.HrNotes = application.HrNotes;

                    model.PositionApplied = application.PositionApplied;
                    model.PositionId = application.PositionId;

                    model.ResumeDownloadId = application.ResumeDownloadId ?? 0;

                    model.ExperienceYears = application.ExperienceYears;
                    model.CurrentCompany = application.CurrentCompany;
                    model.CurrentCTC = application.CurrentCTC;
                    model.ExpectedCTC = application.ExpectedCTC;
                    model.NoticePeriodId = application.NoticePeriodId ?? 0;

                    model.RateTypeId = application.RateTypeId ?? 0;
                    model.LinkTypeId = application.LinkTypeId;
                    model.Amount = application.Amount;
                    model.Url = application.Url;
                    model.City = application.City;
                    model.CoverLetter = application.CoverLetter;
                    model.Skills = application.Skills;
                    model.AdditionalInformation = application.AdditionalInformation;
                }

                model.SourceTypeId = candidate.SourceTypeId;
                model.CandidateTypeId = candidate.CandidateTypeId;

                model.CreatedOnUtc = candidate.CreatedOnUtc;
                model.UpdatedOnUtc = candidate.UpdatedOnUtc;
            }

            model.StatusList = candidatesStatus.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.StatusId.ToString()
            }).ToList();

            model.SourceTypeList = candidatesSource.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.SourceTypeId.ToString()
            }).ToList();

            model.LinkTypeList = candidatesLinkType.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.LinkTypeId.ToString()
            }).ToList();

            model.RateTypeList = candidatesRateType.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.RateTypeId.ToString()
            }).ToList();

            model.AvailableCandidateTypes = candidatesTypeName.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.CandidateTypeId.ToString()
            }).ToList();

            model.AvailableNoticePeriodDays = candidateNoticePeriod.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.NoticePeriodId.ToString()
            }).ToList();

            model.AvailablePosition = candidatePosition.Select(x => new SelectListItem
            {
                Value = x.Value,
                Text = x.Text,
                Selected = x.Value == model.PositionId.ToString()
            }).ToList();
            return model;
        }
        #endregion
    }
}


