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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Titles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class TitleController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ITitleService _titleService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public TitleController(IPermissionService permissionService,
                               ITitleService titleService,
                               INotificationService notificationService,
                               ILocalizationService localizationService)
        {
            _permissionService = permissionService;
            _titleService = titleService;
            _notificationService = notificationService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        public virtual async Task<TitleSearchModel> PrepareTitleSearchModelAsync(TitleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<TitleListModel> PrepareTitleListModelAsync(TitleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get title
            var titles = await _titleService.GetAllTitleAsync(showHidden: true,
                name: searchModel.SearchTitleName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new TitleListModel().PrepareToGridAsync(searchModel, titles, () =>
            {
                //fill in model values from the entity
                return titles.SelectAwait(async titles =>
                {
                    var titleModel = new TitleModel();
                    titleModel.Id = titles.Id;
                    titleModel.Name = titles.Name;

                    return titleModel;
                });
            });
            return model;
        }

        public virtual async Task<TitleModel> PrepareTitleModelAsync(TitleModel model, Title title, bool excludeProperties = false)
        {
            if (title != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new TitleModel();
                    model.Id = title.Id;
                    model.Name = title.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareTitleSearchModelAsync(new TitleSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Titles/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(TitleSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareTitleListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareTitleModelAsync(new TitleModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Titles/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(TitleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                if (await _titleService.TitleExistsAsync(model.Name))
                {
                    ModelState.AddModelError(nameof(model.Name), "This title already exists.");
                }
                else
                {
                    var title = new Title
                    {
                        Name = model.Name
                    };

                    await _titleService.InsertTitleAsync(title);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Title.Added"));

                    //ViewBag.RefreshPage = true;

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = title.Id });
                }
            }

            //prepare model
            model = await PrepareTitleModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Titles/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a title with the specified id
            var title = await _titleService.GetTitleByIdAsync(id);
            if (title == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareTitleModelAsync(null, title);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Titles/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(TitleModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a title with the specified id
            var title = await _titleService.GetTitleByIdAsync(model.Id);
            if (title == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                if (await _titleService.TitleExistsAsync(model.Name, model.Id))
                {
                    ModelState.AddModelError(nameof(model.Name), "This title already exists.");
                }
                else
                {
                    title.Name = model.Name;

                    await _titleService.UpdateTitleAsync(title);

                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Title.Updated"));

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("Edit", new { id = title.Id });
                    //ViewBag.RefreshPage = true;
                }
            }

            //prepare model
            model = await PrepareTitleModelAsync(model, title, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Titles/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _titleService.GetTitleByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _titleService.DeleteTitleAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
