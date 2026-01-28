using App.Core;
using App.Core.Domain;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Forums;
using App.Core.Domain.Gdpr;
using App.Core.Domain.Localization;
using App.Core.Domain.Media;
using App.Core.Domain.PerformanceMeasurements;
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
using App.Services.PerformanceMeasurements;
using App.Services.Security;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;

//using App.Web.Areas.Admin.Factories;
using App.Web.Factories;
using App.Web.Framework.Mvc.Filters;
using App.Web.Models.Customer;
using App.Web.Models.Employee;
using App.Web.Models.PerformanceMeasurements;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;


namespace App.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class PerformanceController : BasePublicController
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
        private readonly IPerformanceModelFactory _performanceModelFactory;
        private readonly ITeamPerformanceMeasurementService _teamPerformanceMeasurementService;
        private readonly IEncryptionService _encryptionService;
       

        #endregion

        #region Ctor

        public PerformanceController(AddressSettings addressSettings,
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
            IPerformanceModelFactory performanceModelFactory,
            ITeamPerformanceMeasurementService teamPerformanceMeasurementService,
            IEncryptionService encryptionService)
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
            _performanceModelFactory = performanceModelFactory;
            _teamPerformanceMeasurementService = teamPerformanceMeasurementService;
            _encryptionService = encryptionService;
        }

        #endregion

        #region Methods

        #region My account / Employee Performance Report
        public virtual async Task<IActionResult> AddRatings(int monthId, int employeeId, int Year)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTeamPerformanceMeasurement))
                return Challenge();


            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            //prepare model
            int currCustomer = 0;

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    currCustomer = employeeByCustomer.Id;

                }

            }

            if (!(await _teamPerformanceMeasurementService.IsEmployeeCanAddRatings(currCustomer)))
                return AccessDeniedView();

            PerformanceMeasurementModel teamPerformanceMeasurementModel = new PerformanceMeasurementModel();
            teamPerformanceMeasurementModel.MonthId = monthId;
            teamPerformanceMeasurementModel.EmployeeManagerId = currCustomer;
            teamPerformanceMeasurementModel.EmployeeId = employeeId;
            //teamPerformanceMeasurementModel.SelectedEmployeeId.Add(employeeId);
            //teamPerformanceMeasurementModel.SelectedManagerId.Add(managerId);
            teamPerformanceMeasurementModel.Year = Year;

            if(teamPerformanceMeasurementModel.MonthId == 0)
            {
              teamPerformanceMeasurementModel.MonthId=  DateTime.Now.Month;
            }
            if(teamPerformanceMeasurementModel.Year == 0)
            {
                teamPerformanceMeasurementModel.Year = DateTime.Now.Year;
            }

            //prepare model
            var model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(teamPerformanceMeasurementModel, null);

            if (!model.Employees.Any(e => e.Value == model.EmployeeId.ToString()))
            {
                // EmployeeId is not in the dropdown list, return Access Denied
                return AccessDeniedView();
            }

            return View("/Themes/DefaultClean/Views/Extension/Performance/AddRatings.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> AddRatings(PerformanceMeasurementModel model, bool continueEditing)
        {
            
            
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            //prepare model
            int currCustomer = 0;

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    currCustomer = employeeByCustomer.Id;
                }

            }
            int selectedEmployeeId = model.EmployeeId;
            int selectedManagerId = currCustomer;
            model.EmployeeManagerId = currCustomer;

            if (selectedManagerId == 0 || selectedEmployeeId == 0)
            {
                if (selectedManagerId == 0)
                { _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectManagerValidation")); }
                else { _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation")); }

                //prepare model
                model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form
                return View("/Themes/DefaultClean/Views/Extension/Performance/AddRatings.cshtml", model);
            }
            if (selectedEmployeeId == selectedManagerId && selectedEmployeeId != 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.EmployeeManagerNotSameValidation"));
                //prepare model
                model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form
                return View("/Themes/DefaultClean/Views/Extension/Performance/AddRatings.cshtml", model);
            }

            if (model.MonthId  == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectMonth"));
                //prepare model
                model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(model, null, true);

                //if we got this far, something failed, redisplay form
                return View("/Themes/DefaultClean/Views/Extension/Performance/AddRatings.cshtml", model);
            }
            if (ModelState.IsValid)
            {
                var teamPerformances = new TeamPerformanceMeasurement();
                teamPerformances.CreateOnUtc = DateTime.UtcNow;
                teamPerformances.UpdateOnUtc = DateTime.UtcNow;
                teamPerformances.Year = model.Year;
                teamPerformances.KPIMasterData = JsonConvert.SerializeObject(model.KPIMaster);
                teamPerformances.Feedback = model.Feedback;
                teamPerformances.EmployeeId=model.EmployeeId;
                teamPerformances.EmployeeManagerId=model.EmployeeManagerId;
                teamPerformances.MonthId =model.MonthId;
                teamPerformances.Id = model.Id;

               
                
                if (model.Id == 0)
                {
                    await _teamPerformanceMeasurementService.InsertTeamPerformanceMeasurementAsync(teamPerformances);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Rating.Added"));
                }
                else
                {
                    teamPerformances.UpdateOnUtc = DateTime.UtcNow;
                    await _teamPerformanceMeasurementService.UpdateTeamPerformanceMeasurementAsync(teamPerformances);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Rating.Updated"));
                }
          
                    return RedirectToAction("AddRatings", "Performance", new { Monthid = model.MonthId,employeeId = model.EmployeeId, year = model.Year });
           
            }
            //prepare model
            model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("/Themes/DefaultClean/Views/Extension/Performance/AddRatings.cshtml", model);
        }
        public virtual async Task<IActionResult> MonthlyReview(int monthId, int employeeId, int year)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            //prepare model
            int currCustomer = 0;

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    currCustomer = employeeByCustomer.Id;
                }

            }
            var employee = await _employeeService.GetEmployeeByIdAsync(currCustomer);
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            var performance = await _teamPerformanceMeasurementService.GetTeamPerformanceMeasurementByIdAsync(employeeId);
            //var model = new EmployeeInfoModel();
            PerformanceMeasurementModel performanceMeasurementModel = new PerformanceMeasurementModel();
            performanceMeasurementModel.MonthId = monthId;
            performanceMeasurementModel.EmployeeId = employeeId;
            performanceMeasurementModel.Year = year;
            
            if (performanceMeasurementModel.MonthId == 0)
            {
                performanceMeasurementModel.MonthId = DateTime.Now.Month;
            }
            if (performanceMeasurementModel.Year == 0)
            {
                performanceMeasurementModel.Year = DateTime.Now.Year;
            }

            //model = await _employeeModelFactory.PrepareEmployeeInfoModelAsync(model, employee);
            var model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(performanceMeasurementModel, performance);
            TeamPerformanceMeasurement teamPerformance = new TeamPerformanceMeasurement();

            if (model != null)
            {
                model.currCustomer = currCustomer;
                model.measurementModel = await _performanceModelFactory.PrepareMonthlyReviewModelAsync(performanceMeasurementModel, teamPerformance, false);
            }
            if (!model.Employees.Any(e => e.Value == model.EmployeeId.ToString()))
            {
                // EmployeeId is not in the dropdown list, return Access Denied
                return AccessDeniedView();
            }
            return View("/Themes/DefaultClean/Views/Extension/Performance/MonthlyReview.cshtml", model);
        }

        #endregion

        #region My account / Employee Performance Report

        public virtual async Task<IActionResult> YearlyReview(int startmonth, int endmonth, int startYear, int endYear, int employeeId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var empId = customer.Id;
            var employee = await _employeeService.GetEmployeeByIdAsync(empId);
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            int currCustomer = 0;

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    currCustomer = employeeByCustomer.Id;
                }

            }

            var performance = await _teamPerformanceMeasurementService.GetTeamPerformanceMeasurementByIdAsync(employeeId);
            //var model = new EmployeeInfoModel();
            PerformanceMeasurementModel performanceMeasurementModel = new PerformanceMeasurementModel();
            performanceMeasurementModel.StartMonth = startmonth;
            performanceMeasurementModel.EndMonth = endmonth;
            performanceMeasurementModel.StartYear = startYear;
            performanceMeasurementModel.EndYear = endYear;
            performanceMeasurementModel.EmployeeId = employeeId;
            //model = await _employeeModelFactory.PrepareEmployeeInfoModelAsync(model, employee);
            var model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(performanceMeasurementModel, performance);

            model.currCustomer = currCustomer;
            // Validate the date range (months difference)
            if (startYear != 0 && startmonth != 0 && endYear != 0 && endmonth != 0)
            {
                var startDate = new DateTime(startYear, startmonth, 1);
                var endDate = new DateTime(endYear, endmonth, 1);

                var monthDifference = (endDate.Year - startDate.Year) * 12 + endDate.Month - startDate.Month;

                // Check if the difference in months exceeds 12
                if (monthDifference >= 12)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.YearError"));
                    return View("/Themes/DefaultClean/Views/Extension/Performance/YearlyReview.cshtml", model);
                }
            }

            TeamPerformanceMeasurement teamPerformance = new TeamPerformanceMeasurement();

            model.measurementModel = await _performanceModelFactory.PrepareYearlyReviewModelAsync(performanceMeasurementModel, teamPerformance, false);

            return View("/Themes/DefaultClean/Views/Extension/Performance/YearlyReview.cshtml",model);
        }


        public virtual async Task<IActionResult> ProjectLeaderReview(int monthId, int year)

        {

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            //prepare model
            int currCustomer = 0;

            if (customer != null)
            {
                var employeeByCustomer = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (employeeByCustomer != null)
                {
                    currCustomer = employeeByCustomer.Id;
                }

            }
            PerformanceMeasurementModel teamPerformanceMeasurementModel = new PerformanceMeasurementModel();
            teamPerformanceMeasurementModel.MonthId = monthId;
            //teamPerformanceMeasurementModel.EmployeeId = employeeId;
            teamPerformanceMeasurementModel.Year = year;
            teamPerformanceMeasurementModel.EmployeeManagerId = currCustomer;
            if (teamPerformanceMeasurementModel.MonthId == 0)
            {
                teamPerformanceMeasurementModel.MonthId = DateTime.Now.Month;
            }
            if (teamPerformanceMeasurementModel.Year == 0)
            {
                teamPerformanceMeasurementModel.Year = DateTime.Now.Year;
            }


            TeamPerformanceMeasurement teamPerformance = new TeamPerformanceMeasurement();

            //prepare model

            var model = await _performanceModelFactory.PreparePerformanceMeasurementModelAsync(teamPerformanceMeasurementModel, null);

            model.measurementModel = await _performanceModelFactory.PrepareProjectLeaderReviewModelAsync(teamPerformanceMeasurementModel, teamPerformance, false);

            return View("/Themes/DefaultClean/Views/Extension/Performance/ProjectLeaderReview.cshtml", model);

        }
        #endregion

        #endregion
    }
}