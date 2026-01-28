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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Deals;
using Satyanam.Plugin.Misc.EmailVerification.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DealsController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IDealsService _dealsService;
        private readonly IContactsService _contactsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly ILeadSourceService _leadSourceService;
        private readonly ILeadService _leadService;
        private readonly ITitleService _titleService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IIndustryService _industryService;
        private readonly ITagsService _tagsService;
        private readonly ICompanyService _companyService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAccountTypeService _accountTypeService;
        private readonly IEmailverificationService _emailverificationService;
        #endregion

        #region Ctor 

        public DealsController(IPermissionService permissionService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               IDealsService dealsService,
                               IDateTimeHelper dateTimeHelper,
                               IContactsService contactsService,
                               IWorkContext workContext,
                               ICustomerService customerService,
                               ILeadSourceService leadSourceService,
                               ILeadService leadService,
                               IAddressService addressService,
                               ICountryService countryService,
                               IIndustryService industryService,
                               ITagsService tagsService,
                               ITitleService titleService,
                               ICompanyService companyService,
                               IStateProvinceService stateProvinceService,
                               IAccountTypeService accountTypeService,
                               IEmailverificationService emailverificationService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _dealsService = dealsService;
            _dateTimeHelper = dateTimeHelper;
            _contactsService = contactsService;
            _workContext = workContext;
            _customerService = customerService;
            _leadSourceService = leadSourceService;
            _leadService = leadService;
            _addressService = addressService;
            _countryService = countryService;
            _industryService = industryService;
            _tagsService = tagsService;
            _titleService = titleService;
            _companyService = companyService;
            _stateProvinceService = stateProvinceService;
            _accountTypeService = accountTypeService;
            _emailverificationService = emailverificationService;
        }

        #endregion

        #region Utilities
        public virtual async Task PrepareLeadSourcesListAsync(DealsModel model)
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
        public virtual async Task PrepareCompanyListAsync(DealsModel model)
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
        public virtual async Task PrepareCustomersListAsync(DealsModel model)
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
        public virtual async Task PrepareCustomersListAsync(DealsContactModel model)
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
        }
        public virtual async Task PrepareContactsListAsync(DealsModel model)
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
        public virtual async Task PrepareTitleListAsync(DealsContactModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Titles.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var titles = await _titleService.GetAllTitleByNameAsync("");
            foreach (var p in titles)
            {
                model.Titles.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareContactsSourcesListAsync(DealsContactModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.ContactsSources.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var leadSource = await _leadSourceService.GetAllLeadSourceByNameAsync("");
            foreach (var p in leadSource)
            {
                model.ContactsSources.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareIndustryListAsync(DealsContactModel model)
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
        public virtual async Task PrepareIndustryListAsync(DealsCompanyModel model)
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
        public virtual async Task PrepareTagsListAsync(DealsContactModel model, int contactsId)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Tags = new List<SelectListItem>();

            var tags = await _tagsService.GetAllTagsAsync("");
            foreach (var p in tags)
            {
                model.Tags.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString(),
                    Selected = model.SelectedTagIds.Contains(p.Id)
                });
            }
            var leadTags = await _contactsService.GetContactsTagByContactsIdAsync(contactsId);
            model.SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();

        }
        public virtual async Task PrepareCountriesListAsync(DealsContactModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Countrys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Usually, "0" represents a "Select" option in dropdowns.
            });

            var countries = await _countryService.GetAllCountriesAsync();
            foreach (var country in countries)
            {
                model.Countrys.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(country, x => x.Name),
                    Value = country.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareCustomersListAsync(DealsCompanyModel model)
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
        }

        public virtual async Task PrepareBullingCountriesListAsync(DealsCompanyModel model)
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

        public virtual async Task PrepareShipingCountriesListAsync(DealsCompanyModel model)
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
        public virtual async Task PrepareBillingStatesListAsync(DealsCompanyModel model, int countryId)
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
        public virtual async Task PrepareAccountListAsync(DealsCompanyModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.ParentAccounts.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0" // Usually, "0" represents a "Select" option in dropdowns.
            });

            var contacts = await _contactsService.GetAllContactsAsync("", "", "");
            foreach (var contact in contacts)
            {
                model.ParentAccounts.Add(new SelectListItem
                {
                    Text = contact.CompanyName,
                    Value = contact.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareAccountTypeListAsync(DealsCompanyModel model)
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
        public virtual async Task PrepareShipingStatesListAsync(DealsCompanyModel model, int countryId)
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
        public virtual async Task PrepareContactsListAsync(DealsCompanyModel model)
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
        private async Task<EmailValidationStatus> GetEmailValidationStatusAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return EmailValidationStatus.None;

            string verificationResult = await _emailverificationService.VerifyEmailApi(email);

            // Handle session/limit expired
            if (verificationResult == "__SESSION_EXPIRED__")
            {
                _notificationService.WarningNotification("Email verification limit reached or session expired. Skipping validation.");
                return EmailValidationStatus.None;
            }

            if (!string.IsNullOrWhiteSpace(verificationResult))
            {
                dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                var result = ((string)verificationResponse?.result)?.ToLowerInvariant();
                var safeToSend = ((string)verificationResponse?.safe_to_send)?.ToLowerInvariant();

                if (result == "valid" && safeToSend == "true")
                    return EmailValidationStatus.Valid;
                else if (result == "invalid" || safeToSend == "false")
                    return EmailValidationStatus.Invalid;
            }

            return EmailValidationStatus.None;
        }

        public virtual async Task<DealsSearchModel> PrepareDealsSearchModelAsync(DealsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Stages = (await StageEnum.Qualification.ToSelectListAsync()).ToList();
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<DealsListModel> PrepareDealsListModelAsync(DealsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get deals
            var deals = await _dealsService.GetAllDealsAsync(showHidden: true,
                name: searchModel.DealName,
                amount: searchModel.Amount,
                stageid: searchModel.StageId,
                closingdate: searchModel.ClosingDate,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new DealsListModel().PrepareToGridAsync(searchModel, deals, () =>
            {
                //fill in model values from the entity
                return deals.SelectAwait(async deal =>
                {
                    var dealsModel = new DealsModel();
                    var selectedAvailableOption = deal.StageId;
                    var selectAvailableOption = deal.TypeId;
                    dealsModel.Id = deal.Id;
                    dealsModel.CustomerId = deal.CustomerId;
                    dealsModel.DealName = deal.DealName;
                    dealsModel.Amount = deal.Amount;
                    dealsModel.CompanyId = deal.CompanyId;
                    dealsModel.Probability = deal.Probability;
                    dealsModel.NextStep = deal.NextStep;
                    dealsModel.ExpectedRevenue = deal.ExpectedRevenue;
                    dealsModel.LeadSourceId = deal.LeadSourceId;
                    dealsModel.ClosingDate = deal.ClosingDate;
                    dealsModel.Date =deal.ClosingDate.HasValue ? deal.ClosingDate.Value.ToString("MM/dd/yyyy") : "";
                    dealsModel.ContactId = deal.ContactId;
                    dealsModel.Description = deal.Description;
                    dealsModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(deal.CreatedOnUtc, DateTimeKind.Utc);
                    dealsModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(deal.UpdatedOnUtc, DateTimeKind.Utc);

                    dealsModel.StageId = deal.StageId;
                    dealsModel.StageName = ((StageEnum)deal.StageId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) dealsModel.StageId
                        = (int)((StageEnum)selectedAvailableOption);

                    dealsModel.TypeId = deal.TypeId;
                    dealsModel.TypeName = ((TypeEnum)deal.TypeId).ToString();
                    if (selectAvailableOption != 0 || selectAvailableOption != null) dealsModel.TypeId
                        = (int)((TypeEnum)selectAvailableOption);

                    return dealsModel;
                });
            });
            return model;
        }

        public virtual async Task<DealsModel> PrepareDealsModelAsync(DealsModel model, Deals deals, bool excludeProperties = false)
        {
            var stage = await StageEnum.Select.ToSelectListAsync();
            var type = await TypeEnum.None.ToSelectListAsync();
            if (deals != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new DealsModel();
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

        public virtual async Task<DealsContactListModel> PrepareContactDealsListModelAsync(DealsContactSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var contactPagedList = await _contactsService.GetDealsByContactsIdAsync(
                contactsId: searchModel.ContactId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            var contactModels = new List<DealsContactModel>();

            foreach (var d in contactPagedList)
            {
                if (d == null)
                    continue;

                var leadTags = await _leadService.GetLeadTagByLeadIdAsync(d.Id);
                var title = d.TitleId > 0 && _titleService != null
            ? await _titleService.GetTitleByIdAsync(d.TitleId)
            : null;

                var createdOn = await _dateTimeHelper.ConvertToUserTimeAsync(d.CreatedOnUtc, DateTimeKind.Utc);
                var updatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(d.UpdatedOnUtc, DateTimeKind.Utc);

                var emailStatusId = d.EmailStatusId;
                var emailStatusName = emailStatusId > 0
                    ? ((EmailValidationStatus)emailStatusId).ToString()
                    : "None";
                contactModels.Add(new DealsContactModel
                {
                    Id = d.Id,
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    CompanyName = d.CompanyName,
                    Phone = d.Phone,
                    Email = d.Email,
                    AnnualRevenue = d.AnnualRevenue,
                    WebsiteUrl = d.WebsiteUrl,
                    NoofEmployee = d.NoofEmployee,
                    SkypeId = d.SkypeId,
                    EmailOptOut = d.EmailOptOut,
                    Twitter = d.Twitter,
                    LinkedinUrl = d.LinkedinUrl,
                    Facebookurl = d.Facebookurl,
                    SecondaryEmail = d.SecondaryEmail,
                    Description = d.Description,
                    TitleId = d.TitleId,
                    IndustryId = d.IndustryId,
                    ContactsSourceId = d.ContactsSourceId,
                    Name = d.FirstName + " " + d.LastName,
                    AddressId = d.AddressId,
                    CustomerId = d.CustomerId,
                    SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>(),
                    CreatedOnUtc = createdOn,
                    UpdatedOnUtc = updatedOn,
                    TitleName = title?.Name ?? string.Empty,
                    EmailStatusId = emailStatusId,
                    EmailStatusName = emailStatusName
                });
            }

            return new DealsContactListModel().PrepareToGrid(searchModel, contactPagedList, () => contactModels);
        }
        public virtual async Task<DealsContactModel> PrepareContactDealsModelAsync(DealsContactModel model, Contacts contacts, bool excludeProperties = false)
        {
            var stage = await StageEnum.Qualification.ToSelectListAsync();
            var type = await TypeEnum.None.ToSelectListAsync();
            var status = await EmailValidationStatus.None.ToSelectListAsync();
            if (contacts != null)
            {
                if (model == null)
                {
                    model = new DealsContactModel();
                    model.Id = contacts.Id;
                    model.FirstName = contacts.FirstName;
                    model.LastName = contacts.LastName;
                    model.CompanyName = contacts.CompanyName;
                    model.Phone = contacts.Phone;
                    model.Email = contacts.Email;
                    model.AnnualRevenue = contacts.AnnualRevenue;
                    model.WebsiteUrl = contacts.WebsiteUrl;
                    model.NoofEmployee = contacts.NoofEmployee;
                    model.SkypeId = contacts.SkypeId;
                    model.EmailOptOut = contacts.EmailOptOut;
                    model.SecondaryEmail = contacts.SecondaryEmail;
                    model.Twitter = contacts.Twitter;
                    model.LinkedinUrl = contacts.LinkedinUrl;
                    model.Facebookurl = contacts.Facebookurl;
                    model.Description = contacts.Description;
                    model.TitleId = contacts.TitleId;
                    model.IndustryId = contacts.IndustryId;
                    model.ContactsSourceId = contacts.ContactsSourceId;
                    model.Name = contacts.FirstName + " " + contacts.LastName;
                    model.CustomerId = contacts.CustomerId;
                    model.EmailStatusId = contacts.EmailStatusId;
                    model.CreatedOnUtc = contacts.CreatedOnUtc;
                    model.UpdatedOnUtc = contacts.UpdatedOnUtc;
                    if (contacts.AddressId > 0)
                    {
                        var address = await _addressService.GetAddressByIdAsync(contacts.AddressId);
                        if (address != null)
                        {
                            model.Address1 = address.Address1;
                            model.Address2 = address.Address2;
                            model.City = address.City;
                            model.ZipCode = address.ZipPostalCode;
                            model.CountryId = address.CountryId;
                            model.StateId = address.StateProvinceId;
                            model.AddressId = address.Id;
                        }
                    }

                    var leadTags = await _contactsService.GetContactsTagByContactsIdAsync(contacts.Id);
                    model.SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();
                }
            }
                    model.EmailStatus = status.Select(store => new SelectListItem
                    {
                        Value = store.Value,
                        Text = store.Text,
                        Selected = model.EmailStatusId.ToString() == store.Value
                    }).ToList();
            await PrepareTitleListAsync(model);
            await PrepareContactsSourcesListAsync(model);
            await PrepareIndustryListAsync(model);
            await PrepareTagsListAsync(model, model.Id);
            await PrepareCountriesListAsync(model);
            await PrepareCustomersListAsync(model);

            return model;
        }
        public virtual async Task<DealsCompanyListModel> PrepareCompanyDealsListModelAsync(DealsCompanySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var companyPagedList = await _companyService.GetDealsByCompanyIdAsync(
                companyId: searchModel.CompanyId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            var companyModels = new List<DealsCompanyModel>();

            foreach (var d in companyPagedList)
            {
                if (d == null)
                    continue;

                var leadTags = await _leadService.GetLeadTagByLeadIdAsync(d.Id);
                var createdOn = await _dateTimeHelper.ConvertToUserTimeAsync(d.CreatedOnUtc, DateTimeKind.Utc);
                var updatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(d.UpdatedOnUtc, DateTimeKind.Utc);

                var selectedAvailableOption = d.OwnerShipId;

                var companyModel = new DealsCompanyModel
                {
                    Id = d.Id,
                    ContactId = d.ContactId,
                    CompanyName = d.CompanyName,
                    WebsiteUrl = d.WebsiteUrl,
                    ParentAccountID = d.ParentAccountID,
                    AccountNumber = d.AccountNumber,
                    AccountTypeId = d.AccountTypeId,
                    IndustryId = d.IndustryId,
                    CustomerId = d.CustomerId,
                    Phone = d.Phone,
                    NoofEmployee = d.NoofEmployee,
                    AnnualRevenue = d.AnnualRevenue,
                    BillingAddressId = d.BillingAddressId,
                    ShipingAddressId = d.ShipingAddressId,
                    Description = d.Description,
                    CreatedOnUtc = createdOn,
                    UpdatedOnUtc = updatedOn,
                    OwnerShipId = d.OwnerShipId,
                    OwnerShipName = ((OwnereShipEnum)d.OwnerShipId).ToString()
                };

                // Optional: only override OwnerShipId if needed
                if (selectedAvailableOption != 0)
                {
                    companyModel.OwnerShipId = (int)((OwnereShipEnum)selectedAvailableOption);
                }

                companyModels.Add(companyModel);
            }

            return new DealsCompanyListModel().PrepareToGrid(searchModel, companyPagedList, () => companyModels);
        }
        public virtual async Task<DealsCompanyModel> PrepareCompanyDealsModelAsync(DealsCompanyModel model, Company company, bool excludeProperties = false)
        {
            var ownerShip = await OwnereShipEnum.None.ToSelectListAsync();
            if (company != null)
            {
                if (model == null)
                {
                    model = new DealsCompanyModel();
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
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareDealsSearchModelAsync(new DealsSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(DealsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareDealsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareDealsModelAsync(new DealsModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(DealsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.Add))
                return AccessDeniedView();

            model = await PrepareDealsModelAsync(model, null, true);
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
            // 👇 Additional validation: check if no Contacts or Companies exist
            if (model.Contacts == null || model.Contacts.Count <= 1) // only "Select"
                ModelState.AddModelError(nameof(model.ContactId), "No contacts available. Please add a contact before creating a deal.");

            if (model.Companys == null || model.Companys.Count <= 1) // only "Select"
                ModelState.AddModelError(nameof(model.CompanyId), "No companies available. Please add a company before creating a deal.");
            if (ModelState.IsValid)
            {
                var deals = new Deals();
                deals.Id = model.Id;
                deals.CustomerId = model.CustomerId;
                deals.DealName = model.DealName;
                deals.Amount = model.Amount;
                deals.CompanyId = model.CompanyId??0;
                deals.Probability = model.Probability;
                deals.NextStep = model.NextStep;
                deals.ExpectedRevenue = model.ExpectedRevenue;
                deals.LeadSourceId = model.LeadSourceId;
                deals.ClosingDate = model.ClosingDate;
                deals.ContactId = model.ContactId ?? 0;
                deals.Description = model.Description;
                deals.StageId = model.StageId??0;
                deals.TypeId = model.TypeId ?? 0;
                deals.CreatedOnUtc = DateTime.UtcNow;
                deals.UpdatedOnUtc = DateTime.UtcNow;

                await _dealsService.InsertDealsAsync(deals);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Deals.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = deals.Id });
            }

            //prepare model
            //model = await PrepareDealsModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(id);
            if (deals == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareDealsModelAsync(null, deals);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(DealsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(model.Id);
            if (deals == null)
                return RedirectToAction("List");

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
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = deals.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareDealsModelAsync(model, deals, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(id);

            if (deals == null)
                return RedirectToAction("List");

            await _dealsService.DeleteDealsAsync(deals);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Deals.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDeals, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _dealsService.GetDealsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _dealsService.DeleteDealsAsync(item);
            }

            return Json(new { Result = true });
        }

        #region DealsContact

        [HttpPost]
        public async Task<IActionResult> DealsContact(DealsContactSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealContacts, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            var model = await PrepareContactDealsListModelAsync(searchModel);

            return Json(model);
        }
       
        public virtual async Task<IActionResult> DealsContactEdit(int contactsId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealContacts, PermissionAction.Edit))
                return AccessDeniedView();

            var contacts = await _contactsService.GetContactsByIdAsync(id);
            if (contacts == null)
                return RedirectToAction("DealsContact");

            //prepare model
            var model = await PrepareContactDealsModelAsync(null, contacts);
            model.Id = contactsId;
            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/DealsContactEdit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> DealsContactEdit(DealsContactModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealContacts, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a contacts with the specified id
            var contacts = await _contactsService.GetContactsByIdAsync(model.Id);
            if (contacts == null)
                return RedirectToAction("DealsContact");

            if (ModelState.IsValid)
            {
                if (contacts.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(contacts.AddressId);
                    if (address != null)
                    {
                        address.Address1 = model.Address1;
                        address.Address2 = model.Address2;
                        address.City = model.City;
                        address.ZipPostalCode = model.ZipCode;
                        address.CountryId = model.CountryId;
                        address.StateProvinceId = model.StateId;

                        await _addressService.UpdateAddressAsync(address);
                    }
                }

                contacts.Id = model.Id;
                contacts.FirstName = model.FirstName;
                contacts.LastName = model.LastName;
                contacts.CompanyName = model.CompanyName;
                contacts.Phone = model.Phone;
                contacts.Email = model.Email;
                contacts.AnnualRevenue = model.AnnualRevenue;
                contacts.WebsiteUrl = model.WebsiteUrl;
                contacts.NoofEmployee = model.NoofEmployee;
                contacts.SkypeId = model.SkypeId;
                contacts.SecondaryEmail = model.SecondaryEmail;
                contacts.EmailOptOut = model.EmailOptOut;
                contacts.Twitter = model.Twitter;
                contacts.LinkedinUrl = model.LinkedinUrl;
                contacts.Facebookurl = model.Facebookurl;
                contacts.Description = model.Description;
                contacts.TitleId = model.TitleId;
                contacts.IndustryId = model.IndustryId;
                contacts.ContactsSourceId = model.ContactsSourceId;
                contacts.CustomerId = model.CustomerId;
                contacts.CreatedOnUtc = model.CreatedOnUtc;
                contacts.UpdatedOnUtc = DateTime.UtcNow;
                contacts.AddressId = model.AddressId;
                model.Name = contacts.FirstName + " " + contacts.LastName;

                if (!string.Equals(contacts.Email, model.Email, StringComparison.InvariantCultureIgnoreCase))
                {
                    contacts.EmailStatusId = (int)await GetEmailValidationStatusAsync(model.Email);
                }

                contacts.Email = model.Email;
                model.Name = contacts.FirstName + " " + contacts.LastName;
                await _contactsService.UpdateContactsAsync(contacts);

                var existingTags = await _contactsService.GetContactsTagByContactsIdAsync(model.Id);
                var existingTagIds = existingTags?.Select(t => t.TagsId).ToList() ?? new List<int>();

                // Remove tags that are no longer selected
                foreach (var tag in existingTags)
                {
                    if (!model.SelectedTagIds.Contains(tag.TagsId))
                    {
                        await _contactsService.DeleteContactsTagsByLeadIdAsync(contacts.Id, tag.TagsId); // Ensure proper delete logic
                    }
                }

                // Add new tags that do not exist in the database yet
                foreach (var tagId in model.SelectedTagIds)
                {
                    if (!existingTagIds.Contains(tagId))
                    {
                        var newContactsTag = new ContactsTags
                        {
                            ContactsId = contacts.Id,
                            TagsId = tagId
                        };
                        await _contactsService.InsertContactsTagsAsync(newContactsTag);
                    }
                }

                // Refresh selected tags list to ensure correctness
                model.SelectedTagIds = (await _contactsService.GetContactsTagByContactsIdAsync(contacts.Id))
                    ?.Select(t => t.TagsId)
                    .ToList() ?? new List<int>();
                var dealId = await _dealsService.GetDealIdByContactIdAsync(contacts.Id);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Contacts.Updated"));

                if (!continueEditing)
                {
                    if (dealId > 0)
                    {
                        // Redirect to the Deal Edit page
                        return RedirectToAction("Edit", "Deals", new { id = dealId });
                    }
                    else
                    {
                        // If no associated deal is found, redirect to the Deals list page
                        return RedirectToAction("List", "Deals");
                    }
                }
                return RedirectToAction("DealsContactEdit", new { id = contacts.Id, contactsId = contacts.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("DealsContact");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DealsContactDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealContacts, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a contacts with the specified id
            var contacts = await _contactsService.GetContactsByIdAsync(id);
            if (contacts == null)
                return RedirectToAction("DealsContact");

            await _contactsService.DeleteContactsAsync(contacts);

            return new NullJsonResult();

        }
        #endregion

        #region DealsCompany
        [HttpPost]
        public async Task<IActionResult> DealsCompany(DealsCompanySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealCompanies, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            var model = await PrepareCompanyDealsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> DealsCompanyEdit(int companyId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealCompanies, PermissionAction.Edit))
                return AccessDeniedView();

            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
                return RedirectToAction("DealsCompany");

            //prepare model
            var model = await PrepareCompanyDealsModelAsync(null, company);
            model.Id = companyId;
            return View("~/Plugins/Misc.SatyanamCRM/Views/Deals/DealsCompanyEdit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> DealsCompanyEdit(DealsCompanyModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealCompanies, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a company with the specified id
            var company = await _companyService.GetCompanyByIdAsync(model.Id);
            if (company == null)
                return RedirectToAction("DealsCompany");

            if (ModelState.IsValid)
            {
                if (company.BillingAddressId > 0)
                {
                    var billingAddress = await _addressService.GetAddressByIdAsync(company.BillingAddressId);
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
                if (company.ShipingAddressId > 0)
                {
                    var shipingAddress = await _addressService.GetAddressByIdAsync(company.ShipingAddressId);
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
                company.Id = model.Id;
                company.ContactId = model.ContactId;
                company.CompanyName = model.CompanyName;
                company.WebsiteUrl = model.WebsiteUrl;
                company.ParentAccountID = model.ParentAccountID;
                company.AccountNumber = model.AccountNumber;
                company.AccountTypeId = model.AccountTypeId;
                company.IndustryId = model.IndustryId;
                company.CustomerId = model.CustomerId;
                company.OwnerShipId = model.OwnerShipId;
                company.Phone = model.Phone;
                company.NoofEmployee = model.NoofEmployee;
                company.AnnualRevenue = model.AnnualRevenue;
                company.Description = model.Description;
                company.CreatedOnUtc = model.CreatedOnUtc;
                company.UpdatedOnUtc = DateTime.UtcNow;
                company.BillingAddressId = model.BillingAddressId;
                company.ShipingAddressId = model.ShipingAddressId;
                await _companyService.UpdateCompanyAsync(company);
                
                var dealId = await _dealsService.GetDealIdByCompanyIdAsync(company.Id);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Company.Updated"));

                if (!continueEditing)
                {
                    if (dealId > 0)
                    {
                        // Redirect to the Deal Edit page
                        return RedirectToAction("Edit", "Deals", new { id = dealId });
                    }
                    else
                    {
                        // If no associated deal is found, redirect to the Deals list page
                        return RedirectToAction("List", "Deals");
                    }
                }

                return RedirectToAction("DealsCompanyEdit", new { id = company.Id, companyId = company.Id });
            }
            //if we got this far, something failed, redisplay form
            return View("DealsCompany");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DealsCompanyDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageDealCompanies, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a contacts with the specified id
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
                return RedirectToAction("DealsCompany");

            await _companyService.DeleteCompanyAsync(company);

            return new NullJsonResult();

        }
        #endregion
        #endregion
    }
}
