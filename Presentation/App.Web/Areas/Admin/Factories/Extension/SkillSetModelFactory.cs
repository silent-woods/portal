using App.Core;
using App.Data.Extensions;
using App.Services.Helpers;
using App.Web.Areas.Admin.Models.Extension.SkillSet;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the SkillSet  model factory implementation
    /// </summary>
    public partial class SkillSetModelFactory : ISkillSetModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ISkillSetService _skillSetService;
        private readonly ITechnologyService _technologyService;
        private readonly ITechnologySkillMappingService _technologySkillMappingService;
        #endregion

        #region Ctor

        public SkillSetModelFactory(IDateTimeHelper dateTimeHelper,
                                    ISkillSetService skillSetService,
                                    ITechnologyService technologyService,
                                    ITechnologySkillMappingService technologySkillMappingService)
        {
            _dateTimeHelper = dateTimeHelper;
            _skillSetService = skillSetService;
            _technologyService = technologyService;
            _technologySkillMappingService = technologySkillMappingService;
        }

        #endregion

        #region Methods
        public async Task<SkillSetSearchModel> PrepareSkillSetSearchModelAsync(SkillSetSearchModel searchModel)
        {
            searchModel ??= new SkillSetSearchModel();

            var technologies = await _technologyService.GetAllPublishedTechnologiesAsync();

            searchModel.AvailableTechnologies = technologies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public async Task<SkillSetListModel> PrepareSkillSetListModelAsync(SkillSetSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var skills = await _skillSetService.GetAllSkillsAsync(searchModel.SearchName, null, 0, int.MaxValue);

            var allTechnologies = await _technologyService.GetAllPublishedTechnologiesAsync();

            var rawList = new List<(SkillSet Skill, List<int> TechIds)>();

            foreach (var skill in skills)
            {
                var mappings = await _technologySkillMappingService.GetBySkillSetIdAsync(skill.Id);

                rawList.Add((skill, mappings.Select(x => x.TechnologyId).OrderBy(x => x).ToList()));
            }

            var grouped = rawList
                .GroupBy(x => new
                {
                    TechKey = string.Join(",", x.TechIds),
                    x.Skill.DisplayOrder
                }).ToList();

            var groupedModels = new List<SkillSetModel>();

            foreach (var group in grouped)
            {
                var first = group.First();

                var techNames = string.Join(", ",
                    allTechnologies
                        .Where(t => first.TechIds.Contains(t.Id))
                        .Select(t => t.Name));

                var skillNames = string.Join(", ",
                    group.Select(x => x.Skill.Name).Distinct());

                var entity = first.Skill;

                groupedModels.Add(new SkillSetModel
                {
                    Id = entity.Id,
                    Name = skillNames,
                    TechnologyName = techNames,
                    DisplayOrder = entity.DisplayOrder,
                    Published = entity.Published,
                    CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(entity.CreatedOnUtc, DateTimeKind.Utc),
                    UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(entity.UpdatedOnUtc, DateTimeKind.Utc)
                });
            }

            // convert to paged list
            var pagedList = new PagedList<SkillSetModel>(
                groupedModels,
                searchModel.Page - 1,
                searchModel.PageSize);

            // correct PrepareToGridAsync usage
            var model = await ModelExtensions.PrepareToGridAsync<SkillSetListModel, SkillSetModel, SkillSetModel>(
                new SkillSetListModel(),
                searchModel,
                pagedList,
                () => pagedList.SelectAwait(x => new ValueTask<SkillSetModel>(x)));

            return model;
        }


        public async Task<SkillSetModel> PrepareSkillSetModelAsync(SkillSetModel model, SkillSet entity, bool excludeProperties = false)
        {
            model ??= new SkillSetModel();
            model.SelectedTechnologyIds ??= new List<int>();

            if (entity != null)
            {
                // Step 1: get technologies of current skill
                var mappings = await _technologySkillMappingService.GetBySkillSetIdAsync(entity.Id);

                var techIds = mappings.Select(x => x.TechnologyId).Distinct().ToList();

                // Step 2: get all skills
                var allSkills = await _skillSetService.GetAllSkillsAsync();

                // Step 3: filter skills belonging to SAME GROUP
                var groupedSkills = new List<SkillSet>();

                foreach (var skill in allSkills)
                {
                    if (skill.DisplayOrder != entity.DisplayOrder)
                        continue;

                    var skillMappings = await _technologySkillMappingService.GetBySkillSetIdAsync(skill.Id);

                    var skillTechIds = skillMappings.Select(x => x.TechnologyId).OrderBy(x => x);

                    if (skillTechIds.SequenceEqual(techIds.OrderBy(x => x)))
                    {
                        groupedSkills.Add(skill);
                    }
                }

                // Step 4: assign SkillTags
                model.SkillTags = string.Join(", ", groupedSkills
                    .Select(x => x.Name)
                    .Distinct());

                // Step 5: assign other fields
                model.Id = entity.Id;
                model.DisplayOrder = entity.DisplayOrder;
                model.Published = entity.Published;
                model.CreatedOnUtc = entity.CreatedOnUtc;
                model.UpdatedOnUtc = entity.UpdatedOnUtc;
                model.SelectedTechnologyIds = techIds;
            }

            // Load available technologies
            var technologies = await _technologyService.GetAllPublishedTechnologiesAsync();

            model.AvailableTechnologies = technologies.Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = model.SelectedTechnologyIds.Contains(x.Id)
            }).ToList();

            return model;
        }
        #endregion
    }
}


