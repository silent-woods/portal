using App.Core;
using App.Core.Domain;
using App.Core.Domain.Common;
using App.Core.Domain.Localization;
using App.Core.Domain.Security;
using App.Core.Infrastructure;
using App.Services.Common;
using App.Services.Directory;
using App.Services.Html;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Web.Areas.Admin.Controllers;
using App.Web.Areas.Admin.Factories;
using App.Web.Factories;
using App.Web.Framework.Mvc.Filters;
using App.Web.Framework.Themes;
using App.Web.Models.Common;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.EmailVerification.Services;
using System.Linq;
using System.Threading.Tasks;


namespace Satyanam.Plugin.Misc.EmailVerification.Controller
{
    /// <summary>
    /// Represents overridden workflow message service
    /// </summary>
    public class EmailCommonController : CommonController
    {

        #region Fields
        private readonly CaptchaSettings _captchaSettings;
        private readonly ICommonModelFactory _commonModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly ISitemapModelFactory _sitemapModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly IThemeContext _themeContext;
        private readonly IVendorService _vendorService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly SitemapSettings _sitemapSettings;
        private readonly SitemapXmlSettings _sitemapXmlSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly VendorSettings _vendorSettings;
        private readonly IDiscussProjectFormModelFactory _discussProjectFormModelFactory;
        private readonly INotificationService _notificationService;
        private readonly IPictureService _pictureService;
        private readonly IWebHelper _webHelper;
        private readonly INopFileProvider _fileProvider;
        private readonly ILogger _logger;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IEmailSender _emailSender;
        private readonly IProductService _productService;
        private readonly IDownloadService _downloadService;
        private readonly EmailVerificationSettings _emailVerificationSettings;
        private readonly IEmailverificationService _emailverificationService;
        private readonly CommonSettings _commonSettings;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly SecuritySettings _securitySettings;
        #endregion

        #region Ctor
        public EmailCommonController(
            EmailVerificationSettings emailVerificationSettings,
            IEmailverificationService emailverificationService,
            CaptchaSettings captchaSettings,
            CommonSettings commonSettings,
            ICommonModelFactory commonModelFactory,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IHtmlFormatter htmlFormatter,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ISitemapModelFactory sitemapModelFactory,
            IStoreContext storeContext,
            IThemeContext themeContext,
            IVendorService vendorService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            SitemapSettings sitemapSettings,
            SitemapXmlSettings sitemapXmlSettings,
            StoreInformationSettings storeInformationSettings,
            VendorSettings vendorSettings,
            IDiscussProjectFormModelFactory discussProjectFormModelFactory,
            INotificationService notificationService,
            IPictureService pictureService,
            IWebHelper webHelper,
            INopFileProvider nopFileProvider,
            ILogger logger,
            IEmailAccountService emailAccountService,
            IEmailSender emailSender,
            IProductService productService,
            IDownloadService downloadService,
            INewsLetterSubscriptionService newsLetterSubscriptionServic,
            SecuritySettings securitySettings
        ) : base(
            captchaSettings,
            commonSettings,
            commonModelFactory,
            currencyService,
            customerActivityService,
            genericAttributeService,
            htmlFormatter,
            languageService,
            localizationService,
            sitemapModelFactory,
            storeContext,
            themeContext,
            vendorService,
            workContext,
            workflowMessageService,
            localizationSettings,
            sitemapSettings,
            sitemapXmlSettings,
            storeInformationSettings,
            vendorSettings,
            discussProjectFormModelFactory,
            notificationService,
            pictureService,
            webHelper,
            nopFileProvider,
            logger,
            emailAccountService,
            emailSender,
            productService,
            downloadService,
            newsLetterSubscriptionServic, securitySettings
        )
        {
            _captchaSettings = captchaSettings;
            _commonModelFactory = commonModelFactory;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _htmlFormatter = htmlFormatter;
            _languageService = languageService;
            _localizationService = localizationService;
            _sitemapModelFactory = sitemapModelFactory;
            _storeContext = storeContext;
            _themeContext = themeContext;
            _vendorService = vendorService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _sitemapSettings = sitemapSettings;
            _sitemapXmlSettings = sitemapXmlSettings;
            _storeInformationSettings = storeInformationSettings;
            _vendorSettings = vendorSettings;
            _discussProjectFormModelFactory = discussProjectFormModelFactory;
            _notificationService = notificationService;
            _pictureService = pictureService;
            _webHelper = webHelper;
            _fileProvider = nopFileProvider;
            _logger = logger;
            _emailAccountService = emailAccountService;
            _emailSender = emailSender;
            _productService = productService;
            _downloadService = downloadService;
            _emailVerificationSettings = emailVerificationSettings;
            _emailverificationService = emailverificationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionServic;
            _securitySettings = securitySettings;
        }
        #endregion


        #region Utilities



        [HttpPost]
        [ValidateHoneypot]
        [IgnoreAntiforgeryToken]
        public override async Task<IActionResult> SubmitDiscussProjectForm(DiscussProjectFormModel model)
        {
            string verificationResult = await _emailverificationService.VerifyEmailApi(model.Email);
            dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

            if (!string.IsNullOrEmpty(verificationResult) && (verificationResponse.result != "valid" || verificationResponse.safe_to_send != "true"))
            {
                ModelState.AddModelError("Email", await _localizationService.GetResourceAsync("Invalid email address."));
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    errorType = "ModelStateError",
                    errors = ModelState.ToDictionary(
                        kvp => kvp.Key,
                        kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    )
                });
            }

            return await base.SubmitDiscussProjectForm(model);
        }



        [HttpPost, ActionName("ContactUs")]
        [ValidateHoneypot]
        // available even when a store is closed
        [CheckAccessClosedStore(ignore: true)]
        public override async Task<IActionResult> ContactUsSend(ContactUsModel model)
        {

            if (_emailVerificationSettings.ContactUspages == true)
            {
                string verificationResult = await _emailverificationService.VerifyEmailApi(model.Email);
                dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                // Check if the email is invalid or not safe to send
                if (verificationResponse.result != "valid" || verificationResponse.safe_to_send != "true")
                {
                    // Return a bad request with an error message
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Invalid email address."));
                    return View(model); // Ensure the action returns a View with errors
                }
            }

            return await base.ContactUsSend(model);
        }


        //public override async Task<IActionResult> SendEbookLink(string name, string email, string companyName, int productId, string userType, bool captchaValid)
        //{
        //    if (_emailVerificationSettings.EBookspage == true)
        //    {
        //        // Custom logic
        //        string verificationResult = await _emailverificationService.VerifyEmailApi(email);
        //        dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

        //        if (verificationResponse.result != "valid" || verificationResponse.safe_to_send != "true")
        //        {
        //            return new BadRequestObjectResult(new { success = false, message = "Invalid email address." });
        //        }
        //    }
        //    // Optionally call the base method
        //    return await base.SendEbookLink(name, email, companyName, productId, userType, captchaValid);
        //}



        #endregion
    }
}