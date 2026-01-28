using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using App.Core;
using App.Services.ExportImport;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class NewsLetterSubscriptionController : BaseAdminController
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IExportManager _exportManager;
        private readonly IImportManager _importManager;
        private readonly ILocalizationService _localizationService;
        private readonly INewsletterSubscriptionModelFactory _newsletterSubscriptionModelFactory;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public NewsLetterSubscriptionController(IDateTimeHelper dateTimeHelper,
            IExportManager exportManager,
            IImportManager importManager,
            ILocalizationService localizationService,
            INewsletterSubscriptionModelFactory newsletterSubscriptionModelFactory,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _dateTimeHelper = dateTimeHelper;
            _exportManager = exportManager;
            _importManager = importManager;
            _localizationService = localizationService;
            _newsletterSubscriptionModelFactory = newsletterSubscriptionModelFactory;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            //prepare model
            var model = await _newsletterSubscriptionModelFactory.PrepareNewsletterSubscriptionSearchModelAsync(new NewsletterSubscriptionSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SubscriptionList(NewsletterSubscriptionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _newsletterSubscriptionModelFactory.PrepareNewsletterSubscriptionListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SubscriptionUpdate(NewsletterSubscriptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(model.Id);

            //fill entity from model
            subscription = model.ToEntity(subscription);
            await _newsLetterSubscriptionService.UpdateNewsLetterSubscriptionAsync(subscription);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> SubscriptionDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByIdAsync(id)
                ?? throw new ArgumentException("No subscription found with the specified id", nameof(id));

            await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ImportCsv(IFormFile importcsvfile)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            try
            {
                if (importcsvfile != null && importcsvfile.Length > 0)
                {
                    var count = await _importManager.ImportNewsletterSubscribersFromTxtAsync(importcsvfile.OpenReadStream());

                    _notificationService.SuccessNotification(string.Format(await _localizationService.GetResourceAsync("Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess"), count));

                    return RedirectToAction("List");
                }

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        #endregion
    }
}