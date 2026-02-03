using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Core.Domain.Messages;
using App.Core.Domain.Security;
using App.Core.Events;
using App.Services.Configuration;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Projects;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.Announcements;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class AnnouncementController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IAnnouncementModelFactory _announcementModelFactory;
        private readonly IAnnouncementService _announcementService;
        private readonly IEmployeeService _employeeService;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IProjectsService _projectsService;
        private readonly IDesignationService _designationService;
        private readonly IWorkflowMessagePluginService _workflowMessagePluginService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IEmailSender _emailSender;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ISettingService _settingService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITokenizer _tokenizer;
        #endregion

        #region Ctor

        public AnnouncementController(
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IAnnouncementModelFactory announcementModelFactory,
            IAnnouncementService announcementService,
            IEmployeeService employeeService,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IProjectsService projectsService,
            IDesignationService designationService,
            IWorkflowMessagePluginService workflowMessagePluginService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IEmailSender emailSender,
            IEmailAccountService emailAccountService,
            ISettingService settingService,
            IWebHostEnvironment hostingEnvironment,
            IMessageTemplateService messageTemplateService,
            IEventPublisher eventPublisher,
            ITokenizer tokenizer)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _announcementModelFactory = announcementModelFactory;
            _announcementService = announcementService;
            _employeeService = employeeService;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _projectsService = projectsService;
            _designationService = designationService;
            _workflowMessagePluginService = workflowMessagePluginService;
            _workContext = workContext;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _emailSender = emailSender;
            _emailAccountService = emailAccountService;
            _settingService = settingService;
            _hostingEnvironment = hostingEnvironment;
            _messageTemplateService = messageTemplateService;
            _eventPublisher = eventPublisher;
            _tokenizer = tokenizer;
        }

        #endregion

        #region List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _announcementModelFactory.PrepareAnnouncementSearchModelAsync(new AnnouncementSearchModel());

            return View("/Areas/Admin/Views/Extension/Announcements/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(AnnouncementSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _announcementModelFactory.PrepareAnnouncementListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region Create

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _announcementModelFactory.PrepareAnnouncementModelAsync(new AnnouncementModel(), null);

            return View("/Areas/Admin/Views/Extension/Announcements/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(AnnouncementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.Add))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var announcement = model.ToEntity<Announcement>();
                announcement.CreatedOnUtc = DateTime.UtcNow;

                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                //convert from IST to UTC
                if (announcement.ScheduledOnUtc != null)
                {
                    announcement.ScheduledOnUtc = TimeZoneInfo.ConvertTimeToUtc(announcement.ScheduledOnUtc.Value, istTimeZone);
                }


                announcement.SendEmployeeIds = (model.SendEmployeeIdList != null && model.SendEmployeeIdList.Any())
    ? string.Join(",", model.SendEmployeeIdList)
    : string.Empty;

                announcement.ReferenceIds = (model.ReferenceIdList != null && model.ReferenceIdList.Any())
                    ? string.Join(",", model.ReferenceIdList)
                    : string.Empty;



                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    var fileName = Path.GetFileName(model.AttachmentFile.FileName);
                    var uploadsDir = Path.Combine(_hostingEnvironment.WebRootPath, "attachments");

                    if (!Directory.Exists(uploadsDir))
                        Directory.CreateDirectory(uploadsDir);

                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }

                    announcement.AttachmentPath = "/attachments/" + fileName;
                }

                await _announcementService.InsertAnnouncementAsync(announcement);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Announcements.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = announcement.Id });
            }

            //prepare model again
            model = await _announcementModelFactory.PrepareAnnouncementModelAsync(model, null, true);

            return View("/Areas/Admin/Views/Extension/Announcements/Create.cshtml", model);
        }

        #endregion

        #region Edit

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.Edit))
                return AccessDeniedView();

            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
                return RedirectToAction("List");

            var model = await _announcementModelFactory.PrepareAnnouncementModelAsync(null, announcement);

            return View("/Areas/Admin/Views/Extension/Announcements/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(AnnouncementModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.Edit))
                return AccessDeniedView();

            var announcement = await _announcementService.GetAnnouncementByIdAsync(model.Id);
            if (announcement == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                announcement = model.ToEntity(announcement);

                var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

                //convert from IST to UTC
                if (announcement.ScheduledOnUtc != null)
                {
                    announcement.ScheduledOnUtc = TimeZoneInfo.ConvertTimeToUtc(announcement.ScheduledOnUtc.Value, istTimeZone);
                }



                announcement.SendEmployeeIds = (model.SendEmployeeIdList != null && model.SendEmployeeIdList.Any())
 ? string.Join(",", model.SendEmployeeIdList)
 : string.Empty;

                announcement.ReferenceIds = (model.ReferenceIdList != null && model.ReferenceIdList.Any())
                    ? string.Join(",", model.ReferenceIdList)
                    : string.Empty;

                await _announcementService.UpdateAnnouncementAsync(announcement);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Announcements.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = announcement.Id });
            }

            model = await _announcementModelFactory.PrepareAnnouncementModelAsync(model, announcement, true);

            return View("/Areas/Admin/Views/Extension/Announcements/Edit.cshtml", model);
        }

        #endregion

        #region Delete

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.Delete))
                return AccessDeniedView();

            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
                return RedirectToAction("List");

            await _announcementService.DeleteAnnouncementAsync(announcement);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Announcements.Deleted"));

            return RedirectToAction("List");
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var announcements = await _announcementService.GetAnnouncementsByIdsAsync(selectedIds.ToArray());
            foreach (var ann in announcements)
            {
                await _announcementService.DeleteAnnouncementAsync(ann);
            }

              return Json(new { Result = true });
        }
        #region Ajax Employee Selection

        public virtual async Task<IActionResult> GetEmployeesByProjects(string projectIds)
        {
            if (string.IsNullOrEmpty(projectIds))
                return Json(new List<object>());

            var ids = projectIds.Split(',').Select(int.Parse).ToArray();

            // get employees by project(s)
            var employees = await _projectEmployeeMappingService.GetEmployeesByProjectIdsAsync(ids);

            var result = employees.Select(e => new
            {
                Value = e.Id,
                Text = e.FirstName + " " + e.LastName
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetEmployeesByDesignations(string designationIds)
        {
            if (string.IsNullOrEmpty(designationIds))
                return Json(new List<object>());

            var ids = designationIds.Split(',').Select(int.Parse).ToArray();

            var employees = await _employeeService.GetEmployeesByDesignationIdsAsync(ids);

            var result = employees.Select(e => new
            {
                Value = e.Id,
                Text = e.FirstName + " " + e.LastName
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetEmployeesByIds(string employeeIds)
        {
            if (string.IsNullOrEmpty(employeeIds))
                return Json(new List<object>());

            var ids = employeeIds.Split(',').Select(int.Parse).ToArray();

            var employees = await _employeeService.GetEmployeesByIdsAsync(ids);

            var result = employees.Select(e => new
            {
                Value = e.Id,
                Text = e.FirstName + " " + e.LastName
            }).ToList();

            return Json(result);
        }

        //[HttpGet]
        //public virtual async Task<IActionResult> GetAllEmployees()
        //{
        //    var employees = await _employeeService.GetAllEmployeesAsync();

        //    var result = employees.Select(e => new
        //    {
        //        Id = e.Id,
        //        Name = e.FirstName + " " + e.LastName
        //    }).ToList();

        //    return Json(result);
        //}

        #endregion

        #region Reference Dropdowns

        [HttpGet]
        public virtual async Task<IActionResult> GetAllProjects()
        {
            var projects = await _projectsService.GetAllProjectsAsync("");
            var result = projects.Select(p => new
            {
                Value = p.Id,
                Text = p.ProjectTitle
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllDesignations()
        {
            var designations = await _designationService.GetAllDesignationAsync("");
            var result = designations.Select(d => new
            {
                Value = d.Id,
                Text = d.Name
            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public virtual async Task<IActionResult> GetAllEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            var result = employees.Select(e => new
            {
                Value = e.Id,
                Text = e.FirstName + " " + e.LastName
            }).ToList();

            return Json(result);
        }

        #endregion


        [HttpPost]
        public virtual async Task<IActionResult> SendAnnouncement(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements))
                return Json(new { success = false, message = "Access denied" });

            var announcement = await _announcementService.GetAnnouncementByIdAsync(id);
            if (announcement == null)
                return Json(new { success = false, message = "Announcement not found" });

            var employeeIds = !string.IsNullOrEmpty(announcement.SendEmployeeIds)
                ? announcement.SendEmployeeIds.Split(',').Select(int.Parse).ToList()
                : new List<int>();

            if (!employeeIds.Any())
                return Json(new { success = false, message = "No employees selected for this announcement." });

            await _workflowMessagePluginService.SendAnnouncementMessageAsync(
                announcement,
                (await _workContext.GetWorkingLanguageAsync()).Id,
                employeeIds);

            announcement.IsSent = true;
            await _announcementService.UpdateAnnouncementAsync(announcement);

            return Json(new { success = true, message = "Announcement emails queued successfully." });
        }


        [HttpPost]
        public virtual async Task<IActionResult> SendTestAnnouncementEmail(AnnouncementModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAnnouncements))
                return AccessDeniedView();

            var announcement = await _announcementService.GetAnnouncementByIdAsync(model.Id);
            if (announcement == null)
                return RedirectToAction("List");

            if (!CommonHelper.IsValidEmail(model.SendTestEmailTo))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.WrongEmail"));
                return RedirectToAction("Edit", new { id = model.Id });
            }

            try
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var languageId = (await _workContext.GetWorkingLanguageAsync()).Id;

                // get message templates for announcement
                var messageTemplates = await _messageTemplateService.GetMessageTemplatesByNameAsync(
                    MessageTemplateSystemNames.AnnouncementEmail, store.Id);

                if (!messageTemplates.Any())
                {
                    _notificationService.ErrorNotification("Announcement email template not found.");
                    return RedirectToAction("Edit", new { id = announcement.Id });
                }

                foreach (var messageTemplate in messageTemplates)
                {
                    var emailAccount = await _workflowMessagePluginService.GetEmailAccountOfMessageTemplateAsync(messageTemplate, languageId);
                    if (emailAccount == null)
                    {
                        _notificationService.ErrorNotification("No email account configured.");
                        return RedirectToAction("Edit", new { id = announcement.Id });
                    }

                    // build tokens
                    var tokens = new List<Token>
            {
                new Token("Announcement.Title", announcement.Title),
                new Token("Announcement.Body", announcement.Message, true)
            };

                    // Like URL for test (dummy employeeId=0)
                    var likeUrl = $"{_webHelper.GetStoreLocation()}Employee/LikeAnnouncement?id={announcement.Id}&employeeId=0";
                    tokens.Add(new Token("Announcement.LikeUrl", likeUrl, true));

                    // fire event so plugins can add tokens
                    await _eventPublisher.MessageTokensAddedAsync(messageTemplate, tokens);

                    var subject = _tokenizer.Replace(
     await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Subject, languageId),
     tokens,
     true
 );

                    var body = _tokenizer.Replace(
                        await _localizationService.GetLocalizedAsync(messageTemplate, mt => mt.Body, languageId),
                        tokens,
                        true
                    );

                    await _emailSender.SendEmailAsync(
                        emailAccount,
                        subject,
                        body,
                        emailAccount.Email,
                        emailAccount.DisplayName,
                        model.SendTestEmailTo,
                        "Test User"
                    );

                }

                _notificationService.SuccessNotification("Test email sent successfully.");
                return RedirectToAction("Edit", new { id = model.Id });
            }
            catch (Exception ex)
            {
                await _notificationService.ErrorNotificationAsync(ex);
                return RedirectToAction("Edit", new { id = model.Id });
            }
        }


        #endregion
    }
}
