using App.Core;
using App.Core.Domain;
using App.Core.Domain.Media;
using App.Core.Domain.Messages;
using App.Core.Domain.Security;
using App.Data;
using App.Data.Extensions;
using App.Services;
using App.Services.Configuration;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using App.Web.Framework.Themes;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Core.Settings;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Campaings;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.Leads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    //[AutoValidateAntiforgeryToken]
    //[AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CampaingsController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ITitleService _titleService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICampaingsService _campaingsService;
        private readonly ITagsService _tagsService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILeadService _leadService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ISettingService _settingService;
        private readonly IPictureService _pictureService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHelper _webHelper;
        private readonly MediaSettings _mediaSettings;
        private readonly IThemeContext _themeContext;
        private readonly IStoreContext _storeContext;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<QueuedEmail> _queuedEmailRepository;
        private readonly IContactsService _contactsService;
        private readonly ICampaingsEmailLogsService _campaingsEmailLogsService;
        private readonly IRepository<CampaingsEmailLogs> _campaingsEmailLogsRepository;
        #endregion

        #region Ctor 

        public CampaingsController(IPermissionService permissionService,
                               ITitleService titleService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               ICampaingsService campaingsService,
                               ITagsService tagsService,
                               IEmailAccountService emailAccountService,
                               ILeadService leadService,
                               IDateTimeHelper dateTimeHelper,
                               IQueuedEmailService queuedEmailService,
                               ISettingService settingService,
                               IPictureService pictureService,
                               IHttpContextAccessor httpContextAccessor,
                               IWebHelper webHelper,
                               MediaSettings mediaSettings,
                               IThemeContext themeContext,
                               IStoreContext storeContext,
                               IEmailSender emailSender,
                               IRepository<QueuedEmail> queuedEmailRepository,
                               IContactsService contactsService,
                               ICampaingsEmailLogsService campaingsEmailLogsService,
                               IRepository<CampaingsEmailLogs> campaingsEmailLogsRepository)
        {
            _permissionService = permissionService;
            _titleService = titleService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _campaingsService = campaingsService;
            _tagsService = tagsService;
            _emailAccountService = emailAccountService;
            _leadService = leadService;
            _dateTimeHelper = dateTimeHelper;
            _queuedEmailService = queuedEmailService;
            _settingService = settingService;
            _pictureService = pictureService;
            _httpContextAccessor = httpContextAccessor;
            _webHelper = webHelper;
            _mediaSettings = mediaSettings;
            _themeContext = themeContext;
            _storeContext = storeContext;
            _emailSender = emailSender;
            _queuedEmailRepository = queuedEmailRepository;
            _contactsService = contactsService;
            _campaingsEmailLogsService = campaingsEmailLogsService;
            _campaingsEmailLogsRepository = campaingsEmailLogsRepository;
        }

        #endregion

        #region Utilities
        private string ReplaceTokens(string template, IList<Token> tokens)
        {
            if (string.IsNullOrEmpty(template) || tokens == null || !tokens.Any())
                return template;

            foreach (var token in tokens)
            {
                template = template.Replace($"%{token.Key}%", token.Value?.ToString() ?? "", StringComparison.OrdinalIgnoreCase);
            }

            return template;
        }

        public virtual async Task PrepareEmailAccountListAsync(CampaingsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Emails.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            var emails = await _emailAccountService.GetAllEmailAccountsAsync();
            foreach (var p in emails)
            {
                model.Emails.Add(new SelectListItem
                {
                    Text = p.Email,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareTagsListAsync(CampaingsModel model, int leadId)
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
                    Selected = model.TagsId.Contains(p.Id)
                });
            }
            var leadTags = await _leadService.GetLeadTagByLeadIdAsync(leadId);
            model.TagsId = leadTags?.Select(x => x.TagsId).ToList() ?? new List<int>();

        }
        public virtual async Task<CampaingsSearchModel> PrepareCampaingsSearchModelAsync(CampaingsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();
            searchModel.Status = (await StatusEnum.Select.ToSelectListAsync()).ToList();
            return searchModel;
        }
        public virtual async Task<CampaingsEmailLogListModel> PrepareCampaingsEmailLogListModelAsync(CampaingsEmailLogSearchModel searchModel)
        {
            var logs = (await _campaingsEmailLogsService.GetByCampaignIdAsync(searchModel.CampaignId)).AsQueryable();

            if (!string.IsNullOrEmpty(searchModel.Email))
                logs = logs.Where(l => l.Email.Contains(searchModel.Email, StringComparison.OrdinalIgnoreCase));

            if (searchModel.IsOpened.HasValue)
                logs = logs.Where(l => l.IsOpened == searchModel.IsOpened.Value);

            if (searchModel.IsClicked.HasValue)
                logs = logs.Where(l => l.IsClicked == searchModel.IsClicked.Value);

            if (searchModel.OpenCountMin.HasValue && searchModel.OpenCountMin.Value > 0)
                logs = logs.Where(l => l.OpenCount >= searchModel.OpenCountMin.Value);

            // Only apply OpenCountMax if it's greater than 0 (prevent filtering out actual opens)
            if (searchModel.OpenCountMax.HasValue && searchModel.OpenCountMax.Value > 0)
                logs = logs.Where(l => l.OpenCount <= searchModel.OpenCountMax.Value);

            // Same logic for ClickCount
            if (searchModel.ClickCountMin.HasValue && searchModel.ClickCountMin.Value > 0)
                logs = logs.Where(l => l.ClickCount >= searchModel.ClickCountMin.Value);

            if (searchModel.ClickCountMax.HasValue && searchModel.ClickCountMax.Value > 0)
                logs = logs.Where(l => l.ClickCount <= searchModel.ClickCountMax.Value);

            if (searchModel.IsUnsubscribed.HasValue)
                logs = logs.Where(l => l.IsUnsubscribed == searchModel.IsUnsubscribed.Value);

            var pagedList = logs.ToList().ToPagedList(searchModel);


            var model = new CampaingsEmailLogListModel().PrepareToGrid(searchModel, pagedList, () =>
            {
                return pagedList.Select(l => new CampaingsEmailLogModel
                {
                    Email = l.Email,
                    IsOpened = l.IsOpened,
                    OpenCount = l.OpenCount,
                    IsClicked = l.IsClicked,
                    ClickCount = l.ClickCount,
                    IsUnsubscribed = l.IsUnsubscribed,
                    OpenedOnUtc = l.OpenedOnUtc
                });
            });

            return model;
        }

        public virtual async Task<CampaingsListModel> PrepareCampaingsListModelAsync(CampaingsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get campaings
            var campaings = await _campaingsService.GetAllCampaingsAsync(
                name: searchModel.Name,
                statusid: searchModel.StatusId,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showHidden: true);

            //prepare grid model
            var model = await new CampaingsListModel().PrepareToGridAsync(searchModel, campaings, () =>
            {
                //fill in model values from the entity
                return campaings.SelectAwait(async campaings =>
                {
                    var campaingModel = new CampaingsModel();
                    var selectedAvailableOption = campaings.StatusId;
                    campaingModel.Id = campaings.Id;
                    campaingModel.Name = campaings.Name;
                    campaingModel.Subject = campaings.Subject;
                    campaingModel.Body = campaings.Body;
                    campaingModel.ScheduledDate = campaings.ScheduledDate;

                    campaingModel.EmailAccountId = campaings.EmailAccountId;
                    campaingModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(campaings.CreatedOnUtc, DateTimeKind.Utc);
                    campaingModel.UpdateOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(campaings.UpdatedOnUtc, DateTimeKind.Utc);
                    campaingModel.StatusId = campaings.StatusId;
                    campaingModel.StatusName = ((StatusEnum)campaings.StatusId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) campaingModel.StatusId
                        = (int)((StatusEnum)selectedAvailableOption);
                    campaingModel.TagsId = !string.IsNullOrEmpty(campaings.TagsId)
                    ? campaings.TagsId.Split(',').Select(int.Parse).ToList()
                    : new List<int>();
                    var (leads, contacts) = await _leadService.GetLeadsAndContactsByTagsAsync(campaingModel.TagsId.ToList());
                    var TotalLeads = leads.Count + contacts.Count;
                    campaingModel.TotalLead = TotalLeads;
                    var campaignEmailLogs = await _campaingsEmailLogsRepository.Table
    .Where(log => log.CampaingId == campaings.Id)
    .ToListAsync();
                    var totalsend = campaignEmailLogs.Count();
                    var totalOpen = campaignEmailLogs.Count(l => l.IsOpened);
                    var totalClick = campaignEmailLogs.Count(l => l.IsClicked);
                    var totalUnsubscribe = campaignEmailLogs.Count(l => l.IsUnsubscribed);

                    // Set to model
                    campaingModel.TotalSent = totalsend;
                    campaingModel.TotalOpens = totalOpen;
                    campaingModel.TotalClicks = totalClick;
                    campaingModel.UnsubscribedCount = totalUnsubscribe;
                    return campaingModel;
                });
            });
            return model;
        }

        public virtual async Task<CampaingsModel> PrepareCampaingsModelAsync(CampaingsModel model, Campaings campaings, bool excludeProperties = false)
        {
            var status = await StatusEnum.Draft.ToSelectListAsync();
            if (campaings != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new CampaingsModel();
                    model.Id = campaings.Id;
                    model.Name = campaings.Name;
                    model.Subject = campaings.Subject;
                    model.Body = campaings.Body;
                    model.ScheduledDate = campaings.ScheduledDate;
                    model.EmailAccountId = campaings.EmailAccountId;
                    model.StatusId = campaings.StatusId;
                    model.CreatedOnUtc = campaings.CreatedOnUtc;
                    model.UpdateOnUtc = campaings.UpdatedOnUtc;

                }
            }
            model.Status = status.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StatusId.ToString() == store.Value
            }).ToList();
            await PrepareTagsListAsync(model, model.Id);
            if (campaings != null)
            {
                model.TagsId = !string.IsNullOrEmpty(campaings.TagsId)
                    ? campaings.TagsId.Split(',').Select(int.Parse).ToList() : new List<int>();
                if (model.TagsId.Any())
                {
                    var (leads, contacts) = await _leadService.GetLeadsAndContactsByTagsAsync(model.TagsId.ToList());
                    model.TotalLeads = leads.Count + contacts.Count;
                }
                else
                {
                    model.TotalLeads = 0;
                }
                model.TotalEmailsSent = await _campaingsEmailLogsService.GetTotalEmailCountAsync(campaings.Id);
                model.TotalOpens = await _campaingsEmailLogsService.GetOpenedEmailCountAsync(campaings.Id);
                model.TotalClicks = await _campaingsEmailLogsService.GetClickedEmailCountAsync(campaings.Id);
                model.UnsubscribedCount = await _campaingsEmailLogsService.GetUnsubscribedCountAsync(campaings.Id);
                if (model.TotalEmailsSent > 0)
                {
                    model.UnsubscribeRate = $"{(model.UnsubscribedCount * 100 / model.TotalEmailsSent)}";
                }
                else
                {
                    model.UnsubscribeRate = "0";
                }
            }

            await PrepareEmailAccountListAsync(model);
            var tokens = new List<Token>();
            var lead = new Lead(); // You need to pass an actual Lead object here
            await _leadService.AddLeadTokensAsync(tokens, lead);
            var contact = new Contacts(); // Or a sample/test contact
            await _contactsService.AddContactTokensAsync(tokens, contact);
            model.AllowedTokens = tokens.Select(t => t.Key).Distinct().ToList();
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCampaingsSearchModelAsync(new CampaingsSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CampaingsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareCampaingsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareCampaingsModelAsync(new CampaingsModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CampaingsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var campaings = new Campaings();
                campaings.Id = model.Id;
                campaings.Name = model.Name;
                campaings.Subject = model.Subject;
                campaings.Body = model.Body;
                campaings.ScheduledDate = model.ScheduledDate.HasValue && model.ScheduledDate != DateTime.MinValue ? model.ScheduledDate : null;
                campaings.EmailAccountId = model.EmailAccountId;
                campaings.StatusId = model.StatusId;
                campaings.CreatedOnUtc = DateTime.UtcNow;
                campaings.UpdatedOnUtc = DateTime.UtcNow;
                campaings.TagsId = model.TagsId != null ? string.Join(",", model.TagsId) : null;
                await _campaingsService.InsertCampaingsAsync(campaings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Campaings.Added"));

                //ViewBag.RefreshPage = true;

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = campaings.Id });
            }

            //prepare model
            model = await PrepareCampaingsModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a campaings with the specified id
            var campaings = await _campaingsService.GetCampaingsByIdAsync(id);
            if (campaings == null)
                return RedirectToAction("List");

            //prepare model
            var model = await PrepareCampaingsModelAsync(null, campaings);
            var logs = await _campaingsEmailLogsService.GetByCampaignIdAsync(campaings.Id);
            model.TotalOpens = await _campaingsEmailLogsService.GetOpenedEmailCountAsync(campaings.Id);
            model.TotalClicks = await _campaingsEmailLogsService.GetClickedEmailCountAsync(campaings.Id);
            model.TotalEmailsSent = await _campaingsEmailLogsService.GetTotalEmailCountAsync(campaings.Id);
            return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/Edit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CampaingsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a campaings with the specified id
            var campaings = await _campaingsService.GetCampaingsByIdAsync(model.Id);
            if (campaings == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                campaings = new Campaings();
                campaings.Id = model.Id;
                campaings.Name = model.Name;
                campaings.Subject = model.Subject;
                campaings.Body = model.Body;
                campaings.ScheduledDate = model.ScheduledDate.HasValue && model.ScheduledDate != DateTime.MinValue ? model.ScheduledDate : null;
                campaings.EmailAccountId = model.EmailAccountId;
                campaings.StatusId = model.StatusId;
                campaings.CreatedOnUtc = model.CreatedOnUtc;
                campaings.UpdatedOnUtc = DateTime.UtcNow;
                campaings.TagsId = model.TagsId != null ? string.Join(",", model.TagsId) : string.Empty;
                if (model.TagsId != null && model.TagsId.Any())
                {
                    var tagIds = model.TagsId.Select(tag => int.Parse(tag.ToString())).ToList();
                    var (leads, contacts) = await _leadService.GetLeadsAndContactsByTagsAsync(tagIds);
                    model.TotalLeads = leads.Count + contacts.Count;
                }
                else
                {
                    model.TotalLeads = 0;
                }
                await _campaingsService.UpdateCampaingsAsync(campaings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.Campaings.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = campaings.Id });
                //ViewBag.RefreshPage = true;

            }

            //prepare model
            model = await PrepareCampaingsModelAsync(model, campaings, true);

            model.TotalOpens = await _campaingsEmailLogsService.GetOpenedEmailCountAsync(campaings.Id);
            model.TotalClicks = await _campaingsEmailLogsService.GetClickedEmailCountAsync(campaings.Id);
            model.TotalEmailsSent = await _campaingsEmailLogsService.GetTotalEmailCountAsync(campaings.Id);
            //if we got this far, something failed, redisplay form
            return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var campaings = await _campaingsService.GetCampaingsByIdAsync(id);
            if (campaings == null)
                return RedirectToAction("List");

            await _campaingsService.DeleteCampaingsAsync(campaings);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Campaings.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _campaingsService.GetCampaingsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _campaingsService.DeleteCampaingsAsync(item);
            }

            return Json(new { Result = true });
        }
        [HttpPost]
        public async Task<JsonResult> GetLeadCountByTags([FromBody] List<int> tagIds)
        {
            if (tagIds == null || !tagIds.Any())
                return Json(0);

            var (leads, contcats) = await _leadService.GetLeadsAndContactsByTagsAsync(tagIds);
            int leadCount = leads.Count + contcats.Count; // Ensure only count is returned

            return Json(leadCount);
        }

        [HttpGet]
        public async Task<IActionResult> Replicate(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Edit))
                return AccessDeniedView();

            var campaign = await _campaingsService.GetCampaingsByIdAsync(id);
            if (campaign == null)
            {
                _notificationService.ErrorNotification("Campaign not found.");
                return RedirectToAction("List");
            }
            var allTags = await _tagsService.GetAllTagsAsync(""); // Assuming this method exists
            var selectedTagIds = !string.IsNullOrEmpty(campaign.TagsId)
                ? campaign.TagsId.Split(',').Select(s => int.TryParse(s, out int tagId) ? tagId : 0).Where(tagId => tagId > 0).ToList()
                : new List<int>();
            var statusList = Enum.GetValues(typeof(StatusEnum)).Cast<StatusEnum>()
                            .Select(s => new SelectListItem
                            {
                                Value = ((int)s).ToString(),
                                Text = s.ToString()
                            }).ToList();

            var (leads, contacts) = await _leadService.GetLeadsAndContactsByTagsAsync(selectedTagIds);
            var tokens = new List<Token>();
            var lead = new Lead();
            await _leadService.AddLeadTokensAsync(tokens, lead);
            // Create a new campaign model with copied values
            var newCampaign = new CampaingsModel
            {
                Name = campaign.Name,
                EmailAccountId = campaign.EmailAccountId,
                Emails = (await _emailAccountService.GetAllEmailAccountsAsync())
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Email }).ToList(),
                Subject = campaign.Subject,
                Body = campaign.Body,
                TagsId = selectedTagIds,
                Tags = allTags.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name,
                    Selected = selectedTagIds.Contains(t.Id)
                }).ToList(),
                StatusId = (int)StatusEnum.Select,
                Status = statusList,
                ScheduledDate = campaign.ScheduledDate,
                TotalLeads = leads.Count + contacts.Count,
                AllowedTokens = tokens.Select(t => t.Key).ToList(),
            };

            return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/Create.cshtml", newCampaign);
        }
        public string WrapLinksWithTracking(string body, Guid trackingGuid, int campaignId)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(body);

            var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var originalUrl = link.GetAttributeValue("href", string.Empty);
                    if (!string.IsNullOrEmpty(originalUrl) &&
                        !originalUrl.Contains("email-tracking/click")) // avoid double wrap
                    {
                        var encodedRedirect = HttpUtility.UrlEncode(originalUrl);
                        var trackingUrl = $"{_webHelper.GetStoreLocation()}email-tracking/click?cid={campaignId}&guid={trackingGuid}&url={encodedRedirect}";
                        link.SetAttributeValue("href", trackingUrl);
                    }
                }
            }

            return htmlDoc.DocumentNode.OuterHtml;
        }
        public string ConvertPlainTextUrlsToLinks(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            // Skip if already contains anchor tags
            if (input.Contains("<a", StringComparison.OrdinalIgnoreCase))
                return input;

            // Step 1: Convert http, https, www
            var fullUrlRegex = new Regex(@"((http|https):\/\/[^\s<]+)|(www\.[^\s<]+)", RegexOptions.IgnoreCase);
            input = fullUrlRegex.Replace(input, match =>
            {
                var url = match.Value;
                var href = url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url : "http://" + url;
                return $"<a href=\"{href}\" target=\"_blank\">{url}</a>";
            });

            // Step 2: Convert plain domains like hoffmanboots.com
            // This regex matches domains like example.com, mysite.in, test.co.uk etc.
            var nakedDomainRegex = new Regex(@"(?<![@\/\w\.])([a-zA-Z0-9\-]+\.(com|net|org|info|biz|in|co|io|me|us|uk|shop|store|tech|site)(\/[^\s<]*)?)", RegexOptions.IgnoreCase);
            input = nakedDomainRegex.Replace(input, match =>
            {
                var url = match.Value;
                var href = "http://" + url;
                return $"<a href=\"{href}\" target=\"_blank\">{url}</a>";
            });

            return input;
        }

        [HttpPost]
        public async Task<IActionResult> EmailLogs(CampaingsEmailLogSearchModel searchModel)
        {
            var model = await PrepareCampaingsEmailLogListModelAsync(searchModel);
            return Json(model);
        }


        [HttpPost]
        public virtual async Task<IActionResult> SendMassEmail(CampaingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Edit))
                return AccessDeniedView();

            var campaign = await _campaingsService.GetCampaingsByIdAsync(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            model = await PrepareCampaingsModelAsync(model, campaign);

            try
            {
                var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(campaign.EmailAccountId);
                if (emailAccount == null)
                {
                    _notificationService.ErrorNotification("Invalid email account selected.");
                    return RedirectToAction("Edit", new { id = campaign.Id });
                }

                var campaignTagIds = campaign.TagsId?.Split(',')
                    .Select(id => int.TryParse(id, out var tagId) ? tagId : (int?)null)
                    .Where(id => id.HasValue)
                    .Select(id => id.Value)
                    .ToList();
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();

                //store information settings
                var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeScope);

                if (campaignTagIds == null || !campaignTagIds.Any())
                {
                    _notificationService.ErrorNotification("No tags are selected for this campaign.");
                    return RedirectToAction("Edit", new { id = campaign.Id });
                }

                var logoUrl = string.Empty;
                var logoPictureId = storeInformationSettings.LogoPictureId;

                if (logoPictureId > 0)
                {
                    logoUrl = await _pictureService.GetPictureUrlAsync(logoPictureId, showDefaultPicture: false);
                }

                if (string.IsNullOrEmpty(logoUrl))
                {
                    // Use a default logo if no logo is set
                    var pathBase = _httpContextAccessor.HttpContext.Request.PathBase.Value ?? string.Empty;
                    var storeLocation = _mediaSettings.UseAbsoluteImagePath ? _webHelper.GetStoreLocation() : $"{pathBase}/";
                    logoUrl = $"{storeLocation}Themes/{await _themeContext.GetWorkingThemeNameAsync()}/Content/images/logo.svg";
                }

                //  Embed the logo in the email body
                var logoHtml = $"<div style=\"text-align: center;\"><img src=\"{logoUrl}\" alt=\"Company Logo\" style=\"max-width: 200px; display: block; margin: 0 auto 10px;\" /></div>";



                // Get leads with matching tags & filter out those who have opted out
                var (leads, contacts) = await _leadService.GetLeadsAndContactsByTagsAsync(campaignTagIds);
                int totalLeads = leads.Count + contacts.Count;
                if (!leads.Any() && !contacts.Any())
                {
                    _notificationService.WarningNotification("No leads found with the selected tags or all leads have opted out.");
                    return RedirectToAction("Edit", new { id = campaign.Id });
                }

                int totalEmailsSent = 0;
                foreach (var lead in leads)
                {
                    var tokens = new List<Token>();
                    await _leadService.AddLeadTokensAsync(tokens, lead);
                    var campaignEmailSettings = await _settingService.LoadSettingAsync<CampaignEmailSettings>();

                    var linkedInUrl = campaignEmailSettings.LinkedInUrl;
                    var websiteUrl = campaignEmailSettings.WebsiteUrl;
                    var footerText = campaignEmailSettings.FooterText;

                    var unsubscribeLink = $"{_webHelper.GetStoreLocation()}unsubscribe?email={HttpUtility.UrlEncode(lead.Email)}";
                    // generate unique guid
                    var trackingGuid = Guid.NewGuid();

                    var socialLinksHtml = "";
                    if (!string.IsNullOrEmpty(linkedInUrl))
                    {
                        socialLinksHtml += $@"
                                    <td style= 'text-align: center; border-right: 1px solid #ddd;'>
                                        <a href='{linkedInUrl}' style='text-decoration: none; color: inherit; display:             block;'>
                                            <img src='https://cdn-icons-png.flaticon.com/512/174/174857.png'                           alt='LinkedIn' style='width: 20px; height: 20px;'><br>
                                        </a>
                                    </td>";
                    }
                    if (!string.IsNullOrEmpty(websiteUrl))
                    {
                        socialLinksHtml += $@"
                                                <td style='text-align: center;'>
                                                <a href='{websiteUrl}' style='text-decoration: none; color: inherit;                       display: block;'>
                                                <img src='https://cdn-icons-png.flaticon.com/512/1006/1006771.png'                         alt='Website' style='width: 20px; height: 20px;'><br>
                                                    </a>
                                                </td>";
                    }

                    var socialLinksTable = !string.IsNullOrEmpty(socialLinksHtml) ? $@"
                                            <table align='center' cellpadding='0' cellspacing='0' width='200' 
                                                   style='border: 1px solid #ddd; text-align: center; border-collapse:                       collapse;'>
                                                <tr>{socialLinksHtml}</tr>
                                            </table>" : "";

                    var footerTextHtml = !string.IsNullOrEmpty(footerText) ? $@"
                                    <tr>
                                        <td style='font-size: 12px; color: #777; padding-top: 10px; text-align: center;'>
                                            {footerText}
                                        </td>
                                    </tr>" : "";

                    var footerHtml = $@"
                                        <hr style='border: 0; margin: 20px 0;'>
                                        <table align='center' cellpadding='0' cellspacing='0' width='100%' style='text-               align: center; font-size: 14px; color: #555; border-collapse: collapse;                    border: none;'>
                                            <tr>
                                                <td>
                                                    {socialLinksTable} <!-- Now properly boxed and centered -->
                                                </td>
                                            </tr>
                                            {footerTextHtml} <!-- Only included if footer text exists -->
                                        </table>";

                    // Replace tokens dynamically

                    var pixelUrl = $"{_webHelper.GetStoreLocation()}email-tracking/pixel?cid={campaign.Id}&guid={trackingGuid}";
                    var pixelImgTag = $"<img src='{pixelUrl}' width='1' height='1' style='display:none;' alt='' />";

                    var rawBody = ReplaceTokens(campaign.Body, tokens);
                    var htmlWithLinks = ConvertPlainTextUrlsToLinks(rawBody);
                    var bodyWithWrappedLinks = WrapLinksWithTracking(htmlWithLinks, trackingGuid, campaign.Id);
                    var emailBody = $"{logoHtml}<br>{bodyWithWrappedLinks}<br>{footerHtml}{pixelImgTag}";

                    var localScheduledTime = campaign.ScheduledDate; // Keep it as local time in DB
                    var utcScheduledTime = localScheduledTime.HasValue ? localScheduledTime.Value.ToUniversalTime()
        : (DateTime?)null;
                    emailBody = emailBody.Replace("{unsubscribeLink}", unsubscribeLink);
                    // Check if this email with the same subject is already queued
                    var alreadyExists = await _queuedEmailRepository.Table
    .AnyAsync(q => q.To == lead.Email && q.Subject == campaign.Subject);

                    if (!alreadyExists)
                    {
                        var email = new QueuedEmail
                        {
                            From = emailAccount.Email,
                            FromName = emailAccount.Username,
                            Subject = campaign.Subject,
                            Body = emailBody,
                            To = lead.Email,
                            ToName = $"{lead.FirstName} {lead.LastName}",
                            EmailAccountId = emailAccount.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            SentTries = 0,
                            SentOnUtc = null,
                            DontSendBeforeDateUtc = utcScheduledTime
                        };

                        await _queuedEmailService.InsertQueuedEmailAsync(email);
                        await _campaingsEmailLogsService.InsertAsync(new CampaingsEmailLogs
                        {
                            CampaingId = campaign.Id,
                            Email = lead.Email, // or contacts1.Email
                            TrackingGuid = trackingGuid,
                            QueuedEmailId = email.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow,
                            IsOpened = false
                        });
                        totalEmailsSent++;
                    }
                    else
                    {
                        _notificationService.WarningNotification("No emails were queued.");

                        // Optionally log or notify that it's skipped
                        //_logger.Information($"Skipped duplicate email for {lead.Email} with subject '{campaign.Subject}'");
                    }

                }
                foreach (var contacts1 in contacts)
                {
                    var tokens = new List<Token>();
                    await _contactsService.AddContactTokensAsync(tokens, contacts1);
                    var campaignEmailSettings = await _settingService.LoadSettingAsync<CampaignEmailSettings>();

                    var linkedInUrl = campaignEmailSettings.LinkedInUrl;
                    var websiteUrl = campaignEmailSettings.WebsiteUrl;
                    var footerText = campaignEmailSettings.FooterText;

                    var unsubscribeLink = $"{_webHelper.GetStoreLocation()}unsubscribe?email={HttpUtility.UrlEncode(contacts1.Email)}";

                    // generate unique guid
                    var trackingGuid = Guid.NewGuid();

                    var socialLinksHtml = "";
                    if (!string.IsNullOrEmpty(linkedInUrl))
                    {
                        socialLinksHtml += $@"
                                    <td style= 'text-align: center; border-right: 1px solid #ddd;'>
                                        <a href='{linkedInUrl}' style='text-decoration: none; color: inherit; display:             block;'>
                                            <img src='https://cdn-icons-png.flaticon.com/512/174/174857.png'                           alt='LinkedIn' style='width: 20px; height: 20px;'><br>
                                        </a>
                                    </td>";
                    }
                    if (!string.IsNullOrEmpty(websiteUrl))
                    {
                        socialLinksHtml += $@"
                                                <td style='text-align: center;'>
                                                <a href='{websiteUrl}' style='text-decoration: none; color: inherit;                       display: block;'>
                                                <img src='https://cdn-icons-png.flaticon.com/512/1006/1006771.png'                         alt='Website' style='width: 20px; height: 20px;'><br>
                                                    </a>
                                                </td>";
                    }

                    var socialLinksTable = !string.IsNullOrEmpty(socialLinksHtml) ? $@"
                                            <table align='center' cellpadding='0' cellspacing='0' width='200' 
                                                   style='border: 1px solid #ddd; text-align: center; border-collapse:                       collapse;'>
                                                <tr>{socialLinksHtml}</tr>
                                            </table>" : "";

                    var footerTextHtml = !string.IsNullOrEmpty(footerText) ? $@"
                                    <tr>
                                        <td style='font-size: 12px; color: #777; padding-top: 10px; text-align: center;'>
                                            {footerText}
                                        </td>
                                    </tr>" : "";

                    var footerHtml = $@"
                                        <hr style='border: 0; margin: 20px 0;'>
                                        <table align='center' cellpadding='0' cellspacing='0' width='100%' style='text-               align: center; font-size: 14px; color: #555; border-collapse: collapse;                    border: none;'>
                                            <tr>
                                                <td>
                                                    {socialLinksTable} <!-- Now properly boxed and centered -->
                                                </td>
                                            </tr>
                                            {footerTextHtml} <!-- Only included if footer text exists -->
                                        </table>";

                    // Replace tokens dynamically
                    var pixelUrl = $"{_webHelper.GetStoreLocation()}email-tracking/pixel?cid={campaign.Id}&guid={trackingGuid}";
                    var pixelImgTag = $"<img src='{pixelUrl}' width='1' height='1' style='display:none;' alt='' />";

                    var rawBody = ReplaceTokens(campaign.Body, tokens);
                    var htmlWithLinks = ConvertPlainTextUrlsToLinks(rawBody);
                    var bodyWithWrappedLinks = WrapLinksWithTracking(htmlWithLinks, trackingGuid, campaign.Id);
                    var emailBody = $"{logoHtml}<br>{bodyWithWrappedLinks}<br>{footerHtml}{pixelImgTag}";

                    var localScheduledTime = campaign.ScheduledDate; // Keep it as local time in DB
                    var utcScheduledTime = localScheduledTime.HasValue ? localScheduledTime.Value.ToUniversalTime()
        : (DateTime?)null;
                    emailBody = emailBody.Replace("{unsubscribeLink}", unsubscribeLink);
                    // Check if this email with the same subject is already queued
                    var alreadyExists = await _queuedEmailRepository.Table
    .AnyAsync(q => q.To == contacts1.Email && q.Subject == campaign.Subject);

                    if (!alreadyExists)
                    {
                        var email = new QueuedEmail
                        {
                            From = emailAccount.Email,
                            FromName = emailAccount.Username,
                            Subject = campaign.Subject,
                            Body = emailBody,
                            To = contacts1.Email,
                            ToName = $"{contacts1.FirstName} {contacts1.LastName}",
                            EmailAccountId = emailAccount.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            SentTries = 0,
                            SentOnUtc = null,
                            DontSendBeforeDateUtc = utcScheduledTime
                        };

                        await _queuedEmailService.InsertQueuedEmailAsync(email);
                        await _campaingsEmailLogsService.InsertAsync(new CampaingsEmailLogs
                        {
                            CampaingId = campaign.Id,
                            Email = contacts1.Email, // or contacts1.Email
                            TrackingGuid = trackingGuid,
                            QueuedEmailId = email.Id,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow,
                            IsOpened = false
                        });
                        totalEmailsSent++;
                    }
                    else
                    {
                        _notificationService.WarningNotification("No emails were queued.");

                        // Optionally log or notify that it's skipped
                        //_logger.Information($"Skipped duplicate email for {lead.Email} with subject '{campaign.Subject}'");
                    }

                }

                if (totalEmailsSent > 0)
                {
                    campaign.StatusId = (int)StatusEnum.Completed;
                    await _campaingsService.UpdateCampaingsAsync(campaign);
                    campaign = await _campaingsService.GetCampaingsByIdAsync(campaign.Id);
                    _notificationService.SuccessNotification($"Successfully queued {totalEmailsSent} emails.");
                }
                else
                {
                    _notificationService.WarningNotification("No emails were queued.");
                }
                TempData["TotalLeads"] = totalLeads;
                return RedirectToAction("Edit", new { id = campaign.Id });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("Edit", new { id = campaign.Id });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> SendTestEmail(CampaingsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageCampaigns, PermissionAction.Edit))
                return AccessDeniedView();

            var campaign = await _campaingsService.GetCampaingsByIdAsync(model.Id);
            if (campaign == null)
                return RedirectToAction("List");

            if (!CommonHelper.IsValidEmail(model.SendTestEmailTo))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.WrongEmail"));
                return View("~/Plugins/Misc.SatyanamCRM/Views/Campaings/Edit.cshtml", model);
            }

            try
            {
                var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(campaign.EmailAccountId);
                if (emailAccount == null)
                {
                    _notificationService.ErrorNotification("Invalid email account selected.");
                    return RedirectToAction("Edit", new { id = campaign.Id });
                }

                // Get store settings and logo
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(storeScope);
                var logoUrl = string.Empty;
                var logoPictureId = storeInformationSettings.LogoPictureId;

                if (logoPictureId > 0)
                {
                    logoUrl = await _pictureService.GetPictureUrlAsync(logoPictureId, showDefaultPicture: false);
                }

                if (string.IsNullOrEmpty(logoUrl))
                {
                    var pathBase = _httpContextAccessor.HttpContext.Request.PathBase.Value ?? string.Empty;
                    var storeLocation = _mediaSettings.UseAbsoluteImagePath ? _webHelper.GetStoreLocation() : $"{pathBase}/";
                    logoUrl = $"{storeLocation}Themes/{await _themeContext.GetWorkingThemeNameAsync()}/Content/images/logo.svg";
                }

                // Embed the logo in the email body
                var logoHtml = $"<div style=\"text-align: center;\"><img src=\"{logoUrl}\" alt=\"Company Logo\" style=\"max-width: 200px; display: block; margin: 0 auto 10px;\" /></div>";

                var campaignEmailSettings = await _settingService.LoadSettingAsync<CampaignEmailSettings>();
                var linkedInUrl = campaignEmailSettings.LinkedInUrl;
                var websiteUrl = campaignEmailSettings.WebsiteUrl;
                var footerText = campaignEmailSettings.FooterText;

                var socialLinksHtml = "";
                if (!string.IsNullOrEmpty(linkedInUrl))
                {
                    socialLinksHtml += $@"
                                        <td style='text-align: center; border-right: 1px solid #ddd;'>
                                            <a href='{linkedInUrl}' style='text-decoration: none; color: inherit; display: block;'>
                                                <img src='https://cdn-icons-png.flaticon.com/512/174/174857.png' alt='LinkedIn' width='32'><br>
                                            </a>
                                        </td>";
                }
                if (!string.IsNullOrEmpty(websiteUrl))
                {
                    socialLinksHtml += $@"
                                        <td style='text-align: center;'>
                                        <a href='{websiteUrl}' style='text-decoration: none; color: inherit; display: block;'>
                                                <img src='https://cdn-icons-png.flaticon.com/512/1006/1006771.png' alt='Website' width='32'><br>
                                            </a>
                                        </td>";
                }

                var socialLinksTable = !string.IsNullOrEmpty(socialLinksHtml) ? $@"
                                        <table align='center' cellpadding='0' cellspacing='0' width='200' 
                                          style='border: 1px solid #ddd; text-align: center; border-collapse: collapse;'>
                                            <tr>{socialLinksHtml}</tr>
                                        </table>" : "";

                var footerTextHtml = !string.IsNullOrEmpty(footerText) ? $@"
                                    <tr>
                                        <td style='font-size: 12px; color: #777; padding-top: 10px; text-align: center;'>
                                            {footerText}
                                        </td>
                                    </tr>" : "";

                var footerHtml = $@"<hr style='border: 0; margin: 20px 0;'>
                                    <table align='center' cellpadding='0' cellspacing='0' width='100%' style='text-align:        center; font-size: 14px; color: #555; border-collapse: collapse; border: none;'>
                                        <tr>
                                            <td>
                                                {socialLinksTable} <!-- Now properly boxed and centered -->
                                            </td>
                                        </tr>
                                        {footerTextHtml} <!-- Only included if footer text exists -->
                                    </table>";
                var testLead = new LeadModel
                {
                    FirstName = "Test",
                    LastName = "User",
                    Email = model.SendTestEmailTo
                };

                // **New: Generate Unsubscribe Link**
                var unsubscribeLink = $"{_webHelper.GetStoreLocation()}unsubscribe?email={HttpUtility.UrlEncode(testLead.Email)}";

                // Construct final email body
                var emailBody = $"{logoHtml}<br>{campaign.Body}<br>{footerHtml}";
                emailBody = emailBody.Replace("{unsubscribeLink}", unsubscribeLink);
                // Send email immediately
                await _emailSender.SendEmailAsync(
                    emailAccount,
                    campaign.Subject,
                    emailBody,
                    emailAccount.Email,
                    emailAccount.Username,
                    model.SendTestEmailTo,
                    "Test User"
                );

                _notificationService.SuccessNotification("Test email sent successfully.");
                return RedirectToAction("Edit", new { id = campaign.Id });
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                return RedirectToAction("Edit", new { id = campaign.Id });
            }
        }
        #endregion
    }
}
