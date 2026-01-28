using App.Core;
using App.Core.Domain.Security;
using App.Data.Extensions;
using App.Services;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Framework.Controllers;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using Satyanam.Nop.Plugin.Misc.SatyanamCRM.Services;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area("Admin")]
    public class LinkedInFollowupsController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly ILinkedInFollowupsService _linkedInFollowupsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILinkedInFollowupsExportService _linkedInFollowupsExportService;
        private readonly ILinkedInFollowupsImportService _linkedInFollowupsImportService;

        #endregion

        #region Ctor 

        public LinkedInFollowupsController(IPermissionService permissionService,
                               ILinkedInFollowupsService linkedInFollowupsService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               IDateTimeHelper dateTimeHelper,
                               ILinkedInFollowupsExportService linkedInFollowupsExportService,
                               ILinkedInFollowupsImportService linkedInFollowupsImportService)
        {
            _permissionService = permissionService;
            _linkedInFollowupsService = linkedInFollowupsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _linkedInFollowupsExportService = linkedInFollowupsExportService;
            _linkedInFollowupsImportService = linkedInFollowupsImportService;
        }

        #endregion

        #region Utilities
        private static string GetDisplayName(Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttributes(false)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            return displayAttribute?.Name ?? enumValue.ToString();
        }

        public void PrepareFollowUpModel(LinkedInFollowupsModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Status = Enum.GetValues(typeof(FollowUpStatusEnum))
                .Cast<FollowUpStatusEnum>()
                .Select(x => new SelectListItem
                {
                    Value = ((int)x).ToString(),
                    Text = GetDisplayName(x)
                }).ToList();
        }
        [HttpGet]
        public async Task<IActionResult> GetStatusSummary()
        {
            // Fetch all records
            var all = await _linkedInFollowupsService.GetAllLinkedInFollowupsAsync(
                "", "", "", "", "", null, null, null, 0, int.MaxValue, true, null);

            // Group by StatusId to count how many per status
            var summary = all
                .GroupBy(x => x.StatusId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build the final list including all enum values
            var result = Enum.GetValues(typeof(FollowUpStatusEnum))
                .Cast<FollowUpStatusEnum>()
                .Select(status =>
                {
                    var displayName = status
                        .GetType()
                        .GetMember(status.ToString())
                        .First()
                        .GetCustomAttributes(false)
                        .OfType<DisplayAttribute>()
                        .FirstOrDefault()?.Name ?? status.ToString();

                    return new
                    {
                        StatusId = (int)status,
                        StatusName = displayName,
                        Count = summary.ContainsKey((int)status) ? summary[(int)status] : 0,
                        ColorCode = GetColorForStatus(status) // Optional helper for colors
                    };
                })
                .ToList();

            return Json(result);
        }

        // Optional helper method for color coding (you can adjust colors)
        private string GetColorForStatus(FollowUpStatusEnum status)
        {
            return status switch
            {
                FollowUpStatusEnum.Pending => "#f0ad4e",
                FollowUpStatusEnum.MessageSent => "#5bc0de",
                FollowUpStatusEnum.Replied => "#5cb85c",
                FollowUpStatusEnum.InConversation => "#0275d8",
                FollowUpStatusEnum.NotInterested => "#d9534f",
                FollowUpStatusEnum.Converted => "#28a745",
                FollowUpStatusEnum.Closed => "#6c757d",
                FollowUpStatusEnum.BlockedOrRemoved => "#343a40",
                _ => "#777"
            };
        }



        public virtual async Task<LinkedInFollowupsSearchModel> PrepareLinkedInFollowupsSearchModelAsync(LinkedInFollowupsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Status = Enum.GetValues(typeof(FollowUpStatusEnum))
        .Cast<FollowUpStatusEnum>()
        .Select(e => new SelectListItem
        {
            Text = e.ToString(),
            Value = ((int)e).ToString()
        })
        .ToList();

            // Add "All" option
            searchModel.Status.Insert(0, new SelectListItem { Text = "All", Value = "" });
            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<LinkedInFollowupsListModel> PrepareLinkedInFollowupsListModelAsync(LinkedInFollowupsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get linkedInFollowups
            var linkedInFollowups = await _linkedInFollowupsService.GetAllLinkedInFollowupsAsync(showHidden: true,
                firstname: searchModel.SearchFirstName,
                lastname: searchModel.SearchLastName,
                email: searchModel.SearchEmail,
                linkedinUrl: searchModel.SearchLinkedinUrl,
                website: searchModel.SearchWebsiteUrl,
                lastMessageDate: searchModel.SearchLastMessDate,
                nextFollowUpDate: searchModel.NextFollowUpDate,
                statusId: searchModel.SearchStatus,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new LinkedInFollowupsListModel().PrepareToGridAsync(searchModel, linkedInFollowups, () =>
            {
                //fill in model values from the entity
                return linkedInFollowups.SelectAwait(async linkedInFollowups =>
                {
                    var linkedInFollowupsModel = new LinkedInFollowupsModel();
                    var selectedAvailableOption = linkedInFollowups.StatusId;
                    linkedInFollowupsModel.Id = linkedInFollowups.Id;
                    linkedInFollowupsModel.FirstName = linkedInFollowups.FirstName;
                    linkedInFollowupsModel.LastName = linkedInFollowups.LastName;
                    linkedInFollowupsModel.LinkedinUrl = linkedInFollowups.LinkedinUrl;
                    linkedInFollowupsModel.Email = linkedInFollowups.Email;
                    linkedInFollowupsModel.WebsiteUrl = linkedInFollowups.WebsiteUrl;
                    linkedInFollowupsModel.LastMessageDate = linkedInFollowups.LastMessageDate;
                    linkedInFollowupsModel.LastMessDate = linkedInFollowups.LastMessageDate.HasValue ? linkedInFollowups.LastMessageDate.Value.ToString("MM/dd/yyyy") : "";
                    linkedInFollowupsModel.FollowUp = linkedInFollowups.FollowUp;
                    linkedInFollowupsModel.NextFollowUpDate = linkedInFollowups.NextFollowUpDate;
                    linkedInFollowupsModel.NextFollowupsDate = linkedInFollowups.NextFollowUpDate.HasValue
                        ? linkedInFollowups.NextFollowUpDate.Value.ToString("MM/dd/yyyy")
                        : "";
                    linkedInFollowupsModel.DaysUntilNext = linkedInFollowups.DaysUntilNext;
                    linkedInFollowupsModel.RemainingFollowUps = linkedInFollowups.RemainingFollowUps;
                    linkedInFollowupsModel.AutoStatus = linkedInFollowups.AutoStatus;
                    linkedInFollowupsModel.StatusId = linkedInFollowups.StatusId;
                    linkedInFollowupsModel.StatusText = ((FollowUpStatusEnum)linkedInFollowups.StatusId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) linkedInFollowupsModel.StatusId
                        = (int)((FollowUpStatusEnum)selectedAvailableOption);
                    linkedInFollowupsModel.Notes = linkedInFollowups.Notes;
                    linkedInFollowupsModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(linkedInFollowups.CreatedOnUtc, DateTimeKind.Utc);
                    linkedInFollowupsModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(linkedInFollowups.UpdatedOnUtc, DateTimeKind.Utc);

                    return linkedInFollowupsModel;
                });
            });
            return model;
        }

        public virtual async Task<LinkedInFollowupsModel> PrepareLinkedInFollowupsModelAsync(LinkedInFollowupsModel model, LinkedInFollowups linkedInFollowups, bool excludeProperties = false)
        {
            var statusList = await FollowUpStatusEnum.None.ToSelectListAsync();
            if (linkedInFollowups != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new LinkedInFollowupsModel();
                    model.Id = linkedInFollowups.Id;
                    model.FirstName = linkedInFollowups.FirstName;
                    model.LastName = linkedInFollowups.LastName;
                    model.LinkedinUrl = linkedInFollowups.LinkedinUrl;
                    model.Email = linkedInFollowups.Email;
                    model.WebsiteUrl = linkedInFollowups.WebsiteUrl;
                    model.LastMessageDate = linkedInFollowups.LastMessageDate;
                    model.FollowUp = linkedInFollowups.FollowUp;
                    model.NextFollowUpDate = linkedInFollowups.NextFollowUpDate;
                    model.DaysUntilNext = linkedInFollowups.DaysUntilNext;
                    model.RemainingFollowUps = linkedInFollowups.RemainingFollowUps;
                    model.AutoStatus = linkedInFollowups.AutoStatus;
                    model.StatusId = linkedInFollowups.StatusId;
                    model.Notes = linkedInFollowups.Notes;
                    model.CreatedOnUtc = linkedInFollowups.CreatedOnUtc;
                    model.UpdatedOnUtc = linkedInFollowups.UpdatedOnUtc;
                }
            }
            model.Status = statusList.Select(s => new SelectListItem
            {
                Value = s.Value,
                Text = s.Text,
                Selected = s.Value == model.StatusId.ToString()
            }).ToList();
            return model;
        }
        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLinkedInFollowupsSearchModelAsync(new LinkedInFollowupsSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/LinkedInFollowups/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(LinkedInFollowupsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareLinkedInFollowupsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareLinkedInFollowupsModelAsync(new LinkedInFollowupsModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/LinkedInFollowups/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(LinkedInFollowupsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Add))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(model.FirstName))
                ModelState.AddModelError(nameof(model.FirstName), "Enter a first name");

            if (string.IsNullOrWhiteSpace(model.LastName))
                ModelState.AddModelError(nameof(model.LastName), "Enter a last name");

            //  Parse LastMessageDate safely from form input
            DateTime? lastMessageDate = null;
            if (!string.IsNullOrEmpty(Request.Form["LastMessageDate"]))
            {
                var rawDate = Request.Form["LastMessageDate"].ToString().Trim();

                // Try multiple formats
                if (DateTime.TryParseExact(rawDate, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
                    lastMessageDate = parsed;
                else if (DateTime.TryParseExact(rawDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
                    lastMessageDate = parsed;
                else if (DateTime.TryParse(rawDate, out parsed))
                    lastMessageDate = parsed;
            }

            if (!lastMessageDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.LastMessageDate), "Enter a valid Last Message Date (MM/dd/yyyy)");
            }

            if (!ModelState.IsValid)
            {
                model = await PrepareLinkedInFollowupsModelAsync(model, null, true);
                return View("~/Plugins/Misc.SatyanamCRM/Views/LinkedInFollowups/Create.cshtml", model);
            }

            // --- Formula Logic ---
            DateTime? nextFollowUpDate = null;
            int? daysUntilNext = null;
            int? remainingFollowUps = null;
            string autoStatus = "";

            var followUp = model.FollowUp;

            //  Case 1: Follow-up = 0 → same day, no scheduling
            if (lastMessageDate.HasValue && followUp == 0)
            {
                nextFollowUpDate = lastMessageDate;  // same as last message
                daysUntilNext = 0;
                remainingFollowUps = 0;
                autoStatus = "No follow-up scheduled";
            }
            //  Case 2: Normal 1–10 follow-ups → formula-based scheduling
            else if (lastMessageDate.HasValue && followUp >= 1 && followUp <= 10)
            {
                int addDays = followUp switch
                {
                    1 => 3,
                    2 => 5,
                    3 => 8,
                    4 => 10,
                    5 => 15,
                    6 => 20,
                    7 => 25,
                    8 => 30,
                    9 => 40,
                    10 => 50,
                    _ => 0
                };

                nextFollowUpDate = lastMessageDate.Value.AddDays(addDays);
                daysUntilNext = (nextFollowUpDate.Value - DateTime.Today).Days;
                remainingFollowUps = 11 - followUp;

                if (daysUntilNext < 0)
                    autoStatus = $"Overdue by {Math.Abs(daysUntilNext.Value)} days";
                else if (daysUntilNext == 0)
                    autoStatus = "Due Today";
                else
                    autoStatus = $"Scheduled in {daysUntilNext.Value} days";
            }
            //  Case 3: No date or invalid follow-up
            else
            {
                nextFollowUpDate = null;
                daysUntilNext = null;
                remainingFollowUps = 0;
                autoStatus = "No follow-up scheduled";
            }

            //  Assign computed values to model
            model.LastMessageDate = lastMessageDate;
            model.NextFollowUpDate = nextFollowUpDate;
            model.DaysUntilNext = daysUntilNext ?? 0;
            model.RemainingFollowUps = remainingFollowUps ?? 0;
            model.AutoStatus = autoStatus;

            if (model.StatusId == 0)
                model.StatusId = (int)FollowUpStatusEnum.None;

            //  Save to database
            var linkedInFollowUps = new LinkedInFollowups
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                LinkedinUrl = model.LinkedinUrl,
                Email = model.Email,
                WebsiteUrl = model.WebsiteUrl,
                LastMessageDate = model.LastMessageDate,
                FollowUp = model.FollowUp,
                NextFollowUpDate = model.NextFollowUpDate,
                DaysUntilNext = model.DaysUntilNext,
                RemainingFollowUps = model.RemainingFollowUps,
                AutoStatus = model.AutoStatus,
                StatusId = model.StatusId,
                Notes = model.Notes,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            await _linkedInFollowupsService.InsertLinkedInFollowupsAsync(linkedInFollowUps);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.LinkedInFollowUps.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = linkedInFollowUps.Id });
        }


        [HttpPost]

        public virtual async Task<IActionResult> InlineEdit(LinkedInFollowupsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            var linkedInFollowUps = await _linkedInFollowupsService.GetLinkedInFollowupsByIdAsync(model.Id);
            if (linkedInFollowUps == null)
                return Json(new { error = "Record not found" });

            if (!model.LastMessageDate.HasValue && !string.IsNullOrEmpty(model.LastMessDate))
            {
                if (DateTime.TryParse(model.LastMessDate, out DateTime parsedDate))
                    model.LastMessageDate = parsedDate;
            }

            // --- Formula Logic Start ---
            DateTime? nextFollowUpDate = null;
            int? daysUntilNext = null;
            int? remainingFollowUps = null;
            string autoStatus = "";

            var followUp = model.FollowUp;
            var lastMessageDate = model.LastMessageDate;

            if (lastMessageDate.HasValue && followUp >= 1 && followUp <= 10)
            {
                int addDays = followUp switch
                {
                    1 => 3,
                    2 => 5,
                    3 => 8,
                    4 => 10,
                    5 => 15,
                    6 => 20,
                    7 => 25,
                    8 => 30,
                    9 => 40,
                    10 => 50,
                    _ => 0
                };
                nextFollowUpDate = lastMessageDate.Value.AddDays(addDays);
            }

            if (nextFollowUpDate.HasValue)
                daysUntilNext = (nextFollowUpDate.Value - DateTime.Today).Days;

            remainingFollowUps = (followUp == 0 || followUp > 10) ? 0 : 11 - followUp;

            if (!nextFollowUpDate.HasValue)
                autoStatus = "No follow-up scheduled";
            else if (daysUntilNext < 0)
                autoStatus = $"Overdue by {Math.Abs(daysUntilNext.Value)} days";
            else if (daysUntilNext == 0)
                autoStatus = "Due Today";
            else
                autoStatus = $"Scheduled in {daysUntilNext.Value} days";
            // --- Formula Logic End ---
            if (model.StatusId == 0)
                model.StatusId = linkedInFollowUps.StatusId;
            
            //  Update database entity
            linkedInFollowUps.FirstName = model.FirstName;
            linkedInFollowUps.LastName = model.LastName;
            linkedInFollowUps.Email = model.Email;
            linkedInFollowUps.LinkedinUrl = model.LinkedinUrl;
            linkedInFollowUps.WebsiteUrl = model.WebsiteUrl;
            linkedInFollowUps.LastMessageDate = model.LastMessageDate;
            linkedInFollowUps.FollowUp = model.FollowUp;
            linkedInFollowUps.NextFollowUpDate = nextFollowUpDate;
            linkedInFollowUps.DaysUntilNext = daysUntilNext ?? 0;
            linkedInFollowUps.RemainingFollowUps = remainingFollowUps ?? 0;
            linkedInFollowUps.AutoStatus = autoStatus;
            linkedInFollowUps.StatusId = model.StatusId;
            linkedInFollowUps.Notes = model.Notes;
            linkedInFollowUps.UpdatedOnUtc = DateTime.UtcNow;

            await _linkedInFollowupsService.UpdateLinkedInFollowupsAsync(linkedInFollowUps);

            return Json(new
            {
                success = true,
                data = new
                {
                    model.Id,
                    model.FirstName,
                    model.LastName,
                    model.Email,
                    model.LinkedinUrl,
                    model.WebsiteUrl,
                    LastMessDate = model.LastMessageDate?.ToString("yyyy-MM-dd") ?? "",
                    FollowUp = model.FollowUp,
                    NextFollowupsDate = nextFollowUpDate?.ToString("yyyy-MM-dd") ?? "",
                    DaysUntilNext = daysUntilNext ?? 0,
                    RemainingFollowUps = remainingFollowUps ?? 0,
                    AutoStatus = autoStatus,
                    StatusId = model.StatusId,
                    Notes = model.Notes,
                    UpdatedOnUtc = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            });
        }
        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id, string btnId, string formId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            // Get the follow-up record
            var followUp = await _linkedInFollowupsService.GetLinkedInFollowupsByIdAsync(id);
            if (followUp == null)
                return RedirectToAction("List");

            // Prepare the model
            var model = new LinkedInFollowupsModel
            {
                Id = followUp.Id,
                StatusId = followUp.StatusId
            };

            //  prepare your enum dropdown here directly
            PrepareFollowUpModel(model);

            // These are needed so popup knows which button & form to refresh
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            //  return your plugin popup view
            return View("~/Plugins/Misc.SatyanamCRM/Views/LinkedInFollowups/StatusChange.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(LinkedInFollowupsModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _linkedInFollowupsService.GetLinkedInFollowupsByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");

            entity.StatusId = model.StatusId;
            await _linkedInFollowupsService.UpdateLinkedInFollowupsAsync(entity);

            ViewBag.RefreshPage = true; // important
            return View("~/Plugins/Misc.SatyanamCRM/Views/LinkedInFollowups/StatusChange.cshtml", model);
        }


        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _linkedInFollowupsService.GetLinkedInFollowupsByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _linkedInFollowupsService.DeleteLinkedInFollowupsAsync(item);
            }

            return Json(new { Result = true });
        }
        #endregion

        #region Export/Import

        [HttpPost, ActionName("ExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportToExcel(LinkedInFollowupsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageLeads, PermissionAction.View))
                return AccessDeniedView();

            // fetch all matched records (pageSize = int.MaxValue)
            var list = await _linkedInFollowupsService.GetAllLinkedInFollowupsAsync(
                showHidden: true,
                firstname: searchModel.SearchFirstName,
                lastname: searchModel.SearchLastName,
                email: searchModel.SearchEmail,
                linkedinUrl: searchModel.SearchLinkedinUrl,
                website: searchModel.SearchWebsiteUrl,
                lastMessageDate: searchModel.SearchLastMessDate,
                nextFollowUpDate: searchModel.NextFollowUpDate,
                statusId: searchModel.SearchStatus,
                pageIndex: 0,
                pageSize: int.MaxValue);

            // convert to DTOs for export
            var dtoList = new List<LinkedInFollowupsDto>();
            foreach (var item in list)
            {
                var dto = new LinkedInFollowupsDto
                {
                    Id = item.Id,
                    FirstName = item.FirstName ?? " ",
                    LastName = item.LastName ?? " ",
                    LinkedinUrl = item.LinkedinUrl ?? " ",
                    Email = item.Email ?? " ",
                    WebsiteUrl = item.WebsiteUrl ?? " ",
                    LastMessDate = item.LastMessageDate?.ToString("yyyy-MM-dd") ?? " ",
                    FollowUp = item.FollowUp,
                    NextFollowupsDate = item.NextFollowUpDate?.ToString("yyyy-MM-dd") ?? " ",
                    DaysUntilNext = item.DaysUntilNext,
                    RemainingFollowUps = item.RemainingFollowUps,
                    AutoStatus = item.AutoStatus ?? " ",
                    StatusId = item.StatusId,
                    StatusName = item.StatusId > 0 ? ((FollowUpStatusEnum)item.StatusId).ToString() : "None",
                    Notes = item.Notes ?? " "
                };
                dtoList.Add(dto);
            }

            try
            {
                var bytes = await _linkedInFollowupsExportService.ExportLinkedInFollowupsToExcelAsync(dtoList);
                return File(bytes, MimeTypes.TextXlsx, "LinkedInFollowups.xlsx");
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
                _notificationService.WarningNotification("No records selected for export.");
                return RedirectToAction("List");
            }

            var dtoList = new List<LinkedInFollowupsDto>();
            try
            {
                foreach (var id in selectedIds)
                {
                    var item = await _linkedInFollowupsService.GetLinkedInFollowupsByIdAsync(id)
                        ?? throw new ArgumentException("No LinkedInFollowup found with the specified id");

                    dtoList.Add(new LinkedInFollowupsDto
                    {
                        Id = item.Id,
                        FirstName = item.FirstName ?? " ",
                        LastName = item.LastName ?? " ",
                        LinkedinUrl = item.LinkedinUrl ?? " ",
                        Email = item.Email ?? " ",
                        WebsiteUrl = item.WebsiteUrl ?? " ",
                        LastMessDate = item.LastMessageDate?.ToString("yyyy-MM-dd") ?? " ",
                        FollowUp = item.FollowUp,
                        NextFollowupsDate = item.NextFollowUpDate?.ToString("yyyy-MM-dd") ?? " ",
                        DaysUntilNext = item.DaysUntilNext,
                        RemainingFollowUps = item.RemainingFollowUps,
                        AutoStatus = item.AutoStatus ?? " ",
                        StatusId = item.StatusId,
                        StatusName = item.StatusId > 0 ? ((FollowUpStatusEnum)item.StatusId).ToString() : "None",
                        Notes = item.Notes ?? " "
                    });
                }

                var bytes = await _linkedInFollowupsExportService.ExportLinkedInFollowupsToExcelAsync(dtoList);
                return File(bytes, MimeTypes.TextXlsx, "LinkedInFollowups_Selected.xlsx");
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
                    // The import service will create new records or update existing ones (match by Email or LinkedinUrl)
                    var result = await _linkedInFollowupsImportService.ImportLinkedInFollowupsFromExcelAsync(importFile);
                }
                else
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
                    return RedirectToAction("List");
                }

                return RedirectToAction("List");
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                return RedirectToAction("List");
            }
        }
        #endregion
    }
}
