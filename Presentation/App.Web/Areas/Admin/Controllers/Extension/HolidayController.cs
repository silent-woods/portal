using App.Core.Domain.Holidays;
using App.Core.Domain.Security;
using App.Services.Holidays;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Holidays;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class HolidayController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IHolidayModelFactory _holidayModelFactory;
        private readonly IHolidayService _holidayService;

        #endregion Fields

        #region Ctor

        public HolidayController(
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IHolidayModelFactory holidayModelFactory,
            IHolidayService holidayService)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _holidayModelFactory = holidayModelFactory;
            _holidayService = holidayService;
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _holidayModelFactory.PrepareHolidaySearchModelAsync(new HolidaySearchModel());

            return View("/Areas/Admin/Views/Extension/Holiday/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(HolidaySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _holidayModelFactory.PrepareHolidayListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _holidayModelFactory.PrepareHolidayModelAsync(new HolidayModel(), null);

            return View("/Areas/Admin/Views/Extension/Holiday/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(HolidayModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var holiday = model.ToEntity<Holiday>();

                await _holidayService.InsertHolidayAsync(holiday);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Holiday.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = holiday.Id });
            }

            //prepare model
            model = await _holidayModelFactory.PrepareHolidayModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Holiday/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a holiday with the specified id
            var holiday = await _holidayService.GetHolidayByIdAsync(id);
            if (holiday == null)
                return RedirectToAction("List");

            //prepare model
            var model= await _holidayModelFactory.PrepareHolidayModelAsync(null, holiday);

            return View("/Areas/Admin/Views/Extension/Holiday/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(HolidayModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a holiday with the specified id
            var holiday = await _holidayService.GetHolidayByIdAsync(model.Id);
            if (holiday == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                holiday = model.ToEntity(holiday);
                //holiday.Festival = model.Festival;
                //holiday.UpdatedOnUtc = DateTime.UtcNow;
                await _holidayService.UpdateHolidayAsync(holiday);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Holiday.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = holiday.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await _holidayModelFactory.PrepareHolidayModelAsync(model, holiday, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Holiday/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var holiday = await _holidayService.GetHolidayByIdAsync(id);
            if (holiday == null)
                return RedirectToAction("List");

            await _holidayService.DeleteHolidayAsync(holiday);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Holiday.Deleted"));

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageHoliday, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _holidayService.GetHolidayByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _holidayService.DeleteHolidayAsync(item);
            }

            return Json(new { Result = true });
        }

        #endregion
    }
}