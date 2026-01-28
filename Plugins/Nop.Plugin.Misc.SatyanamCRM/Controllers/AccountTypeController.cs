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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.AccountTypes;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.SatyanamCRM.Models.AccountTypes;
using App.Core.Domain.Security;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class AccountTypeController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IAccountTypeService _accountTypeService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor 

        public AccountTypeController(IPermissionService permissionService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               IAccountTypeService accountTypeService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _accountTypeService = accountTypeService;
        }

        #endregion

        #region Utilities

        public virtual async Task<AccountTypeSearchModel> PrepareAccountTypeSearchModelAsync(AccountTypeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<AccountTypeListModel> PrepareAccountTypeListModelAsync(AccountTypeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get accountType
            var accountType = await _accountTypeService.GetAllIAccountTypeAsync(showHidden: true,
                name: searchModel.SearchAccountTypeName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new AccountTypeListModel().PrepareToGridAsync(searchModel, accountType, () =>
            {
                //fill in model values from the entity
                return accountType.SelectAwait(async acountTypes =>
                {
                    var accountTypeModel = new AccountTypeModel();
                    accountTypeModel.Id = acountTypes.Id;
                    accountTypeModel.Name = acountTypes.Name;

                    return accountTypeModel;
                });
            });
            return model;
        }

        public virtual async Task<AccountTypeModel> PrepareAccountTypeModelAsync(AccountTypeModel model, AccountType accountType, bool excludeProperties = false)
        {
            if (accountType != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new AccountTypeModel();
                    model.Id = accountType.Id;
                    model.Name = accountType.Name;
                }
            }
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareAccountTypeSearchModelAsync(new AccountTypeSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/AccountType/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(AccountTypeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareAccountTypeListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareAccountTypeModelAsync(new AccountTypeModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/AccountType/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(AccountTypeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var accountType = new AccountType();
                accountType.Id = model.Id;
                accountType.Name = model.Name;

                await _accountTypeService.InsertAccountTypeAsync(accountType);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.AccountType.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = accountType.Id });
            }

            //prepare model
            model = await PrepareAccountTypeModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/AccountType/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a accountType with the specified id
            var accountType = await _accountTypeService.GetAccountTypeByIdAsync(id);
            if (accountType == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareAccountTypeModelAsync(null, accountType);

            return View("~/Plugins/Misc.SatyanamCRM/Views/AccountType/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(AccountTypeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a accountType with the specified id
            var accountType = await _accountTypeService.GetAccountTypeByIdAsync(model.Id);
            if (accountType == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                accountType = new AccountType();
                accountType.Id = model.Id;
                accountType.Name = model.Name;

                await _accountTypeService.UpdateAccountTypeAsync(accountType);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.AccountType.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = accountType.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareAccountTypeModelAsync(model, accountType, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/AccountType/Edit.cshtml", model);
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageAccountTypes, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _accountTypeService.GetAccountTypeByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _accountTypeService.DeleteAccountTypeAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion
    }
}
