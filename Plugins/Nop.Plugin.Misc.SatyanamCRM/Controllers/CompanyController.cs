using App.Core;
using App.Core.Domain.Common;
using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Services;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CompanyController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ICompanyService _companyService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAddressService _addressService;
        private readonly IIndustryService _industryService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IContactsService _contactsService;
        private readonly IAccountTypeService _accountTypeService;
        private readonly IDealsService _dealsService;
        private readonly ILeadSourceService _leadSourceService;
        private readonly ITitleService _titleService;
        #endregion

        #region Ctor 

        public CompanyController(IPermissionService permissionService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               ICompanyService companyService,
                               IDateTimeHelper dateTimeHelper,
                               IAddressService addressService,
                               IIndustryService industryService,
                               ICountryService countryService,
                               IStateProvinceService stateProvinceService,
                               IWorkContext workContext,
                               ICustomerService customerService,
                               IContactsService contactsService,
                               IAccountTypeService accountTypeService,
                               IDealsService dealsService,
                               ILeadSourceService leadSourceService,
                               ITitleService titleService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _companyService = companyService;
            _dateTimeHelper = dateTimeHelper;
            _addressService = addressService;
            _industryService = industryService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
            _customerService = customerService;
            _contactsService = contactsService;
            _accountTypeService = accountTypeService;
            _dealsService = dealsService;
            _leadSourceService = leadSourceService;
            _titleService = titleService;
        }

        #endregion

        #region Utilities
        public virtual async Task PrepareIndustryListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Industrys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var industrys = await _industryService.GetAllIndustryByNameAsync("");
            foreach (var p in industrys)
            {
                model.Industrys.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareBullingCountriesListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.BillingCountrys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Usually, "0" represents a "Select" option in dropdowns.
            });

            var countries = await _countryService.GetAllCountriesAsync();
            foreach (var country in countries)
            {
                model.BillingCountrys.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(country, x => x.Name),
                    Value = country.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareShipingCountriesListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ShipingCountrys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Usually, "0" represents a "Select" option in dropdowns.
            });

            var countries = await _countryService.GetAllCountriesAsync();
            foreach (var country in countries)
            {
                model.ShipingCountrys.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(country, x => x.Name),
                    Value = country.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareBillingStatesListAsync(CompanyModel model, int countryId)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.BillingStates.Add(new SelectListItem
            {
                Text = "Select State",
                Value = "0" // Default "Select" option
            });

            if (countryId > 0) // Only fetch states if a valid country is selected
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(countryId);
                foreach (var state in states)
                {
                    model.BillingStates.Add(new SelectListItem
                    {
                        Text = state.Name,
                        Value = state.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareAccountListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ParentAccounts.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Usually, "0" represents a "Select" option in dropdowns.
            });

            var contacts = await _companyService.GetAllCompanyAsync("", "");
            foreach (var contact in contacts)
            {
                model.ParentAccounts.Add(new SelectListItem
                {
                    Text = contact.CompanyName,
                    Value = contact.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareAccountTypeListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AccountTypes.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Usually, "0" represents a "Select" option in dropdowns.
            });

            var account = await _accountTypeService.GetAllIAccountTypeAsync("");
            foreach (var accountType in account)
            {
                model.AccountTypes.Add(new SelectListItem
                {
                    Text = accountType.Name,
                    Value = accountType.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareShipingStatesListAsync(CompanyModel model, int countryId)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ShipingStates.Add(new SelectListItem
            {
                Text = "Select State",
                Value = "0" // Default "Select" option
            });

            if (countryId > 0) // Only fetch states if a valid country is selected
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(countryId);
                foreach (var state in states)
                {
                    model.ShipingStates.Add(new SelectListItem
                    {
                        Text = state.Name,
                        Value = state.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareContactsListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.CompanyOwner.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Default "Select" option
            });
            var contacts = await _contactsService.GetAllContactsAsync("", "", "");
            foreach (var contact in contacts)
            {
                model.CompanyOwner.Add(new SelectListItem
                {
                    Text = contact.FirstName + " " + contact.LastName,
                    Value = contact.Id.ToString()
                });
            }

        }
        public virtual async Task PrepareContactsListAsync(CompanyDealsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Contacts.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Default "Select" option
            });
            var contacts = await _contactsService.GetAllContactsAsync("", "", "");
            foreach (var contact in contacts)
            {
                model.Contacts.Add(new SelectListItem
                {
                    Text = contact.FirstName + " " + contact.LastName,
                    Value = contact.Id.ToString()
                });
            }

        }
        public virtual async Task PrepareCustomersListAsync(CompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var currentUser = await _workContext.GetCurrentCustomerAsync();
            var currentUserId = currentUser?.Id.ToString() ?? "0";

            // Only set default selected customer if not already set (initial GET)
            if (model.CustomerId == 0)
                model.CustomerId = Convert.ToInt32(currentUserId);

            model.Customers = new List<SelectListItem>();

            var customers = await _customerService.GetAllCustomersAsync();
            foreach (var customer in customers)
            {
                if (customer == null || string.IsNullOrWhiteSpace(customer.FirstName) || string.IsNullOrWhiteSpace(customer.LastName))
                    continue;

                model.Customers.Add(new SelectListItem
                {
                    Text = $"{customer.FirstName} {customer.LastName}",
                    Value = customer.Id.ToString(),
                    Selected = customer.Id == model.CustomerId
                });
            }

            model.Customers.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0",
                Selected = model.CustomerId == 0 || !model.Customers.Any(c => c.Selected)
            });
            //if (model == null)
            //    throw new ArgumentNullException(nameof(model));

            //var currentUser = await _workContext.GetCurrentCustomerAsync();
            //var currentUserId = currentUser?.Id.ToString() ?? "0";

            //// Set default selected customer
            //model.CustomerId = Convert.ToInt32(currentUserId);

            //model.Customers = new List<SelectListItem>(); // Ensure no previous values remain

            //var customers = await _customerService.GetAllCustomersAsync();
            //foreach (var customer in customers)
            //{
            //    if (customer == null || string.IsNullOrWhiteSpace(customer.FirstName) || string.IsNullOrWhiteSpace(customer.LastName))
            //        continue;

            //    model.Customers.Add(new SelectListItem
            //    {
            //        Text = $"{customer.FirstName} {customer.LastName}",
            //        Value = customer.Id.ToString(),
            //        Selected = customer.Id.ToString() == currentUserId // Ensuring the selection
            //    });
            //}

            //// Add "Select" option at the top if needed
            //model.Customers.Insert(0, new SelectListItem
            //{
            //    Text = "Select",
            //    Value = "0",
            //    Selected = currentUserId == "0" || !model.Customers.Any(c => c.Selected)
            //});
        }
        public virtual async Task PrepareCustomersListAsync(CompanyDealsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var currentUser = await _workContext.GetCurrentCustomerAsync();
            var currentUserId = currentUser?.Id.ToString() ?? "0";

            // Only set default selected customer if not already set (initial GET)
            if (model.CustomerId == 0)
                model.CustomerId = Convert.ToInt32(currentUserId);

            model.DealOwner = new List<SelectListItem>();

            var customers = await _customerService.GetAllCustomersAsync();
            foreach (var customer in customers)
            {
                if (customer == null || string.IsNullOrWhiteSpace(customer.FirstName) || string.IsNullOrWhiteSpace(customer.LastName))
                    continue;

                model.DealOwner.Add(new SelectListItem
                {
                    Text = $"{customer.FirstName} {customer.LastName}",
                    Value = customer.Id.ToString(),
                    Selected = customer.Id == model.CustomerId
                });
            }

            model.DealOwner.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0",
                Selected = model.CustomerId == 0 || !model.DealOwner.Any(c => c.Selected)
            });
        }
        public virtual async Task PrepareCompanyListAsync(CompanyDealsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Companys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Default "Select" option
            });
            var company = await _companyService.GetAllCompanyAsync("", "");
            foreach (var contacts in company)
            {
                model.Companys.Add(new SelectListItem
                {
                    Text = contacts.CompanyName,
                    Value = contacts.Id.ToString()
                });
            }

        }
        public virtual async Task PrepareLeadSourcesListAsync(CompanyDealsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.LeadSources.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });

            var leadSource = await _leadSourceService.GetAllLeadSourceByNameAsync("");
            foreach (var p in leadSource)
            {
                model.LeadSources.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task<CompanySearchModel> PrepareCompanySearchModelAsync(CompanySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<CompanyListModel> PrepareCompanyListModelAsync(CompanySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get company
            var company = await _companyService.GetAllCompanyAsync(showHidden: true,
                name: searchModel.CompanyName,
                website: searchModel.SearchWebsiteUrl,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new CompanyListModel().PrepareToGridAsync(searchModel, company, () =>
            {
                //fill in model values from the entity
                return company.SelectAwait(async companys =>
                {
                    var companyModel = new CompanyModel();
                    var selectedAvailableOption = companys.OwnerShipId;
                    companyModel.Id = companys.Id;
                    companyModel.ContactId = companys.ContactId;
                    companyModel.CompanyName = companys.CompanyName;
                    companyModel.WebsiteUrl = companys.WebsiteUrl;
                    companyModel.ParentAccountID = companys.ParentAccountID;
                    companyModel.AccountNumber = companys.AccountNumber;
                    companyModel.AccountTypeId = companys.AccountTypeId;
                    companyModel.IndustryId = companys.IndustryId;
                    companyModel.CustomerId = companys.CustomerId;
                    companyModel.Phone = companys.Phone;
                    companyModel.NoofEmployee = companys.NoofEmployee;
                    companyModel.AnnualRevenue = companys.AnnualRevenue;
                    companyModel.BillingAddressId = companys.BillingAddressId;
                    companyModel.ShipingAddressId = companys.ShipingAddressId;
                    companyModel.Description = companys.Description;

                    companyModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(companys.CreatedOnUtc, DateTimeKind.Utc);
                    companyModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(companys.UpdatedOnUtc, DateTimeKind.Utc);
                    companyModel.OwnerShipId = companys.OwnerShipId;
                    companyModel.OwnerShipName = ((OwnereShipEnum)companys.OwnerShipId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) companyModel.OwnerShipId
                        = (int)((OwnereShipEnum)selectedAvailableOption);

                    return companyModel;
                });
            });
            return model;
        }

        public virtual async Task<CompanyModel> PrepareCompanyModelAsync(CompanyModel model, Company company, bool excludeProperties = false)
        {
            var ownerShip = await OwnereShipEnum.None.ToSelectListAsync();
            if (company != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new CompanyModel();
                    model.Id = company.Id;
                    model.ContactId = company.ContactId;
                    model.CompanyName = company.CompanyName;
                    model.WebsiteUrl = company.WebsiteUrl;
                    model.ParentAccountID = company.ParentAccountID;
                    model.AccountNumber = company.AccountNumber;
                    model.AccountTypeId = company.AccountTypeId;
                    model.IndustryId = company.IndustryId;
                    model.CustomerId = company.CustomerId;
                    model.Phone = company.Phone;
                    model.OwnerShipId = company.OwnerShipId;
                    model.NoofEmployee = company.NoofEmployee;
                    model.AnnualRevenue = company.AnnualRevenue;
                    model.Description = company.Description;
                    model.CreatedOnUtc = company.CreatedOnUtc;
                    model.UpdatedOnUtc = company.UpdatedOnUtc;

                    if (company.BillingAddressId > 0)
                    {
                        var address = await _addressService.GetAddressByIdAsync(company.BillingAddressId);
                        if (address != null)
                        {
                            model.BillingAddress1 = address.Address1;
                            model.BillingAddress2 = address.Address2;
                            model.BillingCity = address.City;
                            model.BillingZipCode = address.ZipPostalCode;
                            model.BillingCountryId = address.CountryId;
                            model.BillingStateId = address.StateProvinceId;
                            model.BillingAddressId = address.Id;
                        }
                    }
                    if (company.ShipingAddressId > 0)
                    {
                        var address = await _addressService.GetAddressByIdAsync(company.ShipingAddressId);
                        if (address != null)
                        {
                            model.ShipingAddress1 = address.Address1;
                            model.ShipingAddress2 = address.Address2;
                            model.ShipingCity = address.City;
                            model.ShipingZipCode = address.ZipPostalCode;
                            model.ShipingCountryId = address.CountryId;
                            model.ShipingStateId = address.StateProvinceId;
                            model.ShipingAddressId = address.Id;
                        }
                    }
                }
            }
            model.OwnerShips = ownerShip.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.OwnerShipId.ToString() == store.Value
            }).ToList();

            await PrepareIndustryListAsync(model);
            await PrepareBullingCountriesListAsync(model);
            await PrepareBillingStatesListAsync(model, Convert.ToInt32(model.BillingStateId));
            await PrepareShipingCountriesListAsync(model);
            await PrepareShipingStatesListAsync(model, Convert.ToInt32(model.ShipingStateId));
            await PrepareCustomersListAsync(model);
            await PrepareAccountListAsync(model);
            await PrepareAccountTypeListAsync(model);
            await PrepareContactsListAsync(model);
            return model;
        }
        public virtual async Task<CompanyContactListModel> PrepareCompanyContactListModelAsync(CompanyContactSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var deals = await _contactsService.GetContactByCompanyIdAsync(
                companyId: searchModel.CompanyId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );
            var titleIds = deals.Select(c => c.TitleId).Distinct().ToList();

            // Step 3: Fetch all titles
            var titles = await _titleService.GetTitleByIdsAsync(titleIds.ToArray()); // You write this service method
            var titleDict = titles.ToDictionary(t => t.Id, t => t.Name);
            var model = await ModelExtensions.PrepareToGridAsync<CompanyContactListModel, CompanyContactModel, Contacts>(
                new CompanyContactListModel(),
                searchModel,
                deals,
                () => deals.Select(d => new CompanyContactModel
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    Name= $"{d.FirstName} {d.LastName}".Trim(),
                    TitleName = titleDict.TryGetValue(d.TitleId, out var name) ? name : string.Empty
                }).ToAsyncEnumerable()
            );

            return model;
        }

        public virtual async Task<CompanyDealsListModel> PrepareCompanyDealsListModelAsync(CompanyDealsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var deals = await _dealsService.GetDealsByCompanyIdAsync(
                companyId: searchModel.CompanyId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            var model = await ModelExtensions.PrepareToGridAsync<CompanyDealsListModel, CompanyDealsModel, Deals>(
                new CompanyDealsListModel(),
                searchModel,
                deals,
                () => deals.Select(d => new CompanyDealsModel
                {
                    Id = d.Id,
                    DealName = d.DealName,
                    Amount = d.Amount,
                    Stage = ((StageEnum)d.StageId).ToString(),
                    Probability = d.Probability,
                    ClosingDate = d.ClosingDate,
                    Date = d.ClosingDate?.ToString("yyyy-MM-dd")
                }).ToAsyncEnumerable()
            );

            return model;
        }

        public virtual async Task<CompanyDealsModel> PrepareCompanyDealsModelAsync(CompanyDealsModel model, Deals deals, bool excludeProperties = false)
        {
            var stage = await StageEnum.Qualification.ToSelectListAsync();
            var type = await TypeEnum.None.ToSelectListAsync();
            if (deals != null)
            {
                if (model == null)
                {
                    model = new CompanyDealsModel();
                    model.Id = deals.Id;
                    model.CustomerId = deals.CustomerId;
                    model.DealName = deals.DealName;
                    model.Amount = deals.Amount;
                    model.CompanyId = deals.CompanyId;
                    model.Probability = deals.Probability;
                    model.NextStep = deals.NextStep;
                    model.ExpectedRevenue = deals.ExpectedRevenue;
                    model.LeadSourceId = deals.LeadSourceId;
                    model.ClosingDate = deals.ClosingDate;
                    model.ContactId = deals.ContactId;
                    model.Description = deals.Description;
                    model.StageId = deals.StageId;
                    model.TypeId = deals.TypeId;
                    model.CreatedOnUtc = deals.CreatedOnUtc;
                    model.UpdatedOnUtc = deals.UpdatedOnUtc;
                }

            }
            //prepare available countries

            model.Stages = stage.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StageId.ToString() == store.Value
            }).ToList();
            model.Types = type.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.TypeId.ToString() == store.Value
            }).ToList();

            await PrepareCompanyListAsync(model);
            await PrepareCustomersListAsync(model);
            await PrepareLeadSourcesListAsync(model);
            await PrepareContactsListAsync(model);
            return model;
        }
        [HttpGet]
        public async Task<IActionResult> Search(string term)
        {
            var companies = await _companyService.GetAllCompanyAsync(term, "");
            var names = companies
                .Where(c => c.CompanyName.Contains(term, StringComparison.OrdinalIgnoreCase))
                .Select(c => c.CompanyName)
                .Distinct()
                .ToList();

            return Json(names);
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCompanySearchModelAsync(new CompanySearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CompanySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareCompanyListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCompanyModelAsync(new CompanyModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CompanyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var billingAddress = new Address
                {
                    Address1 = model.BillingAddress1,
                    Address2 = model.BillingAddress2,
                    City = model.BillingCity,
                    ZipPostalCode = model.BillingZipCode,
                    CountryId = model.BillingCountryId,
                    StateProvinceId = model.BillingStateId,
                };
                await _addressService.InsertAddressAsync(billingAddress);
                var billingAddressid = billingAddress.Id;

                var shipingAddress = new Address
                {
                    Address1 = model.ShipingAddress1,
                    Address2 = model.ShipingAddress2,
                    City = model.ShipingCity,
                    ZipPostalCode = model.ShipingZipCode,
                    CountryId = model.ShipingCountryId,
                    StateProvinceId = model.ShipingStateId,
                };
                await _addressService.InsertAddressAsync(shipingAddress);
                var shipingAddressid = shipingAddress.Id;
                var company = new Company();
                company.Id = model.Id;
                company.ContactId = model.ContactId;
                company.CompanyName = model.CompanyName;
                company.WebsiteUrl = model.WebsiteUrl;
                company.ParentAccountID = model.ParentAccountID;
                company.OwnerShipId = model.OwnerShipId;
                company.AccountNumber = model.AccountNumber;
                company.AccountTypeId = model.AccountTypeId;
                company.IndustryId = model.IndustryId;
                company.CustomerId = model.CustomerId;
                company.Phone = model.Phone;
                company.NoofEmployee = model.NoofEmployee;
                company.AnnualRevenue = model.AnnualRevenue;
                company.Description = model.Description;
                company.CreatedOnUtc = DateTime.UtcNow;
                company.UpdatedOnUtc = DateTime.UtcNow;
                company.BillingAddressId = billingAddressid;
                company.ShipingAddressId = shipingAddressid;
                await _companyService.InsertCompanyAsync(company);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Company.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = company.Id });
            }

            //prepare model
            model = await PrepareCompanyModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a company with the specified id
            var companys = await _companyService.GetCompanyByIdAsync(id);
            if (companys == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareCompanyModelAsync(null, companys);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CompanyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a company with the specified id
            var companys = await _companyService.GetCompanyByIdAsync(model.Id);
            if (companys == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                if (companys.BillingAddressId > 0)
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(companys.BillingAddressId);
                    if (billingAddress != null)
                    {
                        billingAddress.Address1 = model.BillingAddress1;
                        billingAddress.Address2 = model.BillingAddress2;
                        billingAddress.City = model.BillingCity;
                        billingAddress.ZipPostalCode = model.BillingZipCode;
                        billingAddress.CountryId = model.BillingCountryId;
                        billingAddress.StateProvinceId = model.BillingStateId;

                        await _addressService.UpdateAddressAsync(billingAddress);
                    }
                }
                if (companys.ShipingAddressId > 0)
                {
                    var shipingAddress = await _addressService.GetAddressByIdAsync(companys.ShipingAddressId);
                    if (shipingAddress != null)
                    {
                        shipingAddress.Address1 = model.ShipingAddress1;
                        shipingAddress.Address2 = model.ShipingAddress2;
                        shipingAddress.City = model.ShipingCity;
                        shipingAddress.ZipPostalCode = model.ShipingZipCode;
                        shipingAddress.CountryId = model.ShipingCountryId;
                        shipingAddress.StateProvinceId = model.ShipingStateId;

                        await _addressService.UpdateAddressAsync(shipingAddress);
                    }
                }
                companys.Id = model.Id;
                companys.ContactId = model.ContactId;
                companys.CompanyName = model.CompanyName;
                companys.WebsiteUrl = model.WebsiteUrl;
                companys.ParentAccountID = model.ParentAccountID;
                companys.AccountNumber = model.AccountNumber;
                companys.AccountTypeId = model.AccountTypeId;
                companys.IndustryId = model.IndustryId;
                companys.CustomerId = model.CustomerId;
                companys.OwnerShipId = model.OwnerShipId;
                companys.Phone = model.Phone;
                companys.NoofEmployee = model.NoofEmployee;
                companys.AnnualRevenue = model.AnnualRevenue;
                companys.Description = model.Description;
                companys.CreatedOnUtc = model.CreatedOnUtc;
                companys.UpdatedOnUtc = DateTime.UtcNow;
                companys.BillingAddressId = model.BillingAddressId;
                companys.ShipingAddressId = model.ShipingAddressId;
                await _companyService.UpdateCompanyAsync(companys);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Company.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = companys.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareCompanyModelAsync(model, companys, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var companys = await _companyService.GetCompanyByIdAsync(id);
            if (companys == null)
                return RedirectToAction("List");
            var billingAddress = await _addressService.GetAddressByIdAsync(companys.BillingAddressId);
            var shipingAddress = await _addressService.GetAddressByIdAsync(companys.ShipingAddressId);
            if (billingAddress != null)
            {
                await _addressService.DeleteAddressAsync(billingAddress); // Delete the address
            }
            if (shipingAddress != null)
            {
                await _addressService.DeleteAddressAsync(shipingAddress); // Delete the address
            }

            await _companyService.DeleteCompanyAsync(companys);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Company.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanies, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _companyService.GetCompanyByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                if (item.BillingAddressId != null && item.BillingAddressId > 0)
                {
                    var bilingAddress = await _addressService.GetAddressByIdAsync(item.BillingAddressId);
                    if (bilingAddress != null)
                    {
                        await _addressService.DeleteAddressAsync(bilingAddress); // Delete the address
                    }
                }
                if (item.ShipingAddressId != null && item.ShipingAddressId > 0)
                {
                    var shipingAddress = await _addressService.GetAddressByIdAsync(item.ShipingAddressId);
                    if (shipingAddress != null)
                    {
                        await _addressService.DeleteAddressAsync(shipingAddress); // Delete the address
                    }
                }
                await _companyService.DeleteCompanyAsync(item);
            }

            return Json(new { Result = true });
        }

        #region Company Contact

        [HttpPost]
        public async Task<IActionResult> CompanyContact(CompanyContactSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyContacts, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            var model = await PrepareCompanyContactListModelAsync(searchModel);

            return Json(model);
        }
        #endregion

        #region ContactsDeals

        [HttpPost]
        public async Task<IActionResult> CompanyDeals(CompanyDealsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyDeals, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            var model = await PrepareCompanyDealsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> CompanyDealsCreate(int companyId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyDeals, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCompanyDealsModelAsync(new CompanyDealsModel(), null);
            model.CompanyId = companyId;
            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/CompanyDealsCreate.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> CompanyDealsCreate(CompanyDealsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyDeals, PermissionAction.Add))
                return AccessDeniedView();

            var deals = new Deals();
            model = await PrepareCompanyDealsModelAsync(model, deals, true);
            model.Contacts = model.Contacts?
        .GroupBy(c => c.Value)
        .Select(g => g.First())
        .ToList() ?? new List<SelectListItem>();

            model.Companys = model.Companys?
                .GroupBy(c => c.Value)
                .Select(g => g.First())
                .ToList() ?? new List<SelectListItem>();
            if (model.ContactId == null || model.ContactId <= 0)
                ModelState.AddModelError(nameof(model.ContactId), "Please select a contact.");

            if (model.CompanyId == null || model.CompanyId <= 0)
                ModelState.AddModelError(nameof(model.CompanyId), "Please select a company.");
            if (model.Contacts == null || model.Contacts.Count <= 1) // only "Select"
                ModelState.AddModelError(nameof(model.ContactId), "No contacts available. Please add a contact before creating a deal.");

            if (model.Companys == null || model.Companys.Count <= 1) // only "Select"
                ModelState.AddModelError(nameof(model.CompanyId), "No companies available. Please add a company before creating a deal.");

            if (ModelState.IsValid)
            {
                deals.Id = model.Id;
                deals.CustomerId = model.CustomerId;
                deals.DealName = model.DealName;
                deals.Amount = model.Amount;
                deals.CompanyId = model.CompanyId ?? 0;
                deals.Probability = model.Probability;
                deals.NextStep = model.NextStep;
                deals.ExpectedRevenue = model.ExpectedRevenue;
                deals.LeadSourceId = model.LeadSourceId;
                deals.ClosingDate = model.ClosingDate;
                deals.ContactId = model.ContactId ?? 0;
                deals.Description = model.Description;
                deals.StageId = model.StageId ?? 0;
                deals.TypeId = model.TypeId ?? 0;
                deals.CreatedOnUtc = DateTime.UtcNow;
                deals.UpdatedOnUtc = DateTime.UtcNow;

                await _dealsService.InsertDealsAsync(deals);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Deals.Added"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Company", new { id = deals.CompanyId });

                return RedirectToAction("CompanyDealsEdit", new { id = deals.Id, compnayId = deals.CompanyId });
            }

            //prepare model
            //model = await PrepareCompanyDealsModelAsync(model, deals, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/CompanyDealsCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> CompanyDealsEdit(int companyId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyDeals, PermissionAction.Edit))
                return AccessDeniedView();

            var deals = await _dealsService.GetDealsByIdAsync(id);
            if (deals == null)
                return RedirectToAction("CompanyDeals");

            //prepare model
            var model = await PrepareCompanyDealsModelAsync(null, deals);
            model.CompanyId = companyId;
            return View("~/Plugins/Misc.SatyanamCRM/Views/Company/CompanyDealsEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> CompanyDealsEdit(CompanyDealsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyDeals, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(model.Id);
            if (deals == null)
                return RedirectToAction("CompanyDeals");

            if (ModelState.IsValid)
            {
                deals = new Deals();
                deals.Id = model.Id;
                deals.CustomerId = model.CustomerId;
                deals.DealName = model.DealName;
                deals.Amount = model.Amount;
                deals.CompanyId = model.CompanyId ?? 0;
                deals.Probability = model.Probability;
                deals.NextStep = model.NextStep;
                deals.ExpectedRevenue = model.ExpectedRevenue;
                deals.LeadSourceId = model.LeadSourceId;
                deals.ClosingDate = model.ClosingDate;
                deals.ContactId = model.ContactId ?? 0;
                deals.Description = model.Description;
                deals.StageId = model.StageId ?? 0;
                deals.TypeId = model.TypeId ?? 0;
                deals.CreatedOnUtc = model.CreatedOnUtc;
                deals.UpdatedOnUtc = DateTime.UtcNow;

                await _dealsService.UpdateDealsAsync(deals);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Deals.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Company", new { id = deals.CompanyId });

                return RedirectToAction("CompanyDealsEdit", new { id = deals.Id, companyId = deals.CompanyId });
            }
            //if we got this far, something failed, redisplay form
            return View("CompanyDeals");
        }

        [HttpPost]
        public virtual async Task<IActionResult> CompanyDealsDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCompanyDeals, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(id);
            if (deals == null)
                return RedirectToAction("CompanyDeals");

            await _dealsService.DeleteDealsAsync(deals);

            return new NullJsonResult();

        }
        #endregion
        #endregion
    }
}
