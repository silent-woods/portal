using App.Core.Domain.JobPostings;
using App.Data.Extensions;
using App.Services;
using App.Services.Helpers;
using App.Services.JobPostings;
using App.Services.Localization;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.JobPostings;
using App.Web.Areas.Admin.Models.Extension.Candidates;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    ///Represents the JobPosting model factory implementation
    /// </summary>
    public partial class JobPostingsModelFactory : IJobPostingsModelFactory
    {
        #region Fields

        private readonly IJobPostingService _jobPostingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ITechnologyService _technologyService;
        private readonly ISkillSetService _skillSetService;
        private readonly ITechnologySkillMappingService _technologySkillMappingService;
        private readonly IJobPostingSkillMappingService _jobPostingSkillMappingService;
        private readonly IJobPostingTechnologyMappingService _jobPostingTechnologyMappingService;
        #endregion

        #region Ctor

        public JobPostingsModelFactory(IJobPostingService jobPostingService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            ITechnologyService technologyService,
            ISkillSetService skillSetService,
            ITechnologySkillMappingService technologySkillMappingService,
            IJobPostingSkillMappingService jobPostingSkillMappingService,
            IJobPostingTechnologyMappingService jobPostingTechnologyMappingService)
        {
            _jobPostingService = jobPostingService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _technologyService = technologyService;
            _skillSetService = skillSetService;
            _technologySkillMappingService = technologySkillMappingService;
            _jobPostingSkillMappingService = jobPostingSkillMappingService;
            _jobPostingTechnologyMappingService = jobPostingTechnologyMappingService;
        }

        #endregion

        #region Utilities
        #endregion

        #region Methods

        public virtual async Task<JobPostingSearchModel> PrepareJobPostingsSearchModelAsync(JobPostingSearchModel searchModel)
        {
            var questiontype = await JobPositionEnum.Select.ToSelectListAsync();
            searchModel.AvailablePosition = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.PositionId.ToString() == store.Value
            }).ToList();
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<JobPostingListModel> PrepareJobPostingsListModelAsync
          (JobPostingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get timesheet
            var timeSheet = await _jobPostingService.GetAllJobPostingAsync(tittle: searchModel.Title, positionid: searchModel.PositionId,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new JobPostingListModel().PrepareToGridAsync(searchModel, timeSheet, () =>
            {
                return timeSheet.SelectAwait(async timesheet =>
                {
                    //fill in model values from the entity
                    var selectedAvailablePosition = timesheet.PositionId;
                    var model = timesheet.ToModel<JobPostingModel>();
                    model.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.CreatedOn, DateTimeKind.Utc);
                    model.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(timesheet.UpdatedOn, DateTimeKind.Utc);
                    model.Position = ((JobPositionEnum)selectedAvailablePosition).ToString();
                    model.CandidateType = ((CandidateTypeEnum)timesheet.CandidateTypeId).ToString();
                    return model;
                });
            });

            //prepare grid model
            return model;
        }

        public virtual async Task<JobPostingModel> PrepareJobPostingsModelAsync(JobPostingModel model, JobPosting jobPosting, bool excludeProperties = false)
        {
            var questiontype = await JobPositionEnum.Select.ToSelectListAsync();
            var candidateTypes = await CandidateTypeEnum.Select.ToSelectListAsync();
            var candidateStatus = await CandidateStatusEnum.Select.ToSelectListAsync();

            // IMPORTANT: Initialize model
            model ??= new JobPostingModel();

            if (jobPosting != null)
            {
                if (!excludeProperties)
                {
                    model = jobPosting.ToModel<JobPostingModel>();
                }
                var skillMappings = await _jobPostingSkillMappingService.GetByJobPostingIdAsync(jobPosting.Id);

                model.SelectedSkillIds = skillMappings
                    .Select(x => x.SkillSetId)
                    .ToList();
                var techMappings = await _jobPostingTechnologyMappingService.GetByJobPostingIdAsync(jobPosting.Id);

                model.SelectedTechnologyIds = techMappings
                    .Select(x => x.TechnologyId)
                    .ToList();
            }

            // IMPORTANT: Initialize nested models (THIS IS YOUR FIX)
            model.jobPostingCandidateSearchModel ??= new JobPostingCandidateSearchModel();
            model.InvitationCandidateSearchModel ??= new InvitationCandidateSearchModel();
            model.CandidateSearchModel ??= new CandidateSearchModel();

            // VERY IMPORTANT: set JobPostingId
            model.jobPostingCandidateSearchModel.JobPostingId = jobPosting?.Id ?? 0;
            model.InvitationCandidateSearchModel.JobPostingId = jobPosting?.Id ?? 0;
            model.CandidateSearchModel.JobPostingId = jobPosting?.Id ?? 0;

            // Positions dropdown
            model.AvailablePosition = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.PositionId.ToString() == store.Value
            }).ToList();

            // CandidateTypes dropdown
            model.AvailableCandidateTypes = candidateTypes.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.CandidateTypeId.ToString() == store.Value
            }).ToList();
            var technologies = await _technologyService.GetAllPublishedTechnologiesAsync();

            model.AvailableTechnologies = technologies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = model.SelectedTechnologyIds.Contains(x.Id)
            }).ToList();
            var skills = await _skillSetService.GetAllSkillsAsync();

            model.AvailableSkills = skills.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = model.SelectedSkillIds.Contains(x.Id)
            }).ToList();

            // Candidate search model
            model.CandidateSearchModel = new CandidateSearchModel
            {
                JobPostingId = jobPosting?.Id ?? 0
            };

            // IMPORTANT FIX: now safe to access nested model
            model.jobPostingCandidateSearchModel.AvailableCandidateTypes = candidateTypes.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.jobPostingCandidateSearchModel.CandidateTypeId.ToString() == store.Value
            }).ToList();

            model.jobPostingCandidateSearchModel.AvailableStatuses = candidateStatus.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.jobPostingCandidateSearchModel.StatusId.ToString() == store.Value
            }).ToList();

            return model;
        }

        #endregion
    }
}


