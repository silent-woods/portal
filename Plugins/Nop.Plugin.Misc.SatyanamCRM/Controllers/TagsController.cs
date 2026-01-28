using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class TagsController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ITagsService _tagsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public TagsController(IPermissionService permissionService,
                               ITagsService tagsService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _tagsService = tagsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        public virtual async Task<TagsSearchModel> PrepareTagsSearchModelAsync(TagsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<TagsListModel> PrepareTagsListModelAsync(TagsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get tags
            var tags = await _tagsService.GetAllTagsAsync(showHidden: true,
                name: searchModel.SearchTagsName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new TagsListModel().PrepareToGridAsync(searchModel, tags, () =>
            {
                //fill in model values from the entity
                return tags.SelectAwait(async tags =>
                {
                    var tagsModel = new TagsModel();
                    tagsModel.Id = tags.Id;
                    tagsModel.Name = tags.Name;

                    return tagsModel;
                });
            });
            return model;
        }

        public virtual async Task<TagsModel> PrepareTagsModelAsync(TagsModel model, Tags tags, bool excludeProperties = false)
        {
            if (tags != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new TagsModel();
                    model.Id = tags.Id;
                    model.Name = tags.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareTagsSearchModelAsync(new TagsSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Tags/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(TagsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareTagsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareTagsModelAsync(new TagsModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Tags/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(TagsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var tags = new Tags();
                tags.Id = model.Id;
                tags.Name = model.Name;

                await _tagsService.InsertTagsAsync(tags);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Tags.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = tags.Id });
            }

            //prepare model
            model = await PrepareTagsModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Tags/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a tags with the specified id
            var tags = await _tagsService.GetTagsByIdAsync(id);
            if (tags == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareTagsModelAsync(null, tags);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Tags/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(TagsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a tags with the specified id
            var tags = await _tagsService.GetTagsByIdAsync(model.Id);
            if (tags == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                tags = new Tags();
                tags.Id = model.Id;
                tags.Name = model.Name;

                await _tagsService.UpdateTagsAsync(tags);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Tags.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = tags.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareTagsModelAsync(model, tags, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Tags/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTags, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _tagsService.GetTagsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _tagsService.DeleteTagsAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
