using App.Core;
using App.Core.Domain;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Localization;
using App.Core.Domain.Media;
using App.Core.Domain.Security;
using App.Core.Events;
using App.Services.Authentication;
using App.Services.Authentication.External;
using App.Services.Authentication.MultiFactor;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.ExportImport;
using App.Services.Gdpr;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Factories;
using App.Web.Models.Customer;
using App.Web.Models.Employee;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace App.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class EmployeeController : BasePublicController
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
        private readonly IEmployeeModelFactory _employeeModelFactory;
        private readonly IGdprService _gdprService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMultiFactorAuthenticationPluginManager _multiFactorAuthenticationPluginManager;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IPictureService _pictureService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly MultiFactorAuthenticationSettings _multiFactorAuthenticationSettings;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IEmployeeService _employeeService;
        private readonly IEmployeeAddressModelFactory _employeeAddressModelFactory;
        private readonly IEmployeeEducationModelFactory _employeeEducationModelFactory;
        private readonly IEmployeeExperienceModelFactory _employeeExperienceModelFactory;
        private readonly IEmployeeAssetsModelFactory _employeeAssetsModelFactory;
        private readonly IEmpAddressService _empAddressService;
        private readonly IEducationService _educationService;
        private readonly IAnnouncementService _announcementService;
        #endregion

        #region Ctor

        public EmployeeController(AddressSettings addressSettings,
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
            ILocalizationService localizationService,
            ILogger logger,
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IPictureService pictureService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            MediaSettings mediaSettings,
            MultiFactorAuthenticationSettings multiFactorAuthenticationSettings,
            StoreInformationSettings storeInformationSettings,
            IEmployeeModelFactory employeeModelFactory,
            IEmployeeService employeeService,
            IEmployeeAddressModelFactory employeeAddressModelFactory,
            IEmployeeExperienceModelFactory employeeExperienceModelFactory,
            IEmployeeAssetsModelFactory employeeAssetsModelFactory,
            IEmployeeEducationModelFactory employeeEducationModelFactory,
            IEmpAddressService empAddressService,
            IEducationService educationService,
            IAnnouncementService announcementService)
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
            _localizationService = localizationService;
            _logger = logger;
            _multiFactorAuthenticationPluginManager = multiFactorAuthenticationPluginManager;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _pictureService = pictureService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _mediaSettings = mediaSettings;
            _multiFactorAuthenticationSettings = multiFactorAuthenticationSettings;
            _storeInformationSettings = storeInformationSettings;
            _employeeModelFactory = employeeModelFactory;
            _employeeService = employeeService;
            _employeeAddressModelFactory = employeeAddressModelFactory;
            _employeeExperienceModelFactory = employeeExperienceModelFactory;
            _employeeAssetsModelFactory = employeeAssetsModelFactory;
            _employeeEducationModelFactory = employeeEducationModelFactory;
            _empAddressService = empAddressService;
            _educationService = educationService;
            _announcementService = announcementService;
        }

        #endregion

        #region Methods

        #region My account / Employee Info

        //public virtual async Task<IActionResult> Info()
        //{
        //    var customer = await _workContext.GetCurrentCustomerAsync();
        //    var employeeId = customer.Id;
        //    var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
        //    if (!await _customerService.IsRegisteredAsync(customer))
        //        return Challenge();

        //    var model = new EmployeeInfoModel();
        //    model = await _employeeModelFactory.PrepareEmployeeInfoModelAsync(model, employee);

        //    return View(model);
        //}
        public virtual async Task<IActionResult> Info()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var model = new EmployeeInfoModel();
            model = await _employeeModelFactory.PrepareEmployeeInfoModelAsync(model);

            return View(model);
        }
        #endregion

        #region My account / Employee Addresses

        public virtual async Task<IActionResult> Addresses()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreEmployeeAddresses, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var addresses = await _empAddressService.GetAllAddressAsync(employee.Id, "");

            var model = new List<EmployeeAddressModel>();

            // Loop through all the addresses and prepare the model for each one
            foreach (var address in addresses)
            {
                var addressModel = new EmployeeAddressModel();
                model.Add(await _employeeAddressModelFactory.PrepareAddressAsync(addressModel, address));
            }

            return View(model); // Pass the list of addresses to the view
        }

        #endregion

        #region My account / Employee Education

        public virtual async Task<IActionResult> Education()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreEmployeeEducations, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer)|| customer ==null)
                return Challenge();

            var employeeId = customer.Id;
            var education = new Education();
            education.Id = employeeId;

            var model = new EmployeeEducationModel(); // Create a new EmployeeEducationModel object
            model = await _employeeEducationModelFactory.PrepareEmployeeEducationModelAsync(model, education);

            return View(model);
        }

        #endregion

        #region My account / Employee Experience

        public virtual async Task<IActionResult> Experience()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreEmployeeExperiences, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var employeeId = customer.Id;
            //var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            var experience = new Experience();
            experience.EmployeeID = employeeId;

            var model = new EmployeeExperienceModel();
            model = await _employeeExperienceModelFactory.PrepareEmployeeExperienceModelAsync(model);

            return View(model);
        }

        #endregion

        #region My account / Employee Assets

        public virtual async Task<IActionResult> Asset()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreEmployeeAssets, PermissionAction.View))
            {
                if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                    return Challenge();

                return View("/Themes/DefaultClean/Views/Common/AccessDenied.cshtml");
            }

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            var employeeId = customer.Id;
            //var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            var asset = new Assets();
            asset.Id = employeeId;

            var model = new EmployeeAssetsModel();
            model = await _employeeAssetsModelFactory.PrepareEmployeeAssetsModelAsync(model, asset);

            return View(model);
        }

        #endregion

        #region Other

        [HttpGet]
        public virtual async Task<IActionResult> LikeAnnouncement(int id, int employeeId)
        {
            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
                return NotFound();

            var likedIds = string.IsNullOrEmpty(announcement.LikedEmployeeIds)
                ? new List<int>()
                : announcement.LikedEmployeeIds.Split(',').Select(int.Parse).ToList();

            if (!likedIds.Contains(employeeId))
            {
                likedIds.Add(employeeId);
                announcement.LikedEmployeeIds = string.Join(",", likedIds);
                await _announcementService.UpdateAnnouncementAsync(announcement);
            }

            // You can redirect to a "Thank You" page or show a message
            return Content("Thanks for liking this announcement!");
        }

        #endregion

        #endregion
    }
}