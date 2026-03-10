using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Extension.SkillSet;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class SkillSetController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ISkillSetService _skillSetService;
        private readonly ISkillSetModelFactory _skillSetModelFactory;
        private readonly ITechnologySkillMappingService _technologySkillMappingService;
        #endregion

        #region Ctor

        public SkillSetController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ISkillSetService skillSetService,
            ISkillSetModelFactory skillSetModelFactory,
            ITechnologySkillMappingService technologySkillMappingService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _skillSetService = skillSetService;
            _skillSetModelFactory = skillSetModelFactory;
            _technologySkillMappingService = technologySkillMappingService;
        }

        #endregion

        #region Utilities



        #endregion

        #region Methods 
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();

            var model = await _skillSetModelFactory.PrepareSkillSetSearchModelAsync(new SkillSetSearchModel());

            return View("~/Areas/Admin/Views/Extension/SkillSet/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(SkillSetSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();

            var model = await _skillSetModelFactory.PrepareSkillSetListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();

            //prepare model
            var model = await _skillSetModelFactory.PrepareSkillSetModelAsync(new SkillSetModel(), null);

            return View("~/Areas/Admin/Views/Extension/SkillSet/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(SkillSetModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();
            if (model.SelectedTechnologyIds == null || !model.SelectedTechnologyIds.Any())
            {
                ModelState.AddModelError(nameof(model.SelectedTechnologyIds), "Please select at least one technology");
            }
            if (string.IsNullOrWhiteSpace(model.SkillTags))
            {
                ModelState.AddModelError(nameof(model.SkillTags), "Please enter at least one skill");
            }
            if (ModelState.IsValid)
            {
                var skillNames = model.SkillTags
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Distinct()
            .ToList();

                int lastInsertedSkillId = 0;

                foreach (var skillName in skillNames)
                {
                    var entity = new SkillSet
                    {
                        Name = skillName,
                        DisplayOrder = model.DisplayOrder,
                        Published = model.Published,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _skillSetService.InsertSkillAsync(entity);

                    lastInsertedSkillId = entity.Id;

                    foreach (var techId in model.SelectedTechnologyIds)
                    {
                        await _technologySkillMappingService.InsertAsync(
                            new TechnologySkillMapping
                            {
                                SkillSetId = entity.Id,
                                TechnologyId = techId
                            });
                    }
                }
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.SkillSet.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");
                return RedirectToAction("Edit", new { id = lastInsertedSkillId });
            }
            model = await _skillSetModelFactory.PrepareSkillSetModelAsync(model, null, true);

            return View("~/Areas/Admin/Views/Extension/SkillSet/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();

            var entity = await _skillSetService.GetSkillByIdAsync(id);
            if (entity == null)
                return RedirectToAction("List");

            var model = await _skillSetModelFactory.PrepareSkillSetModelAsync(null, entity);

            return View("~/Areas/Admin/Views/Extension/SkillSet/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(SkillSetModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();

            // Validation
            if (model.SelectedTechnologyIds == null || !model.SelectedTechnologyIds.Any())
                ModelState.AddModelError(nameof(model.SelectedTechnologyIds), "Please select at least one technology");

            if (string.IsNullOrWhiteSpace(model.SkillTags))
                ModelState.AddModelError(nameof(model.SkillTags), "Please enter at least one skill");

            if (!ModelState.IsValid)
            {
                model = await _skillSetModelFactory.PrepareSkillSetModelAsync(model, null, true);
                return View("~/Areas/Admin/Views/Extension/SkillSet/Edit.cshtml", model);
            }

            // Prepare new skill names
            var newSkillNames = model.SkillTags
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var techIds = model.SelectedTechnologyIds
                .OrderBy(x => x)
                .ToList();

            // STEP 1: Get ALL existing skills
            var allSkills = await _skillSetService.GetAllSkillsAsync();

            // STEP 2: Find existing skills by TechnologyIds ONLY
            var existingSkills = new List<SkillSet>();

            foreach (var skill in allSkills)
            {
                var mappings = await _technologySkillMappingService.GetBySkillSetIdAsync(skill.Id);

                var skillTechIds = mappings
                    .Select(x => x.TechnologyId)
                    .OrderBy(x => x)
                    .ToList();

                if (skillTechIds.SequenceEqual(techIds))
                    existingSkills.Add(skill);
            }

            var existingNames = existingSkills
                .Select(x => x.Name)
                .ToList();

            // STEP 3: DELETE removed skills
            var skillsToDelete = existingSkills
                .Where(x => !newSkillNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var skill in skillsToDelete)
            {
                await _technologySkillMappingService.DeleteBySkillSetIdAsync(skill.Id);
                await _skillSetService.DeleteSkillAsync(skill);
            }

            // STEP 4: INSERT new skills
            var skillsToInsert = newSkillNames
                .Where(x => !existingNames.Contains(x, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var skillName in skillsToInsert)
            {
                var entity = new SkillSet
                {
                    Name = skillName,
                    DisplayOrder = model.DisplayOrder,
                    Published = model.Published,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow
                };

                await _skillSetService.InsertSkillAsync(entity);

                foreach (var techId in techIds)
                {
                    await _technologySkillMappingService.InsertAsync(
                        new TechnologySkillMapping
                        {
                            SkillSetId = entity.Id,
                            TechnologyId = techId
                        });
                }
            }

            // STEP 5: UPDATE existing skills
            var skillsToUpdate = existingSkills
                .Where(x => newSkillNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var skill in skillsToUpdate)
            {
                skill.DisplayOrder = model.DisplayOrder;
                skill.Published = model.Published;
                skill.UpdatedOnUtc = DateTime.UtcNow;

                await _skillSetService.UpdateSkillAsync(skill);
            }

            // Success notification
            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.SkillSet.Updated"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = model.Id });
        }


        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSkillSet))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _skillSetService.GetSkillByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _technologySkillMappingService.DeleteBySkillSetIdAsync(item.Id);
                await _skillSetService.DeleteSkillAsync(item);
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}