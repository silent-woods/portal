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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Contacts;
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
    public class ContactsController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IContactsService _contactsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ILeadService _leadService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ITitleService _titleService;
        private readonly ILeadSourceService _leadSourceService;
        private readonly IIndustryService _industryService;
        private readonly ITagsService _tagsService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IAddressService _addressService;
        private readonly IDealsService _dealsService;
        private readonly IEmailverificationService _emailverificationService;
        private readonly ICompanyService _companyService;
        #endregion

        #region Ctor 

        public ContactsController(IPermissionService permissionService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               IContactsService contactsService,
                               ILeadService leadService,
                               IDateTimeHelper dateTimeHelper,
                               ITitleService titleService,
                               ILeadSourceService leadSourceService,
                               IIndustryService industryService,
                               ITagsService tagsService,
                               ICountryService countryService,
                               IStateProvinceService stateProvinceService,
                               IWorkContext workContext,
                               ICustomerService customerService,
                               IAddressService addressService,
                               IDealsService dealsService,
                               IEmailverificationService emailverificationService,
                               ICompanyService companyService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _contactsService = contactsService;
            _leadService = leadService;
            _dateTimeHelper = dateTimeHelper;
            _titleService = titleService;
            _leadSourceService = leadSourceService;
            _industryService = industryService;
            _tagsService = tagsService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
            _customerService = customerService;
            _addressService = addressService;
            _dealsService = dealsService;
            _emailverificationService = emailverificationService;
            _companyService = companyService;
        }

        #endregion

        #region Utilities
        public virtual async Task PrepareTitleListAsync(ContactsModel model)
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
        public virtual async Task PrepareContactsSourcesListAsync(ContactsModel model)
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
        public virtual async Task PrepareIndustryListAsync(ContactsModel model)
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
        public virtual async Task PrepareTagsListAsync(ContactsModel model, int contactsId)
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
        public virtual async Task PrepareCountriesListAsync(ContactsModel model)
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
        public virtual async Task PrepareStatesListAsync(ContactsModel model, int countryId)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.States.Add(new SelectListItem
            {
                Text = "Select State",
                Value = "0" // Default "Select" option
            });

            if (countryId > 0) // Only fetch states if a valid country is selected
            {
                var states = await _stateProvinceService.GetStateProvincesByCountryIdAsync(countryId);
                foreach (var state in states)
                {
                    model.States.Add(new SelectListItem
                    {
                        Text = state.Name,
                        Value = state.Id.ToString()
                    });
                }
            }
        }

        public virtual async Task PrepareCustomersListAsync(ContactsModel model)
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
        public virtual async Task PrepareCompanyNameListAsync(ContactsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Companys = new List<SelectListItem>();

            var companies = await _companyService.GetAllCompanyAsync("", "");

            foreach (var company in companies)
            {
                model.Companys.Add(new SelectListItem
                {
                    Text = company.CompanyName,
                    Value = company.Id.ToString(),
                    Selected = company.Id == model.CompanyId
                });
            }

            // Insert "Select" option at the top only if not selected
            model.Companys.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0",
                Selected = model.CompanyId == 0 || !model.Companys.Any(c => c.Selected)
            });
        }


        public virtual async Task PrepareCompanyListAsync(ContactsDealsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Companys = new List<SelectListItem>();

            var companies = await _companyService.GetAllCompanyAsync("", "");

            foreach (var company in companies)
            {
                model.Companys.Add(new SelectListItem
                {
                    Text = company.CompanyName,
                    Value = company.Id.ToString(),
                    Selected = company.Id == model.CompanyId
                });
            }

            // Insert "Select" option at the top only if not selected
            model.Companys.Insert(0, new SelectListItem
            {
                Text = "Select",
                Value = "0",
                Selected = model.CompanyId == 0 || !model.Companys.Any(c => c.Selected)
            });
            //if (model == null)
            //    throw new ArgumentNullException(nameof(model));

            //model.Companys.Add(new SelectListItem
            //{
            //    Text = "Select",
            //    Value = "0" // Default "Select" option
            //});
            //var company = await _companyService.GetAllCompanyAsync("", "");
            //foreach (var contacts in company)
            //{
            //    model.Companys.Add(new SelectListItem
            //    {
            //        Text = contacts.CompanyName,
            //        Value = contacts.Id.ToString()
            //    });
            //}

        }
        public virtual async Task PrepareCustomersListAsync(ContactsDealsModel model)
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
        public virtual async Task PrepareLeadSourcesListAsync(ContactsDealsModel model)
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
        public virtual async Task PrepareContactsListAsync(ContactsDealsModel model)
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

        public virtual async Task<ContactsSearchModel> PrepareContactsSearchModelAsync(ContactsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<ContactsListModel> PrepareContactsListModelAsync(ContactsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get contacts
            var contacts = await _contactsService.GetAllContactsAsync(showHidden: true,
                name: searchModel.SearchName,
                companyName: searchModel.SearchCompanytName,
                website: searchModel.SearchWebsiteUrl,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ContactsListModel().PrepareToGridAsync(searchModel, contacts, () =>
            {
                //fill in model values from the entity
                return contacts.SelectAwait(async contact =>
                {
                    var contactsModel = new ContactsModel();
                    var selectedAvailableOption = contact.EmailStatusId;
                    contactsModel.Id = contact.Id;
                    contactsModel.FirstName = contact.FirstName;
                    contactsModel.LastName = contact.LastName;
                    contactsModel.CompanyId = contact.CompanyId;
                    var company = await _companyService.GetCompanyByIdAsync(contactsModel.CompanyId);
                    contactsModel.CompanyName = company?.CompanyName ?? "N/A";
                    contactsModel.CompanyName = contact.CompanyName;
                    contactsModel.Phone = contact.Phone;
                    contactsModel.Email = contact.Email;
                    contactsModel.AnnualRevenue = contact.AnnualRevenue;
                    contactsModel.WebsiteUrl = contact.WebsiteUrl;
                    contactsModel.NoofEmployee = contact.NoofEmployee;
                    contactsModel.SkypeId = contact.SkypeId;
                    contactsModel.EmailOptOut = contact.EmailOptOut;
                    contactsModel.Twitter = contact.Twitter;
                    contactsModel.LinkedinUrl = contact.LinkedinUrl;
                    contactsModel.Facebookurl = contact.Facebookurl;
                    contactsModel.SecondaryEmail = contact.SecondaryEmail;
                    contactsModel.Description = contact.Description;
                    contactsModel.TitleId = contact.TitleId;
                    contactsModel.IndustryId = contact.IndustryId;
                    contactsModel.ContactsSourceId = contact.ContactsSourceId;
                    contactsModel.Name = contact.FirstName + " " + contact.LastName;
                    contactsModel.AddressId = contact.AddressId;
                    contactsModel.CustomerId = contact.CustomerId;
                    contactsModel.CompanyId = contact.CompanyId;
                    var leadTags = await _leadService.GetLeadTagByLeadIdAsync(contact.Id);
                    contactsModel.SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();
                    contactsModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(contact.CreatedOnUtc, DateTimeKind.Utc);
                    contactsModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(contact.UpdatedOnUtc, DateTimeKind.Utc);
                    var title = await _titleService.GetTitleByIdAsync(contact.TitleId);
                    contactsModel.TitleName = title?.Name ?? " ";
                    contactsModel.EmailStatusId = contact.EmailStatusId;
                    contactsModel.EmailStatusName = ((EmailValidationStatus)contact.EmailStatusId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) contactsModel.EmailStatusId
                        = (int)((EmailValidationStatus)selectedAvailableOption);
                    return contactsModel;
                });
            });
            return model;
        }

        public virtual async Task<ContactsModel> PrepareContactsModelAsync(ContactsModel model, Contacts contacts, bool excludeProperties = false)
        {
            var status = await EmailValidationStatus.None.ToSelectListAsync();
            if (contacts != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new ContactsModel();
                    model.Id = contacts.Id;
                    model.FirstName = contacts.FirstName;
                    model.LastName = contacts.LastName;
                    model.CompanyId = contacts.CompanyId;
                    var company = await _companyService.GetCompanyByIdAsync(model.CompanyId);
                    model.CompanyName = company?.CompanyName ?? "N/A";
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
                    model.CompanyId = contacts.CompanyId;
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
            await PrepareStatesListAsync(model, Convert.ToInt32(model.CountryId));
            await PrepareCustomersListAsync(model);
            await PrepareCompanyNameListAsync(model);
            return model;
        }


        public virtual async Task<ContactsDealsListModel> PrepareContactDealsListModelAsync(ContactsDealsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var deals = await _dealsService.GetDealsByContactIdAsync(
                contactId: searchModel.ContactId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            var model = await ModelExtensions.PrepareToGridAsync<ContactsDealsListModel, ContactsDealsModel, Deals>(
                new ContactsDealsListModel(),
                searchModel,
                deals,
                () => deals.Select(d => new ContactsDealsModel
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

        public virtual async Task<ContactsDealsModel> PrepareContactDealsModelAsync(ContactsDealsModel model, Deals deals, bool excludeProperties = false)
        {
            var stage = await StageEnum.Qualification.ToSelectListAsync();
            var type = await TypeEnum.None.ToSelectListAsync();
            if (deals != null)
            {
                if (model == null)
                {
                    model = new ContactsDealsModel();
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

        private string NormalizeCompanyName(string name)
        {
            return new string(name
                .Where(c => !char.IsWhiteSpace(c)) // remove all spaces
                .ToArray())
                .ToLowerInvariant(); // make lowercase
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareContactsSearchModelAsync(new ContactsSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ContactsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareContactsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareContactsModelAsync(new ContactsModel(), null);
            
            var companies = await _companyService.GetAllCompanyAsync("", "");
            model.Companys = companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.Id.ToString()
            }).ToList();

            model.Companys.Insert(0, new SelectListItem { Text = "-- Select Company --", Value = "0" });
            model.Companys.Add(new SelectListItem { Text = "+ Add New Company", Value = "-1" });


            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ContactsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.Add))
                return AccessDeniedView();
            if (model.CompanyId == -1 && !string.IsNullOrWhiteSpace(model.NewCompanyName))
            {
                var newCompanyName = model.NewCompanyName.Trim();
                string normalizedNewName = NormalizeCompanyName(model.NewCompanyName);

                var existingCompany = (await _companyService.GetAllCompanyAsync("", ""))
                    .FirstOrDefault(c => NormalizeCompanyName(c.CompanyName) == normalizedNewName);


                if (existingCompany != null)
                {
                    // Company already exists, use its ID
                    model.CompanyId = existingCompany.Id;
                }
                else
                {
                    // Create new company
                    var newCompany = new Company
                    {
                        CompanyName = model.NewCompanyName.Trim(),
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _companyService.InsertCompanyAsync(newCompany);
                    model.CompanyId = newCompany.Id;
                }
            }

            if (ModelState.IsValid)
            {
                var address = new Address
                {
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    City = model.City,
                    ZipPostalCode = model.ZipCode,
                    CountryId = model.CountryId ?? 0,
                    StateProvinceId = model.StateId,
                };
                await _addressService.InsertAddressAsync(address);
                var addressid = address.Id;
                var contacts = new Contacts();

                var companiess = await _companyService.GetAllCompanyAsync("", "");
                model.Companys = companiess.Select(c => new SelectListItem
                {
                    Text = c.CompanyName,
                    Value = c.Id.ToString()
                }).ToList();

                model.Companys.Insert(0, new SelectListItem { Text = "-- Select Company --", Value = "0" });
                model.Companys.Add(new SelectListItem { Text = "+ Add New Company", Value = "-1" });

                contacts.Id = model.Id;
                contacts.FirstName = model.FirstName;
                contacts.LastName = model.LastName;
                var company = await _companyService.GetCompanyByIdAsync(model.CompanyId);
                contacts.CompanyName = company?.CompanyName ?? "N/A";
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
                contacts.ContactsSourceId = model.ContactsSourceId;
                contacts.IndustryId = model.IndustryId;
                contacts.CreatedOnUtc = DateTime.UtcNow;
                contacts.UpdatedOnUtc = DateTime.UtcNow;
                contacts.AddressId = addressid;
                contacts.CustomerId = model.CustomerId;
                contacts.CompanyId = model.CompanyId;
                contacts.EmailStatusId = model.EmailStatusId;
                contacts.EmailStatusId = (int)await GetEmailValidationStatusAsync(model.Email);
                await _contactsService.InsertContactsAsync(contacts);
                if (model.SelectedTagIds != null && model.SelectedTagIds.Any())
                {
                    foreach (var tagId in model.SelectedTagIds)
                    {
                        var contactsTags = new ContactsTags
                        {
                            ContactsId = contacts.Id,
                            TagsId = tagId
                        };
                        await _contactsService.InsertContactsTagsAsync(contactsTags);
                    }
                }

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Contacts.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = contacts.Id });
            }

            //prepare model
            model = await PrepareContactsModelAsync(model, null, true);
            var companies = await _companyService.GetAllCompanyAsync("", "");
            model.Companys = companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.Id.ToString()
            }).ToList();
            model.Companys.Insert(0, new SelectListItem { Text = "-- Select Company --", Value = "0" });
            model.Companys.Add(new SelectListItem { Text = "+ Add New Company", Value = "-1" });
            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a contacts with the specified id
            var contacts = await _contactsService.GetContactsByIdAsync(id);
            if (contacts == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareContactsModelAsync(null, contacts);

            var companies = await _companyService.GetAllCompanyAsync("", "");
            model.Companys = companies.Select(c => new SelectListItem
            {
                Text = c.CompanyName,
                Value = c.Id.ToString()
            }).ToList();

            model.Companys.Insert(0, new SelectListItem { Text = "-- Select Company --", Value = "0" });
            model.Companys.Add(new SelectListItem { Text = "+ Add New Company", Value = "-1" });
            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ContactsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a contacts with the specified id
            var contacts = await _contactsService.GetContactsByIdAsync(model.Id);
            if (contacts == null)
                return RedirectToAction("List");

            if (model.CompanyId == -1 && !string.IsNullOrWhiteSpace(model.NewCompanyName))
            {
                var newCompanyName = model.NewCompanyName.Trim();

                // Case-insensitive check
                string normalizedNewName = NormalizeCompanyName(model.NewCompanyName);

                var existingCompany = (await _companyService.GetAllCompanyAsync("", ""))
                    .FirstOrDefault(c => NormalizeCompanyName(c.CompanyName) == normalizedNewName);


                if (existingCompany != null)
                {
                    model.CompanyId = existingCompany.Id;
                }
                else
                {
                    var newCompany = new Company
                    {
                        CompanyName = newCompanyName,
                        CreatedOnUtc = DateTime.UtcNow,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    await _companyService.InsertCompanyAsync(newCompany);
                    model.CompanyId = newCompany.Id;
                }
            }


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

                //leads = new Lead();
                contacts.Id = model.Id;
                contacts.FirstName = model.FirstName;
                contacts.LastName = model.LastName;
                contacts.CompanyId = model.CompanyId;
                var company = await _companyService.GetCompanyByIdAsync(contacts.CompanyId);
                contacts.CompanyName = company?.CompanyName ?? "N/A";
                contacts.Phone = model.Phone;
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

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Contacts.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = contacts.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareContactsModelAsync(model, contacts, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var contact = await _contactsService.GetContactsByIdAsync(id);
            if (contact == null)
                return RedirectToAction("List");
            await _contactsService.DeleteTagsByContactsIdAsync(contact.Id);
            await _contactsService.DeleteContactsAsync(contact);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Contact.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContacts, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _contactsService.GetContactsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _contactsService.DeleteTagsByContactsIdAsync(item.Id);
                if (item.AddressId != null && item.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(item.AddressId);
                    if (address != null)
                    {
                        await _addressService.DeleteAddressAsync(address); // Delete the address
                    }
                }
                await _contactsService.DeleteContactsAsync(item);
            }
            //_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Contact.Deleted"));
            return Json(new { Result = true });
        }

        #region ContactsDeals

        [HttpPost]
        public async Task<IActionResult> ContactDeals(ContactsDealsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContactDeals, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            var model = await PrepareContactDealsListModelAsync(searchModel);

            return Json(model);
        }
        public virtual async Task<IActionResult> ContactsDealsCreate(int contactId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContactDeals, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareContactDealsModelAsync(new ContactsDealsModel(), null);
            model.ContactId = contactId;
            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/ContactsDealsCreate.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ContactsDealsCreate(ContactsDealsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContactDeals, PermissionAction.Add))
                return AccessDeniedView();
            var deals = new Deals();
            model = await PrepareContactDealsModelAsync(model, deals, true);
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
                    return RedirectToAction("Edit", "Contacts", new { id = deals.ContactId });

                return RedirectToAction("ContactsDealsEdit", new { id = deals.Id, contactId = deals.ContactId });
            }

            //prepare model
            //model = await PrepareContactDealsModelAsync(model, deals, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/ContactsDealsCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> ContactsDealsEdit(int contactId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContactDeals, PermissionAction.Edit))
                return AccessDeniedView();

            var deals = await _dealsService.GetDealsByIdAsync(id);
            if (deals == null)
                return RedirectToAction("ContactDeals");

            //prepare model
            var model = await PrepareContactDealsModelAsync(null, deals);
            model.ContactId = contactId;
            return View("~/Plugins/Misc.SatyanamCRM/Views/Contacts/ContactDealsEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ContactsDealsEdit(ContactsDealsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContactDeals, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(model.Id);
            if (deals == null)
                return RedirectToAction("ContactDeals");

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
                    return RedirectToAction("Edit", "Contacts", new { id = deals.ContactId });

                return RedirectToAction("ContactsDealsEdit", new { id = deals.Id, contactId = deals.ContactId });
            }
            //if we got this far, something failed, redisplay form
            return View("ContactDeals");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ContactDealsDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageContactDeals, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var deals = await _dealsService.GetDealsByIdAsync(id);
            if (deals == null)
                return RedirectToAction("ContactDeals");

            await _dealsService.DeleteDealsAsync(deals);

            return new NullJsonResult();

        }
        #endregion


        #endregion
    }
}
