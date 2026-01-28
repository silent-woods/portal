using App.Core;
using App.Core.Domain;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Localization;
using App.Core.Domain.Media;
using App.Core.Domain.Security;
using App.Core.Events;
using App.Core.Infrastructure;
using App.Services.Authentication;
using App.Services.Authentication.External;
using App.Services.Authentication.MultiFactor;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.ExportImport;
using App.Services.Gdpr;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Controllers;
using App.Web.Framework.Mvc.Filters;
using App.Web.Models.Customer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Plugin.Misc.EmailVerification.Services;
using System.Text.Encodings.Web;
using System.Threading.Tasks;


namespace Satyanam.Plugin.Misc.EmailVerification.Controller
{
    /// <summary>
    /// Represents overridden workflow message service
    /// </summary>
    public class EmailRegisterController : CustomerController
    {

        #region Fields
        private readonly AddressSettings _addressSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ForumSettings _forumSettings;
        private readonly GdprSettings _gdprSettings;
        private readonly HtmlEncoder _htmlEncoder;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerAttributeParser _customerAttributeParser;
        private readonly ICustomerAttributeService _customerAttributeService;
        private readonly ICustomerModelFactory _customerModelFactory;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IDownloadService _downloadService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IExportManager _exportManager;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly TaxSettings _taxSettings;
        private readonly INopFileProvider _fileProvider;
        private readonly IEmailverificationService _emailverificationService;
        private readonly EmailVerificationSettings _emailVerificationSettings;
        #endregion

        #region Ctor
        public EmailRegisterController(
 AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CustomerSettings customerSettings,
        DateTimeSettings dateTimeSettings,
        ForumSettings forumSettings,
        GdprSettings gdprSettings,
        HtmlEncoder htmlEncoder,
        IAddressAttributeParser addressAttributeParser,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        IAuthenticationService authenticationService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerActivityService customerActivityService,
        ICustomerAttributeParser customerAttributeParser,
        ICustomerAttributeService customerAttributeService,
        ICustomerModelFactory customerModelFactory,
        ICustomerRegistrationService customerRegistrationService,
        ICustomerService customerService,
        IDownloadService downloadService,
        IEventPublisher eventPublisher,
        IExportManager exportManager,
        IExternalAuthenticationService externalAuthenticationService,
        IGdprService gdprService,
        IGenericAttributeService genericAttributeService,
        IGiftCardService giftCardService,
        ILocalizationService localizationService,
        ILogger logger,
        IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
        INewsLetterSubscriptionService newsLetterSubscriptionService,
        INotificationService notificationService,
        IOrderService orderService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceFormatter priceFormatter,
        IProductService productService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        ITaxService taxService,
        IWorkContext workContext,
        IWorkflowMessageService workflowMessageService,
        LocalizationSettings localizationSettings,
        MediaSettings mediaSettings,
        MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
        StoreInformationSettings storeInformationSettings,
        TaxSettings taxSettings,
        INopFileProvider nopFileProvider, IEmailverificationService emailverificationService, EmailVerificationSettings emailVerificationSettings)
        : base(addressSettings, captchaSettings, customerSettings, dateTimeSettings, forumSettings,
               gdprSettings, htmlEncoder, addressAttributeParser, addressModelFactory, addressService,
               authenticationService, countryService, currencyService, customerActivityService,
               customerAttributeParser, customerAttributeService, customerModelFactory, customerRegistrationService,
               customerService, downloadService, eventPublisher, exportManager, externalAuthenticationService,
               gdprService, genericAttributeService, giftCardService, localizationService, logger,
               multiFactorAuthenticationPluginManager, newsLetterSubscriptionService, notificationService,
               orderService, permissionService, pictureService, priceFormatter, productService,
               stateProvinceService, storeContext, taxService, workContext, workflowMessageService,
               localizationSettings, mediaSettings, multiFactorAuthenticationSettings, storeInformationSettings,
               taxSettings, nopFileProvider)
        {
            _addressSettings = addressSettings;
            _captchaSettings = captchaSettings;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _forumSettings = forumSettings;
            _gdprSettings = gdprSettings;
            _htmlEncoder = htmlEncoder;
            _addressAttributeParser = addressAttributeParser;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _authenticationService = authenticationService;
            _countryService = countryService;
            _currencyService = currencyService;
            _customerActivityService = customerActivityService;
            _customerAttributeParser = customerAttributeParser;
            _customerAttributeService = customerAttributeService;
            _customerModelFactory = customerModelFactory;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _downloadService = downloadService;
            _eventPublisher = eventPublisher;
            _exportManager = exportManager;
            _externalAuthenticationService = externalAuthenticationService;
            _gdprService = gdprService;
            _genericAttributeService = genericAttributeService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _logger = logger;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
            _storeInformationSettings = storeInformationSettings;
            _taxSettings = taxSettings;
            _fileProvider = nopFileProvider;
            _emailverificationService = emailverificationService;
            _emailVerificationSettings = emailVerificationSettings;
        }

        #endregion

        #region Utilities






        [HttpPost]
        [ValidateCaptcha]
        [ValidateHoneypot]
        //available even when navigation is not allowed
        [CheckAccessPublicStore(ignore: true)]
        public override async Task<IActionResult> Register(RegisterModel model, string returnUrl, bool captchaValid, IFormCollection form)
        {
            if (_emailVerificationSettings.Registartionpage == true)
            {
                string verificationResult = await _emailverificationService.VerifyEmailApi(model.Email);
                dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                // Check if the email is invalid or not safe to send
                if (verificationResponse.result != "valid" || verificationResponse.safe_to_send != "true")
                {
                    // Return a bad request with an error message
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Invalid email address."));

                }
            }


            return await base.Register(model, returnUrl, captchaValid, form);
        }



        #endregion
    }
}