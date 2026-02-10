using App.Core;
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
using Satyanam.Nop.Plugin.SatyanamCRM.Models.ConnectionRequests;
using Satyanam.Nop.Plugin.SatyanamCRM.Models.LinkedInFollowups;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace Satyanam.Nop.Plugin.Misc.SatyanamCRM.Controllers
{
    [AutoValidateAntiforgeryToken]
    [AuthorizeAdmin]
    [Area("Admin")]
    public class ConnectionRequestController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IConnectionRequestService _connectionRequestService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IConnectionRequestExportService _connectionRequestExportService;
        private readonly IConnectionRequestImportService _connectionRequestImportService;
        private readonly ILinkedInFollowupsService _linkedInFollowupsService;

        #endregion

        #region Ctor 

        public ConnectionRequestController(IPermissionService permissionService,
                               IConnectionRequestService connectionRequestService,
                               INotificationService notificationService,
                               ILocalizationService localizationService,
                               IDateTimeHelper dateTimeHelper,
                               IConnectionRequestExportService connectionRequestExportService,
                               IConnectionRequestImportService connectionRequestImportService,
                               ILinkedInFollowupsService linkedInFollowupsService)
        {
            _permissionService = permissionService;
            _connectionRequestService = connectionRequestService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _dateTimeHelper = dateTimeHelper;
            _connectionRequestExportService = connectionRequestExportService;
            _connectionRequestImportService = connectionRequestImportService;
            _linkedInFollowupsService = linkedInFollowupsService;
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

        public void PrepareFollowUpModel(ConnectionRequestModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Status = Enum.GetValues(typeof(ConnectionRequestStatusEnum))
                .Cast<ConnectionRequestStatusEnum>()
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
            var all = await _connectionRequestService.GetAllConnectionRequestAsync(
                "", "", "", "", "", null, 0, int.MaxValue, true, null);

            // Group by StatusId to count how many per status
            var summary = all
                .GroupBy(x => x.StatusId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Build the final list including all enum values
            var result = Enum.GetValues(typeof(ConnectionRequestStatusEnum))
                .Cast<ConnectionRequestStatusEnum>()
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
        private string GetColorForStatus(ConnectionRequestStatusEnum status)
        {
            return status switch
            {
                ConnectionRequestStatusEnum.RequestSent => "#5bc0de",
                ConnectionRequestStatusEnum.Accepted => "#5cb85c",
                ConnectionRequestStatusEnum.Pending => "#f0ad4e",
                ConnectionRequestStatusEnum.Ignored => "#d9534f",
                ConnectionRequestStatusEnum.Withdrawn => "#28a745",
                ConnectionRequestStatusEnum.NotInterested => "#6c757d",
                ConnectionRequestStatusEnum.BlockedOrRemoved => "#343a40",
                _ => "#777"
            };
        }
        public virtual async Task<ConnectionRequestSearchModel> PrepareConnectionRequestSearchModelAsync(ConnectionRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.Status = Enum.GetValues(typeof(ConnectionRequestStatusEnum))
        .Cast<ConnectionRequestStatusEnum>()
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

        public virtual async Task<ConnectionRequestListModel> PrepareConnectionRequestListModelAsync(ConnectionRequestSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get connectionRequest
            var connectionRequest = await _connectionRequestService.GetAllConnectionRequestAsync(showHidden: true,
                firstname: searchModel.SearchFirstName,
                lastname: searchModel.SearchLastName,
                email: searchModel.SearchEmail,
                linkedinUrl: searchModel.SearchLinkedinUrl,
                website: searchModel.SearchWebsiteUrl,
                statusId: searchModel.SearchStatus,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new ConnectionRequestListModel().PrepareToGridAsync(searchModel, connectionRequest, () =>
            {
                //fill in model values from the entity
                return connectionRequest.SelectAwait(async connectionRequest =>
                {
                    var connectionRequestModel = new ConnectionRequestModel();
                    var selectedAvailableOption = connectionRequest.StatusId;
                    connectionRequestModel.Id = connectionRequest.Id;
                    connectionRequestModel.FirstName = connectionRequest.FirstName;
                    connectionRequestModel.LastName = connectionRequest.LastName;
                    connectionRequestModel.LinkedinUrl = connectionRequest.LinkedinUrl;
                    connectionRequestModel.Email = connectionRequest.Email;
                    connectionRequestModel.WebsiteUrl = connectionRequest.WebsiteUrl;
                    connectionRequestModel.StatusId = connectionRequest.StatusId;
                    connectionRequestModel.StatusText = ((ConnectionRequestStatusEnum)connectionRequest.StatusId).ToString();
                    if (selectedAvailableOption != 0 || selectedAvailableOption != null) connectionRequestModel.StatusId
                        = (int)((ConnectionRequestStatusEnum)selectedAvailableOption);
                    connectionRequestModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(connectionRequest.CreatedOnUtc, DateTimeKind.Utc);
                    connectionRequestModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(connectionRequest.UpdatedOnUtc, DateTimeKind.Utc);

                    return connectionRequestModel;
                });
            });
            return model;
        }

        public virtual async Task<ConnectionRequestModel> PrepareConnectionRequestModelAsync(ConnectionRequestModel model, ConnectionRequest connectionRequest, bool excludeProperties = false)
        {
            var statusList = await ConnectionRequestStatusEnum.None.ToSelectListAsync();
            if (connectionRequest != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new ConnectionRequestModel();
                    model.Id = connectionRequest.Id;
                    model.FirstName = connectionRequest.FirstName;
                    model.LastName = connectionRequest.LastName;
                    model.LinkedinUrl = connectionRequest.LinkedinUrl;
                    model.Email = connectionRequest.Email;
                    model.WebsiteUrl = connectionRequest.WebsiteUrl;
                    model.StatusId = connectionRequest.StatusId;
                    model.CreatedOnUtc = connectionRequest.CreatedOnUtc;
                    model.UpdatedOnUtc = connectionRequest.UpdatedOnUtc;
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
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareConnectionRequestSearchModelAsync(new ConnectionRequestSearchModel());

            return View("~/Plugins/Misc.SatyanamCRM/Views/ConnectionRequests/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ConnectionRequestSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await PrepareConnectionRequestListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await PrepareConnectionRequestModelAsync(new ConnectionRequestModel(), null);

            return View("~/Plugins/Misc.SatyanamCRM/Views/ConnectionRequests/Create.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ConnectionRequestModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Add))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(model.FirstName))
                ModelState.AddModelError(nameof(model.FirstName), "Enter a first name");

            if (string.IsNullOrWhiteSpace(model.LastName))
                ModelState.AddModelError(nameof(model.LastName), "Enter a last name");

            if (model.StatusId == 0)
                model.StatusId = (int)FollowUpStatusEnum.None;
            if (!ModelState.IsValid)
            {
                model = await PrepareConnectionRequestModelAsync(model, null, true);
                return View("~/Plugins/Misc.SatyanamCRM/Views/ConnectionRequests/Create.cshtml", model);
            }
            //  Save to database
            var connectionRequest = new ConnectionRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                LinkedinUrl = model.LinkedinUrl,
                Email = model.Email,
                WebsiteUrl = model.WebsiteUrl,
                StatusId = model.StatusId,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };

            await _connectionRequestService.InsertConnectionRequestAsync(connectionRequest);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.ConnectionRequests.Added"));

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = connectionRequest.Id });
        }


        [HttpPost]

        public virtual async Task<IActionResult> InlineEdit(ConnectionRequestModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Edit))
                return AccessDeniedView();

            var connectionRequest = await _connectionRequestService.GetConnectionRequestByIdAsync(model.Id);
            if (connectionRequest == null)
                return Json(new { error = "Record not found" });
            if (model.StatusId == 0)
                model.StatusId = connectionRequest.StatusId;

            //  Update database entity
            connectionRequest.FirstName = model.FirstName;
            connectionRequest.LastName = model.LastName;
            connectionRequest.Email = model.Email;
            connectionRequest.LinkedinUrl = model.LinkedinUrl;
            connectionRequest.WebsiteUrl = model.WebsiteUrl;
            connectionRequest.StatusId = model.StatusId;
            connectionRequest.UpdatedOnUtc = DateTime.UtcNow;

            await _connectionRequestService.UpdateConnectionRequestAsync(connectionRequest);

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
                    StatusId = model.StatusId,
                    UpdatedOnUtc = DateTime.UtcNow.ToString("yyyy-MM-dd")
                }
            });
        }
        [HttpGet]
        public async Task<IActionResult> ChangeStatus(int id, string btnId, string formId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Edit))
                return AccessDeniedView();

            // Get the follow-up record
            var followUp = await _connectionRequestService.GetConnectionRequestByIdAsync(id);
            if (followUp == null)
                return RedirectToAction("List");

            // Prepare the model
            var model = new ConnectionRequestModel
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
            return View("~/Plugins/Misc.SatyanamCRM/Views/ConnectionRequests/StatusChange.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatus(ConnectionRequestModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Edit))
                return AccessDeniedView();

            var entity = await _connectionRequestService.GetConnectionRequestByIdAsync(model.Id);
            if (entity == null)
                return RedirectToAction("List");

            entity.StatusId = model.StatusId;
            await _connectionRequestService.UpdateConnectionRequestAsync(entity);

            ViewBag.RefreshPage = true; // important
            return View("~/Plugins/Misc.SatyanamCRM/Views/ConnectionRequests/StatusChange.cshtml", model);
        }


        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _connectionRequestService.GetConnectionRequestByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _connectionRequestService.DeleteConnectionRequestAsync(item);
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveSelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Edit))
                return AccessDeniedView();

            if (string.IsNullOrWhiteSpace(selectedIds))
                return Json(new { success = false, message = "No records selected." });

            var idList = selectedIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => int.TryParse(id, out var parsed) ? parsed : 0)
                .Where(id => id > 0)
                .ToList();

            if (!idList.Any())
                return Json(new { success = false, message = "No valid record IDs found." });

            try
            {
                int movedCount = 0;
                int updatedCount = 0;

                // 2️⃣ Fetch selected connection requests
                var connectionRequests = await _connectionRequestService.GetConnectionRequestsByIdsAsync(idList);
                if (connectionRequests == null || !connectionRequests.Any())
                    return Json(new { success = false, message = "No matching connection requests found." });

                foreach (var req in connectionRequests)
                {
                    // 3️⃣ Check if a matching follow-up already exists
                    var existingFollowup = await _linkedInFollowupsService.GetLinkedInFollowupByLinkedinUrlOrEmailAsync(
                        req.LinkedinUrl,
                        req.Email
                    );

                    if (existingFollowup != null)
                    {
                        // 4️⃣ Update existing record
                        existingFollowup.FirstName = req.FirstName;
                        existingFollowup.LastName = req.LastName;
                        existingFollowup.WebsiteUrl = req.WebsiteUrl;
                        existingFollowup.StatusId = req.StatusId;
                        existingFollowup.UpdatedOnUtc = DateTime.UtcNow;

                        await _linkedInFollowupsService.UpdateLinkedInFollowupsAsync(existingFollowup);
                        updatedCount++;
                    }
                    else
                    {
                        // 5️⃣ Create new follow-up record
                        var newFollowup = new LinkedInFollowups
                        {
                            LinkedinUrl = req.LinkedinUrl,
                            WebsiteUrl = req.WebsiteUrl,
                            FirstName = req.FirstName,
                            LastName = req.LastName,
                            Email = req.Email,
                            StatusId = req.StatusId,
                            CreatedOnUtc = DateTime.UtcNow,
                            UpdatedOnUtc = DateTime.UtcNow
                        };

                        await _linkedInFollowupsService.InsertLinkedInFollowupsAsync(newFollowup);
                        movedCount++;
                    }

                    // 6️ Optional: mark as moved
                    // req.IsMovedToFollowups = true;
                     await _connectionRequestService.DeleteConnectionRequestAsync(req);
                }
                _notificationService.SuccessNotification(
                 string.Format(await _localizationService.GetResourceAsync("Plugin.SatyanamCRM.ConnectionRequests.Moved"),movedCount, updatedCount));
                // 7️⃣ Return summary
                return Json(new
                {
                    success = true,
                    message = $"{movedCount} new record(s) added and {updatedCount} record(s) updated in LinkedIn Followups."
                });
            }
            catch (Exception ex)
            {
                //await _logger.ErrorAsync("Error moving connection requests to followups", ex);
                return Json(new { success = false, message = "An error occurred while moving the records." });
            }
        }


        #endregion

        #region Export/Import

        [HttpPost, ActionName("ExportToExcel")]
        [FormValueRequired("exportexcel-all")]
        public virtual async Task<IActionResult> ExportToExcel(ConnectionRequestModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Edit))
                return AccessDeniedView();

            // fetch all matched records (pageSize = int.MaxValue)
            var list = await _connectionRequestService.GetAllConnectionRequestAsync(
                showHidden: true,
                firstname: searchModel.FirstName,
                lastname: searchModel.LastName,
                email: searchModel.Email,
                linkedinUrl: searchModel.LinkedinUrl,
                website: searchModel.WebsiteUrl,
                statusId: searchModel.StatusId,
                pageIndex: 0,
                pageSize: int.MaxValue);

            // convert to DTOs for export
            var dtoList = new List<ConnectionRequestDto>();
            foreach (var item in list)
            {
                var dto = new ConnectionRequestDto
                {
                    Id = item.Id,
                    FirstName = item.FirstName ?? " ",
                    LastName = item.LastName ?? " ",
                    LinkedinUrl = item.LinkedinUrl ?? " ",
                    Email = item.Email ?? " ",
                    WebsiteUrl = item.WebsiteUrl ?? " ",
                    StatusId = item.StatusId,
                    StatusName = item.StatusId > 0 ? ((FollowUpStatusEnum)item.StatusId).ToString() : "None"
                };
                dtoList.Add(dto);
            }

            try
            {
                var bytes = await _connectionRequestExportService.ExportConnectionRequestToExcelAsync(dtoList);
                return File(bytes, MimeTypes.TextXlsx, "ConnectionRequest.xlsx");
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
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Edit))
                return AccessDeniedView();

            if (selectedIds == null || !selectedIds.Any())
            {
                _notificationService.WarningNotification("No records selected for export.");
                return RedirectToAction("List");
            }

            var dtoList = new List<ConnectionRequestDto>();
            try
            {
                foreach (var id in selectedIds)
                {
                    var item = await _connectionRequestService.GetConnectionRequestByIdAsync(id)
                        ?? throw new ArgumentException("No ConnectionRequest found with the specified id");

                    dtoList.Add(new ConnectionRequestDto
                    {
                        Id = item.Id,
                        FirstName = item.FirstName ?? " ",
                        LastName = item.LastName ?? " ",
                        LinkedinUrl = item.LinkedinUrl ?? " ",
                        Email = item.Email ?? " ",
                        WebsiteUrl = item.WebsiteUrl ?? " ",
                        StatusId = item.StatusId,
                        StatusName = item.StatusId > 0 ? ((FollowUpStatusEnum)item.StatusId).ToString() : "None"
                    });
                }

                var bytes = await _connectionRequestExportService.ExportConnectionRequestToExcelAsync(dtoList);
                return File(bytes, MimeTypes.TextXlsx, "ConnectionRequest_Selected.xlsx");
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
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageConnectionRequests, PermissionAction.Add))
                return AccessDeniedView();

            try
            {
                if (importFile != null && importFile.Length > 0)
                {
                    // The import service will create new records or update existing ones (match by Email or LinkedinUrl)
                    var result = await _connectionRequestImportService.ImportConnectionRequestFromExcelAsync(importFile);
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
