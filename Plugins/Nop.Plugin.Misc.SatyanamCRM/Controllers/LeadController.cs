using App.Core;
using App.Core.Domain.Common;
using App.Core.Domain.Directory;
using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Services;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.ExportImport;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using Satyanam.Plugin.Misc.EmailVerification.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class LeadController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILeadService _leadService;
        private readonly ITitleService _titleService;
        private readonly ITagsService _tagsService;
        private readonly ICategorysService _categorysService;
        private readonly ILeadSourceService _leadSourceService;
        private readonly ILeadStatusService _leadStatusService;
        private readonly IIndustryService _industryService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IExportManager _exportManager;
        private readonly ILeadImportService _leadImportService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IContactsService _contactsService;
        private readonly ICompanyService _companyService;
        private readonly IDealsService _dealsService;
        private readonly IEmailverificationService _emailverificationService;
        private readonly ILeadExportService _leadExportService;

        #endregion

        #region Ctor 

        public LeadController(IPermissionService permissionService,
                               ILeadService leadService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               ITitleService titleService,
                               ITagsService tagsService,
                               ICategorysService categorysService,
                               ILeadSourceService leadSourceService,
                               ILeadStatusService leadStatusService,
                               IIndustryService industryService,
                               ICountryService countryService,
                               IStateProvinceService stateProvinceService,
                               IAddressService addressService,
                               ICustomerService customerService,
                               IWorkContext workContext,
                               IExportManager exportManager,
                               ILeadImportService leadImportService,
                               IDateTimeHelper dateTimeHelper,
                               IContactsService contactsService,
                               ICompanyService companyService,
                               IDealsService dealsService,
                               IEmailverificationService emailverificationService,
                               ILeadExportService leadExportService)
        {
            _permissionService = permissionService;
            _leadService = leadService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _titleService = titleService;
            _tagsService = tagsService;
            _categorysService = categorysService;
            _leadSourceService = leadSourceService;
            _leadStatusService = leadStatusService;
            _industryService = industryService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _customerService = customerService;
            _workContext = workContext;
            _exportManager = exportManager;
            _leadImportService = leadImportService;
            _dateTimeHelper = dateTimeHelper;
            _contactsService = contactsService;
            _companyService = companyService;
            _dealsService = dealsService;
            _emailverificationService = emailverificationService;
            _leadExportService = leadExportService;
        }

        #endregion

        #region Utilities
        public virtual async Task PrepareTitleListAsync(LeadModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Titles.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var titleName = "";
            var titles = await _titleService.GetAllTitleByNameAsync(titleName);
            foreach (var p in titles)
            {
                model.Titles.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareTitleListAsync(LeadSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var titles = await _titleService.GetAllTitleByNameAsync("");
            foreach (var p in titles)
            {
                searchModel.AvailableTitles.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareLeadSourcesListAsync(LeadModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.LeadSources.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var sourcesName = "";
            var leadSource = await _leadSourceService.GetAllLeadSourceByNameAsync(sourcesName);
            foreach (var p in leadSource)
            {
                model.LeadSources.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareIndustryListAsync(LeadModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Industrys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var industryName = "";
            var industrys = await _industryService.GetAllIndustryByNameAsync(industryName);
            foreach (var p in industrys)
            {
                model.Industrys.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareLeadStatusListAsync(LeadModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.LeadStatus.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var leadStatusName = "";
            var leadStatus = await _leadStatusService.GetAllLeadStatusByNameAsync(leadStatusName);
            foreach (var p in leadStatus)
            {
                model.LeadStatus.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareCategorysListAsync(LeadModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Categorys.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var categorysName = "";
            var categorys = await _categorysService.GetAllCategorysByNameAsync(categorysName);
            foreach (var p in categorys)
            {
                model.Categorys.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareTagsListAsync(LeadModel model, int leadId)
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
            var leadTags = await _leadService.GetLeadTagByLeadIdAsync(leadId);
            model.SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();

        }

        public virtual async Task PrepareCountriesListAsync(LeadModel model)
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
        public virtual async Task PrepareStatesListAsync(LeadModel model, int countryId)
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
        public virtual async Task PrepareCustomersListAsync(LeadModel model)
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

        public virtual async Task PrepareTagsListAsync(LeadSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Tags = new List<SelectListItem>();

            // Get all tags from your tag service
            var tags = await _tagsService.GetAllTagsAsync("");

            // Loop through the tags and build the SelectList
            foreach (var tag in tags)
            {
                model.Tags.Add(new SelectListItem
                {
                    Text = tag.Name,
                    Value = tag.Id.ToString(),
                    Selected = model.SelectedTags != null && model.SelectedTags.Contains(tag.Id)
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

       

        public virtual async Task<LeadSearchModel> PrepareLeadSearchModelAsync(LeadSearchModel searchModel)
        {

            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.AvailableStatus.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            searchModel.AvailableIsSyncReplyOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = "All", Value = "", Selected = true },
                new SelectListItem { Text = "Synced", Value = "true" },
                new SelectListItem { Text = "Not Synced", Value = "false" }
            };
            var leadStatusName = "";
            var leadStatus = await _leadStatusService.GetAllLeadStatusByNameAsync(leadStatusName);
            foreach (var p in leadStatus)
            {
                searchModel.AvailableStatus.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
            foreach (var value in Enum.GetValues(typeof(EmailValidationStatus)).Cast<EmailValidationStatus>())
            {
                var displayName = value.GetType()
                    .GetField(value.ToString())
                    ?.GetCustomAttribute<DisplayAttribute>()
                    ?.Name ?? value.ToString();

                searchModel.AvailableEmailStatus.Add(new SelectListItem
                {
                    Text = displayName,
                    Value = ((int)value).ToString()
                });
            }
            await PrepareTitleListAsync(searchModel);
            await PrepareTagsListAsync(searchModel);
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<LeadListModel> PrepareLeadListModelAsync(LeadSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get lead
            var leads = await _leadService.GetAllLeadAsync(showHidden: true,
                name: searchModel.SearchName, companyName: searchModel.SearchCompanytName,
                selectedtagsid: searchModel.SelectedTags,
                email: searchModel.SearchEmail,
                website: searchModel.SearchWebsiteUrl,
                nofoEmployee: searchModel.SearchNoofEmployee,
                leadStatusId: searchModel.SearchLeadStatusId,
                titleid: searchModel.SearchTitlesId,
                emailStatusId: searchModel.SearchEmailStatusId,
                isSyncedToReply:searchModel.IsSyncedToReply,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            var allTags = await _tagsService.GetAllTagsAsync("");
            //prepare grid model
            var model = await new LeadListModel().PrepareToGridAsync(searchModel, leads, () =>
            {
                //fill in model values from the entity
                return leads.SelectAwait(async leads =>
                {
                    var leadModel = new LeadModel();
                    var selectedAvailableOption = leads.EmailStatusId;
                    leadModel.Id = leads.Id;
                    leadModel.FirstName = leads.FirstName;
                    leadModel.LastName = leads.LastName;
                    leadModel.CompanyName = leads.CompanyName;
                    leadModel.Phone = leads.Phone;
                    leadModel.Email = leads.Email;
                    leadModel.AnnualRevenue = leads.AnnualRevenue;
                    leadModel.WebsiteUrl = leads.WebsiteUrl;
                    leadModel.NoofEmployee = leads.NoofEmployee;
                    leadModel.SkypeId = leads.SkypeId;
                    leadModel.EmailOptOut = leads.EmailOptOut;
                    leadModel.Twitter = leads.Twitter;
                    leadModel.LinkedinUrl = leads.LinkedinUrl;
                    leadModel.Facebookurl = leads.Facebookurl;
                    leadModel.SecondaryEmail = leads.SecondaryEmail;
                    leadModel.Description = leads.Description;
                    leadModel.TitleId = leads.TitleId;
                    leadModel.IndustryId = leads.IndustryId;
                    leadModel.CategoryId = leads.CategoryId;
                    leadModel.LeadSourceId = leads.LeadSourceId;
                    leadModel.IsSyncedToReply = leads.IsSyncedToReply;
                    leadModel.LeadStatusId = leads.LeadStatusId;
                    leadModel.LeadStatusName = await _leadStatusService.GetLeadStatusNameByIdAsync(leads.LeadStatusId);
                    leadModel.Name = leads.FirstName + " " + leads.LastName;
                    leadModel.AddressId = leads.AddressId;
                    leadModel.CustomerId = leads.CustomerId;
                    var leadTags = await _leadService.GetLeadTagByLeadIdAsync(leads.Id);
                    leadModel.SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();
                    var tagNames = new List<string>();
                    foreach (var tag in leadTags)
                    {
                        var tagEntity = await _tagsService.GetTagsByIdAsync(tag.TagsId);
                        if (tagEntity != null)
                            tagNames.Add(tagEntity.Name);
                    }
                    leadModel.TagsName = string.Join(", ", tagNames);
                    leadModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(leads.CreatedOnUtc, DateTimeKind.Utc);
                    leadModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(leads.UpdatedOnUtc, DateTimeKind.Utc);
                    var title = await _titleService.GetTitleByIdAsync(leads.TitleId);
                    leadModel.TitleName = title?.Name ?? " ";
                    leadModel.EmailStatusId = leads.EmailStatusId;
                    leadModel.EmailStatusName = ((EmailValidationStatus)leads.EmailStatusId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) leadModel.EmailStatusId
                        = (int)((EmailValidationStatus)selectedAvailableOption);

                    return leadModel;
                });
            });
            return model;
        }

        public virtual async Task<LeadModel> PrepareLeadModelAsync(LeadModel model, Lead lead, bool excludeProperties = false)
        {
            var status = await EmailValidationStatus.None.ToSelectListAsync();
            if (lead != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new LeadModel();
                    model.Id = lead.Id;
                    model.FirstName = lead.FirstName;
                    model.LastName = lead.LastName;
                    model.CompanyName = lead.CompanyName;
                    model.Phone = lead.Phone;
                    model.Email = lead.Email;
                    model.AnnualRevenue = lead.AnnualRevenue;
                    model.WebsiteUrl = lead.WebsiteUrl;
                    model.NoofEmployee = lead.NoofEmployee;
                    model.SkypeId = lead.SkypeId;
                    model.EmailOptOut = lead.EmailOptOut;
                    model.SecondaryEmail = lead.SecondaryEmail;
                    model.Twitter = lead.Twitter;
                    model.LinkedinUrl = lead.LinkedinUrl;
                    model.Facebookurl = lead.Facebookurl;
                    model.Description = lead.Description;
                    model.TitleId = lead.TitleId;
                    model.IndustryId = lead.IndustryId;
                    model.LeadStatusId = lead.LeadStatusId;
                    model.LeadSourceId = lead.LeadSourceId;
                    model.CategoryId = lead.CategoryId;
                    model.Name = lead.FirstName + " " + lead.LastName;
                    model.CustomerId = lead.CustomerId;
                    model.EmailStatusId = lead.EmailStatusId;
                    model.IsSyncedToReply = lead.IsSyncedToReply;
                    model.CreatedOnUtc = lead.CreatedOnUtc;
                    model.UpdatedOnUtc = lead.UpdatedOnUtc;
                    if (lead.AddressId > 0)
                    {
                        var address = await _addressService.GetAddressByIdAsync(lead.AddressId);
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

                    var leadTags = await _leadService.GetLeadTagByLeadIdAsync(lead.Id);
                    model.SelectedTagIds = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();

                }
            }
            model.EmailStatus = status.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.EmailStatusId.ToString() == store.Value
            }).ToList();
            model.DealsModels.StageId = (int)StageEnum.Select;
            model.DealsModels.Stages = Enum.GetValues(typeof(StageEnum))
        .Cast<StageEnum>()
        .Select(stage => new SelectListItem
        {
            Value = ((int)stage).ToString(),
            Text = stage.ToString()
        })
        .ToList();
            await PrepareTitleListAsync(model);
            await PrepareLeadSourcesListAsync(model);
            await PrepareIndustryListAsync(model);
            await PrepareLeadStatusListAsync(model);
            await PrepareCategorysListAsync(model);
            await PrepareTagsListAsync(model, model.Id);
            await PrepareCountriesListAsync(model);
            await PrepareCustomersListAsync(model);
            await PrepareStatesListAsync(model, Convert.ToInt32(model.CountryId));
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();
            // Show success notification if set via TempData (from popup)
            if (TempData.ContainsKey("SuccessNotification"))
                _notificationService.SuccessNotification(TempData["SuccessNotification"].ToString());
            //prepare model
            var model = await PrepareLeadSearchModelAsync(new LeadSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(LeadSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareLeadListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLeadModelAsync(new LeadModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LeadModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var address = new Address
                {
                    Address1 = model.Address1,
                    Address2 = model.Address2,
                    City = model.City,
                    ZipPostalCode = model.ZipCode,
                    CountryId = model.CountryId,
                    StateProvinceId = model.StateId,
                };
                await _addressService.InsertAddressAsync(address);
                var addressid = address.Id;
                var lead = new Lead();
                lead.Id = model.Id;
                lead.FirstName = model.FirstName;
                lead.LastName = model.LastName;
                lead.CompanyName = model.CompanyName;
                lead.Phone = model.Phone;
                lead.Email = model.Email;
                lead.AnnualRevenue = model.AnnualRevenue;
                lead.WebsiteUrl = model.WebsiteUrl;
                lead.NoofEmployee = model.NoofEmployee;
                lead.SkypeId = model.SkypeId;
                lead.SecondaryEmail = model.SecondaryEmail;
                lead.EmailOptOut = model.EmailOptOut;
                lead.Twitter = model.Twitter;
                lead.LinkedinUrl = model.LinkedinUrl;
                lead.Facebookurl = model.Facebookurl;
                lead.Description = model.Description;
                lead.TitleId = model.TitleId;
                lead.CategoryId = model.CategoryId;
                lead.LeadSourceId = model.LeadSourceId;
                lead.LeadStatusId = model.LeadStatusId;
                lead.IndustryId = model.IndustryId;
                lead.EmailStatusId = model.EmailStatusId;
                lead.IsSyncedToReply = model.IsSyncedToReply;
                lead.CreatedOnUtc = DateTime.UtcNow;
                lead.UpdatedOnUtc = DateTime.UtcNow;
                lead.AddressId = addressid;
                lead.CustomerId = model.CustomerId;
                lead.EmailStatusId = (int)await GetEmailValidationStatusAsync(lead.Email);
                await _leadService.InsertLeadAsync(lead);
                if (model.SelectedTagIds != null && model.SelectedTagIds.Any())
                {
                    foreach (var tagId in model.SelectedTagIds)
                    {
                        var leadTag = new LeadTags
                        {
                            LeadId = lead.Id,
                            TagsId = tagId
                        };
                        await _leadService.InsertLeadTagsAsync(leadTag);
                    }
                }
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Leads.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = lead.Id });
            }

            //prepare model
            model = await PrepareLeadModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a leads with the specified id
            var leads = await _leadService.GetLeadByIdAsync(id);
            if (leads == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareLeadModelAsync(null, leads);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(LeadModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();
            ModelState.Remove("DealsModels");
            //ModelState.Remove("DealsModels.DealName");
            //ModelState.Remove("DealsModels.Amount");
            //ModelState.Remove("DealsModels.ClosingDate");
            //ModelState.Remove("DealsModels.StageId");
            var leads = await _leadService.GetLeadByIdAsync(model.Id);
            if (leads == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                if (leads.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(leads.AddressId);
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

                leads.FirstName = model.FirstName;
                leads.LastName = model.LastName;
                leads.CompanyName = model.CompanyName;
                leads.Phone = model.Phone;
                leads.AnnualRevenue = model.AnnualRevenue;
                leads.WebsiteUrl = model.WebsiteUrl;
                leads.NoofEmployee = model.NoofEmployee;
                leads.SkypeId = model.SkypeId;
                leads.SecondaryEmail = model.SecondaryEmail;
                leads.EmailOptOut = model.EmailOptOut;
                leads.Twitter = model.Twitter;
                leads.LinkedinUrl = model.LinkedinUrl;
                leads.Facebookurl = model.Facebookurl;
                leads.Description = model.Description;
                leads.TitleId = model.TitleId;
                leads.IndustryId = model.IndustryId;
                leads.LeadSourceId = model.LeadSourceId;
                leads.LeadStatusId = model.LeadStatusId;
                leads.CategoryId = model.CategoryId;
                leads.CustomerId = model.CustomerId;
                leads.CreatedOnUtc = model.CreatedOnUtc;
                leads.UpdatedOnUtc = DateTime.UtcNow;
                leads.IsSyncedToReply = model.IsSyncedToReply;
                leads.AddressId = model.AddressId;
                leads.EmailStatusId = model.EmailStatusId;
                // Only re-validate if email was changed
                if (!string.Equals(leads.Email, model.Email, StringComparison.InvariantCultureIgnoreCase))
                {
                    leads.EmailStatusId = (int)await GetEmailValidationStatusAsync(model.Email);
                }

                leads.Email = model.Email;
                model.Name = leads.FirstName + " " + leads.LastName;

                await _leadService.UpdateLeadAsync(leads);

                var existingTags = await _leadService.GetLeadTagByLeadIdAsync(model.Id);
                var existingTagIds = existingTags?.Select(t => t.TagsId).ToList() ?? new List<int>();

                foreach (var tag in existingTags)
                {
                    if (!model.SelectedTagIds.Contains(tag.TagsId))
                        await _leadService.DeleteLeadTagsByLeadIdAsync(leads.Id, tag.TagsId);
                }

                foreach (var tagId in model.SelectedTagIds)
                {
                    if (!existingTagIds.Contains(tagId))
                    {
                        var newLeadTag = new LeadTags
                        {
                            LeadId = leads.Id,
                            TagsId = tagId
                        };
                        await _leadService.InsertLeadTagsAsync(newLeadTag);
                    }
                }

                model.SelectedTagIds = (await _leadService.GetLeadTagByLeadIdAsync(leads.Id))
                    ?.Select(t => t.TagsId)
                    .ToList() ?? new List<int>();

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Leads.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = leads.Id });
            }

            model = await PrepareLeadModelAsync(model, leads, true);
            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/Edit.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var leads = await _leadService.GetLeadByIdAsync(id);
            var address = await _addressService.GetAddressByIdAsync(leads.AddressId);
            if (address != null)
            {
                await _addressService.DeleteAddressAsync(address); // Delete the address
            }
            if (leads == null)
                return RedirectToAction("List");
            await _leadService.DeleteTagsByLeadIdAsync(leads.Id);
            await _leadService.DeleteLeadAsync(leads);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Lead.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _leadService.GetLeadByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _leadService.DeleteTagsByLeadIdAsync(item.Id);
                if (item.AddressId != null && item.AddressId > 0)
                {
                    var address = await _addressService.GetAddressByIdAsync(item.AddressId);
                    if (address != null)
                    {
                        await _addressService.DeleteAddressAsync(address); // Delete the address
                    }
                }
                await _leadService.DeleteLeadAsync(item);
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Export/Import

        [HttpPost, ActionName("ExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportToExcel(LeadSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            var leads = await _leadService.GetAllLeadAsync(showHidden: true,
                name: searchModel.SearchName, companyName: searchModel.SearchCompanytName, selectedtagsid: searchModel.SelectedTags,
                email: searchModel.SearchEmail,
                website: searchModel.SearchWebsiteUrl,
                nofoEmployee: searchModel.SearchNoofEmployee,
                leadStatusId: searchModel.SearchLeadStatusId,
                titleid: searchModel.SearchTitlesId,
                emailStatusId: searchModel.SearchEmailStatusId,
                isSyncedToReply:searchModel.IsSyncedToReply,
                pageIndex: 0, pageSize: int.MaxValue);

            var leadModelDto = new List<LeadDto>();
            try
            {
                foreach (var item in leads)
                {
                    var lead = await _leadService.GetLeadByIdAsync(item.Id)
                        ?? throw new ArgumentException("No lead found with the specified id");
                    var models = await PrepareLeadModelAsync(new LeadModel(), lead);
                    var customer = await _customerService.GetCustomerByIdAsync(models.CustomerId);
                    var customerName = customer != null ? customer.FirstName + " " + customer.LastName : "N/A";
                    var title = (await _titleService.GetTitleByIdAsync(lead.TitleId))?.Name;
                    var leadSources = (await _leadSourceService.GetLeadSourceByIdAsync(lead.LeadSourceId))?.Name;
                    var industry = (await _industryService.GetIndustryByIdAsync(lead.IndustryId))?.Name;
                    var leadStatus = (await _leadStatusService.GetLeadStatusByIdAsync(lead.LeadStatusId))?.Name;
                    var category = (await _categorysService.GetCategorysByIdAsync(lead.CategoryId))?.Name;
                    var emailStatus = Enum.GetName(typeof(EmailValidationStatus), lead.EmailStatusId) ?? " ";
                    //      var address = await _addressService.GetAddressByIdAsync(lead.AddressId);
                    //      var country = (address?.CountryId ?? 0) > 0
                    //? await _countryService.GetCountryByIdAsync(address.CountryId.Value)
                    //: null;

                    //      var state = (address?.StateProvinceId ?? 0) > 0
                    //                  ? await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value)
                    //                  : null;
                    Address address = null;
                    Country country = null;
                    StateProvince state = null;
                    string city = " ";
                    string zipCode = " ";

                    if (lead.AddressId > 0)
                    {
                        address = await _addressService.GetAddressByIdAsync(lead.AddressId);

                        if (address != null)
                        {
                            city = address.City ?? " ";
                            zipCode = address.ZipPostalCode ?? " ";

                            if (address.CountryId > 0)
                                country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);

                            if (address.StateProvinceId > 0)
                                state = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value);
                        }
                    }
                    var leadDTO = new LeadDto
                    {
                        Name = lead.FirstName + " " + lead.LastName ?? " ",
                        CompanyName = lead.CompanyName ?? " ",
                        Phone = lead.Phone ?? " ",
                        Email = lead.Email ?? " ",
                        EmailStatusId = emailStatus,
                        AnnualRevenue = lead.AnnualRevenue,
                        WebsiteUrl = lead.WebsiteUrl ?? " ",
                        NoofEmployee = lead.NoofEmployee,
                        SkypeId = lead.SkypeId ?? " ",
                        SecondaryEmail = lead.SecondaryEmail ?? " ",
                        TitleId = title ?? " ",
                        CustomerId = customerName ?? " ",
                        LeadSourceId = leadSources ?? " ",
                        IndustryId = industry ?? " ",
                        LeadStatusId = leadStatus ?? " ",
                        CategoryId = category ?? " ",
                        Twitter = lead.Twitter ?? " ",
                        LinkedinUrl = lead.LinkedinUrl ?? " ",
                        Facebookurl = lead.Facebookurl ?? " ",
                        Description = lead.Description ?? " ",
                        Country = country?.Name ?? " ",
                        State = state?.Name ?? " ",
                        City =city,
                        ZipCode = zipCode,
                    };
                    leadModelDto.Add(leadDTO);
                }

                var bytes =await _leadExportService.ExportLeadsToExcelAsync(leadModelDto);
                return File(bytes, MimeTypes.TextXlsx, "Lead.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost, ActionName("ExportToExcelReply")]
        [FormValueRequired("exportexcelReply-all")]
        public virtual async Task<IActionResult> ExportToExcelReply(LeadSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            var leads = await _leadService.GetAllLeadAsync(showHidden: true,
                name: searchModel.SearchName, companyName: searchModel.SearchCompanytName, selectedtagsid: searchModel.SelectedTags,
                email: searchModel.SearchEmail,
                website: searchModel.SearchWebsiteUrl,
                nofoEmployee: searchModel.SearchNoofEmployee,
                leadStatusId: searchModel.SearchLeadStatusId,
                titleid: searchModel.SearchTitlesId,
                emailStatusId: searchModel.SearchEmailStatusId,
                isSyncedToReply:searchModel.IsSyncedToReply,
                pageIndex: 0, pageSize: int.MaxValue);

            var leadModelDto = new List<LeadDto>();
            try
            {
                foreach (var item in leads)
                {
                    var lead = await _leadService.GetLeadByIdAsync(item.Id)
                        ?? throw new ArgumentException("No lead found with the specified id");
                    var models = await PrepareLeadModelAsync(new LeadModel(), lead);
                    var customer = await _customerService.GetCustomerByIdAsync(models.CustomerId);
                    var customerName = customer != null ? customer.FirstName + " " + customer.LastName : "N/A";
                    var title = (await _titleService.GetTitleByIdAsync(lead.TitleId))?.Name;
                    var leadSources = (await _leadSourceService.GetLeadSourceByIdAsync(lead.LeadSourceId))?.Name;
                    var industry = (await _industryService.GetIndustryByIdAsync(lead.IndustryId))?.Name;
                    var leadStatus = (await _leadStatusService.GetLeadStatusByIdAsync(lead.LeadStatusId))?.Name;
                    var category = (await _categorysService.GetCategorysByIdAsync(lead.CategoryId))?.Name;
                    var emailStatus = Enum.GetName(typeof(EmailValidationStatus), lead.EmailStatusId) ?? " ";
                    Address address = null;
                    Country country = null;
                    StateProvince state = null;
                    string city = " ";
                    string zipCode = " ";

                    if (lead.AddressId > 0)
                    {
                        address = await _addressService.GetAddressByIdAsync(lead.AddressId);

                        if (address != null)
                        {
                            city = address.City ?? " ";
                            zipCode = address.ZipPostalCode ?? " ";

                            if (address.CountryId > 0)
                                country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);

                            if (address.StateProvinceId > 0)
                                state = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value);
                        }
                    }
                    var leadDTO = new LeadDto
                    {
                        FirstName = lead.FirstName,
                        LastName = lead.LastName,
                        Email = lead.Email ?? " ",
                        TitleId = title ?? " ",
                        CompanyName = lead.CompanyName ?? " ",
                        City = city,
                        State = state?.Name ?? " ",
                        Country = country?.Name ?? " ",
                        LinkedinUrl = lead.LinkedinUrl ?? " ",
                        LinkedInRecruiter =" ",
                        Phone = lead.Phone ?? " ",

                    };
                    leadModelDto.Add(leadDTO);
                }

                var bytes = await _leadExportService.ExportLeadsToExcelReplyAsync(leadModelDto);
                return File(bytes, MimeTypes.TextXlsx, "Lead.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> SelectedExportToExcel(List<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            if (selectedIds == null || !selectedIds.Any())
            {
                _notificationService.WarningNotification("No products selected for export.");
                return RedirectToAction("List");
            }

            var leadModelDto = new List<LeadDto>();
            try
            {
                foreach (var item in selectedIds)
                {
                    var lead = await _leadService.GetLeadByIdAsync(item)
                        ?? throw new ArgumentException("No lead found with the specified id");
                    var models = await PrepareLeadModelAsync(new LeadModel(), lead);
                    var customer = await _customerService.GetCustomerByIdAsync(models.CustomerId);
                    var customerName = customer != null ? customer.FirstName + " " + customer.LastName : "N/A";
                    var title = (await _titleService.GetTitleByIdAsync(lead.TitleId))?.Name;
                    var leadSources = (await _leadSourceService.GetLeadSourceByIdAsync(lead.LeadSourceId))?.Name;
                    var industry = (await _industryService.GetIndustryByIdAsync(lead.IndustryId))?.Name;
                    var leadStatus = (await _leadStatusService.GetLeadStatusByIdAsync(lead.LeadStatusId))?.Name;
                    var category = (await _categorysService.GetCategorysByIdAsync(lead.CategoryId))?.Name;
                    var emailStatus = Enum.GetName(typeof(EmailValidationStatus), lead.EmailStatusId) ?? " ";
                    Address address = null;
                    Country country = null;
                    StateProvince state = null;
                    string city = " ";
                    string zipCode = " ";

                    if (lead.AddressId > 0)
                    {
                        address = await _addressService.GetAddressByIdAsync(lead.AddressId);

                        if (address != null)
                        {
                            city = address.City ?? " ";
                            zipCode = address.ZipPostalCode ?? " ";

                            if (address.CountryId > 0)
                                country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);

                            if (address.StateProvinceId > 0)
                                state = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value);
                        }
                    }
                    var leadDTO = new LeadDto
                    {
                        Name = lead.FirstName + " " + lead.LastName ?? " ",
                        CompanyName = lead.CompanyName ?? " ",
                        Phone = lead.Phone ?? " ",
                        Email = lead.Email ?? " ",
                        EmailStatusId = emailStatus,
                        AnnualRevenue = lead.AnnualRevenue,
                        WebsiteUrl = lead.WebsiteUrl ?? " ",
                        NoofEmployee = lead.NoofEmployee,
                        SkypeId = lead.SkypeId ?? " ",
                        SecondaryEmail = lead.SecondaryEmail ?? " ",
                        TitleId = title ?? " ",
                        CustomerId = customerName ?? " ",
                        LeadSourceId = leadSources ?? " ",
                        IndustryId = industry ?? " ",
                        LeadStatusId = leadStatus ?? " ",
                        CategoryId = category ?? " ",
                        Twitter = lead.Twitter ?? " ",
                        LinkedinUrl = lead.LinkedinUrl ?? " ",
                        Facebookurl = lead.Facebookurl ?? " ",
                        Description = lead.Description ?? " ",
                        Country = country?.Name ?? " ",
                        State = state?.Name ?? " ",
                        City = city,
                        ZipCode = zipCode
                    };
                    leadModelDto.Add(leadDTO);
                }

                var bytes = await _leadExportService.ExportLeadsToExcelAsync(leadModelDto);
                return File(bytes, MimeTypes.TextXlsx, "Lead.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> SelectedExportToExcelReply(List<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            if (selectedIds == null || !selectedIds.Any())
            {
                _notificationService.WarningNotification("No products selected for export.");
                return RedirectToAction("List");
            }

            var leadModelDto = new List<LeadDto>();
            try
            {
                foreach (var item in selectedIds)
                {
                    var lead = await _leadService.GetLeadByIdAsync(item)
                        ?? throw new ArgumentException("No lead found with the specified id");
                    var models = await PrepareLeadModelAsync(new LeadModel(), lead);
                    var customer = await _customerService.GetCustomerByIdAsync(models.CustomerId);
                    var customerName = customer != null ? customer.FirstName + " " + customer.LastName : "N/A";
                    var title = (await _titleService.GetTitleByIdAsync(lead.TitleId))?.Name;
                    var leadSources = (await _leadSourceService.GetLeadSourceByIdAsync(lead.LeadSourceId))?.Name;
                    var industry = (await _industryService.GetIndustryByIdAsync(lead.IndustryId))?.Name;
                    var leadStatus = (await _leadStatusService.GetLeadStatusByIdAsync(lead.LeadStatusId))?.Name;
                    var category = (await _categorysService.GetCategorysByIdAsync(lead.CategoryId))?.Name;
                    var emailStatus = Enum.GetName(typeof(EmailValidationStatus), lead.EmailStatusId) ?? " ";
                    Address address = null;
                    Country country = null;
                    StateProvince state = null;
                    string city = " ";
                    string zipCode = " ";

                    if (lead.AddressId > 0)
                    {
                        address = await _addressService.GetAddressByIdAsync(lead.AddressId);

                        if (address != null)
                        {
                            city = address.City ?? " ";
                            zipCode = address.ZipPostalCode ?? " ";

                            if (address.CountryId > 0)
                                country = await _countryService.GetCountryByIdAsync(address.CountryId.Value);

                            if (address.StateProvinceId > 0)
                                state = await _stateProvinceService.GetStateProvinceByIdAsync(address.StateProvinceId.Value);
                        }
                    }
                    var leadDTO = new LeadDto
                    {
                        FirstName = lead.FirstName,
                        LastName = lead.LastName,
                        Email = lead.Email ?? " ",
                        TitleId = title ?? " ",
                        CompanyName = lead.CompanyName ?? " ",
                        City = city,
                        State = state?.Name ?? " ",
                        Country = country?.Name ?? " ",
                        LinkedinUrl = lead.LinkedinUrl ?? " ",
                        LinkedInRecruiter = " ",
                        Phone = lead.Phone ?? " "
                    };
                    leadModelDto.Add(leadDTO);
                }

                var bytes = await _leadExportService.ExportLeadsToExcelReplyAsync(leadModelDto);
                return File(bytes, MimeTypes.TextXlsx, "Lead.xlsx");
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportFromExcel(IFormFile importFile)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            try
            {
                if (importFile != null && importFile.Length > 0)
                {
                    await _leadImportService.ImportLeadsFromExcelAsync(importFile);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                //_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Lead.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception)
            {

                throw;
            }

        }
        [HttpPost]
        public async Task<IActionResult> ImportFromExcelReply(IFormFile importFile)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            try
            {
                if (importFile != null && importFile.Length > 0)
                {
                    await _leadImportService.ImportLeadsFromExcelReplyAsync(importFile);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                //_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Lead.Imported"));
                return RedirectToAction("List");
            }
            catch (Exception)
            {

                throw;
            }

        }
        public async Task<IActionResult> ConvertLead(int leadId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            var lead = await _leadService.GetLeadByIdAsync(leadId);
            if (lead == null)
            {
                ViewBag.RefreshPage = false;
                return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/ConverLead.cshtml");
            }
            ViewBag.RefreshPage = false;
            var model = await PrepareLeadModelAsync(null, lead, true);
            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/ConverLead.cshtml", model);
        }
        [HttpPost]
        public async Task<IActionResult> ConvertLead(LeadModel model, bool createDeal)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            var lead = await _leadService.GetLeadByIdAsync(model.Id);
            if (lead == null)
            {
                return Json(new { success = false, message = "Lead not found" });
            }
            var address = await _addressService.GetAddressByIdAsync(lead.AddressId);
            // Convert Lead to Company
            var existingCompany = await _companyService.GetCompanyByNameAsync(lead.CompanyName);
            Company company;

            if (existingCompany != null)
            {
                company = existingCompany;
            }
            else
            {
                company = new Company
                {
                    CompanyName = lead.CompanyName,
                    WebsiteUrl = lead.WebsiteUrl,
                    Phone = lead.Phone,
                    IndustryId = lead.IndustryId,
                    NoofEmployee = lead.NoofEmployee,
                    AnnualRevenue = lead.AnnualRevenue,
                    BillingAddressId = lead.AddressId,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                await _companyService.InsertCompanyAsync(company);
            }
            // Convert Lead to Contact
            var newContact = new Contacts
            {
                CustomerId = lead.CustomerId,
                FirstName = lead.FirstName,
                LastName = lead.LastName,
                CompanyName = lead.CompanyName,
                TitleId = lead.TitleId,
                Phone = lead.Phone,
                Email = lead.Email,
                EmailStatusId = lead.EmailStatusId,
                WebsiteUrl = lead.WebsiteUrl,
                IndustryId = lead.IndustryId,
                ContactsSourceId = lead.LeadSourceId,
                NoofEmployee = lead.NoofEmployee,
                AnnualRevenue = lead.AnnualRevenue,
                SecondaryEmail = lead.SecondaryEmail,
                SkypeId = lead.SkypeId,
                Twitter = lead.Twitter,
                LinkedinUrl = lead.LinkedinUrl,
                Facebookurl = lead.Facebookurl,
                AddressId = lead.AddressId,
                Description = lead.Description,
                EmailOptOut = lead.EmailOptOut,
                CreatedOnUtc = DateTime.UtcNow,
                CompanyId = company.Id // Link contact to the new company
            };
            await _contactsService.InsertContactsAsync(newContact);

            var leadTag = await _leadService.GetLeadTagsByLeadIdAsync(model.Id);
            if (leadTag != null)  // Check if leadTag is NOT NULL
            {
                var contactsTags = new ContactsTags
                {
                    ContactsId = newContact.Id,
                    TagsId = leadTag.TagsId
                };
                await _contactsService.InsertContactsTagsAsync(contactsTags);
            }
            model.DealsModels.Contacts = new List<SelectListItem>
    {
        new SelectListItem
        {
            Text = $"{newContact.FirstName} {newContact.LastName}",
            Value = newContact.Id.ToString(),
            Selected = true
        }
    };
            model.DealsModels.ContactId = newContact.Id;
            var dealModel = model.DealsModels;
            // If user selected to create a new deal, create it
            if (createDeal)
            {
                var newDeal = new Deals
                {
                    Amount = dealModel.Amount,
                    DealName = dealModel.DealName,
                    ClosingDate = dealModel.ClosingDate,
                    StageId = dealModel.StageId ?? 0,
                    CompanyId = company.Id,
                    ContactId = newContact.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                await _dealsService.InsertDealsAsync(newDeal);
            }
            await _leadService.DeleteLeadAsync(lead);

            TempData["SuccessNotification"] = await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Leads.Converted");


            ViewBag.RefreshPage = true;
            // Return success response
            return View("~/Plugins/Misc.SatyanamCRM/Views/Leads/ConverLead.cshtml", model);
        }


        #endregion

        [HttpPost]
        public virtual async Task<IActionResult> EmailSelectedCheck(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(selectedIds))
                return NoContent();

            var idList = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.Parse(id.Trim()))
                .ToList();

            var data = await _leadService.GetLeadByIdsAsync(idList.ToArray());

            foreach (var item in data)
            {
                try
                {
                    string verificationResult = await _emailverificationService.VerifyEmailApi(item.Email);

                    if (verificationResult == "__SESSION_EXPIRED__")
                    {
                        _notificationService.WarningNotification("Email verification limit reached or session expired. Skipping validation.");
                        item.EmailStatusId = (int)EmailValidationStatus.None;
                    }
                    else if (!string.IsNullOrWhiteSpace(verificationResult))
                    {
                        dynamic verificationResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(verificationResult);

                        var result = ((string)verificationResponse?.result)?.ToLowerInvariant();
                        var safeToSend = ((string)verificationResponse?.safe_to_send)?.ToLowerInvariant();

                        if (result == "valid" && safeToSend == "true")
                        {
                            item.EmailStatusId = (int)EmailValidationStatus.Valid;
                        }
                        else if (result == "invalid" || safeToSend == "false")
                        {
                            item.EmailStatusId = (int)EmailValidationStatus.Invalid;
                        }
                        else
                        {
                            item.EmailStatusId = (int)EmailValidationStatus.None;
                        }
                    }
                    await _leadService.UpdateLeadAsync(item);
                }
                catch (Exception ex)
                {
                    _notificationService.WarningNotification("Email verification failed. Please try again later.");
                }
            }
            return RedirectToAction("List");
        }

    }
}
