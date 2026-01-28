using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Localization;
using App.Core.Domain.ManageResumes;
using App.Core.Domain.Media;
using App.Core.Domain.result;
using App.Core.Infrastructure;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.ManageResumes;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Manageresumes;
using App.Web.Areas.Admin.Models.ManageResumes;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class CandiatesResumesController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ICandiatesResumesModelFactory _candiatesResumesModelFactory;
        private readonly ICandiatesResumesService _candiatesResumesService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IDownloadService _downloadService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public CandiatesResumesController(IPermissionService permissionService,
           ICandiatesResumesModelFactory candiatesResumesModelFactory,
           ICandiatesResumesService candiatesResumesService,
           INotificationService notificationService,
            ILocalizationService localizationService,
            IEmployeeService employeeService,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IDownloadService downloadService,
            INopFileProvider fileProvider,
            ILogger logger)
        {
            _permissionService = permissionService;
            _candiatesResumesModelFactory = candiatesResumesModelFactory;
            _candiatesResumesService = candiatesResumesService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _employeeService = employeeService;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _downloadService = downloadService;
            _fileProvider = fileProvider;
            _logger = logger;
        }

        #endregion

        public virtual async Task<int> AsyncUpload(IFormFile file)
        {
            var fileBinary = await _downloadService.GetDownloadBitsAsync(file);

            var qqFileNameParameter = "qqfilename";
            var fileName = file.FileName;
            if (string.IsNullOrEmpty(fileName) && Request.Form.ContainsKey(qqFileNameParameter))
                fileName = Request.Form[qqFileNameParameter].ToString();
            //remove path (passed in IE)
            fileName = _fileProvider.GetFileName(fileName);
            var contentType = file.ContentType;

            var fileExtension = _fileProvider.GetFileExtension(fileName);
            if (!string.IsNullOrEmpty(fileExtension))
                fileExtension = fileExtension.ToLowerInvariant();

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = string.Empty,
                DownloadBinary = fileBinary,
                ContentType = contentType,
                //we store filename without extension for downloads
                Filename = _fileProvider.GetFileNameWithoutExtension(fileName),
                Extension = fileExtension,
                IsNew = true
            };

            try
            {
                await _downloadService.InsertDownloadAsync(download);

                //when returning JSON the mime-type must be set to text/plain
                //otherwise some browsers will pop-up a "Save As" dialog.
                return download.Id;
            }
            catch (Exception exc)
            {
                await _logger.ErrorAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
            }

            return download.Id;
        }
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandiatesResumes))
                return AccessDeniedView();
            //prepare model
            var model = await _candiatesResumesModelFactory.PrepareCandiatesResumesSearchModelAsync(new CandiatesResumesSearchModel());
            return View("/Areas/Admin/Views/Extension/ManageResume/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(CandiatesResumesSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandiatesResumes))
                return AccessDeniedView();

            //prepare model
            var model = await _candiatesResumesModelFactory.PrepareCandiatesResumesListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandiatesResumes))
                return AccessDeniedView();

            //prepare model
            var model = await _candiatesResumesModelFactory.PrepareCandiatesResumesModelAsync(new CandiatesResumesModel(), null);

            return View("/Areas/Admin/Views/Extension/ManageResume/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CandiatesResumesModel model, bool continueEditing, IFormFile file, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandiatesResumes))
                return AccessDeniedView();

            var timeSheet = model.ToEntity<CandidatesResumes>();

            if (ModelState.IsValid)
            {
                timeSheet.MobileNo = model.MobileNumber;
                if (file != null)
                {
                    timeSheet.DownloadId = await AsyncUpload(file);
                }
                await _candiatesResumesService.InsertCandiatesResumesAsync(timeSheet);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Candidates.Added"));

                var employeeName = "";
                var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);

                var currentCustomerRoleIds = await _candiatesResumesService.GetEmployeeIdsAsync(timeSheet, true);
                //var newCustomerRoles = new List<Employee>();
                foreach (var customerRole in employee)
                {
                    if (model.SelectedInterviewer.Contains(customerRole.Id))
                    {
                        if (currentCustomerRoleIds.All(roleId => roleId != customerRole.Id))
                            await _candiatesResumesService.AddCustomerRoleMappingAsync(new CandidateInterviewerMapping { CandidatesId = timeSheet.Id, EmployeeId = customerRole.Id });
                        timeSheet.EmployeeId = customerRole.Id;
                        await _workflowMessageService.SendInterviewerMessageAsync(timeSheet, _localizationSettings.DefaultAdminLanguageId);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Interviewer.InterviewerMailSend"));
                    }
                    else
                    {
                        //remove Interwiewer
                        if (currentCustomerRoleIds.Any(roleId => roleId == customerRole.Id))
                            await _candiatesResumesService.RemoveInterviewerMappingAsync(timeSheet, customerRole);
                    }
                }
                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }

            //prepare model
            model = await _candiatesResumesModelFactory.PrepareCandiatesResumesModelAsync(model, timeSheet, true);



            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/ManageResume/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCandiatesResumes))
                return AccessDeniedView();

            var candiatesresumes = await _candiatesResumesService.GetCandiatesResumesByIdAsync(id);
            if (candiatesresumes == null)
                return RedirectToAction("List");
            //prepare model
            var model = await _candiatesResumesModelFactory.PrepareCandiatesResumesModelAsync(null, candiatesresumes);
            return View("/Areas/Admin/Views/Extension/ManageResume/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CandiatesResumesModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting))
                return AccessDeniedView();

            //try to get a project with the specified id
            var candiatesresume = await _candiatesResumesService.GetCandiatesResumesByIdAsync(model.Id);
            if (candiatesresume == null)
                return RedirectToAction("List");
            //validate Emlpoyee 
            var employeeName = "";
            var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            var newCustomerRoles = new List<Employee>();
            foreach (var customerRole in employee)
                if (model.SelectedInterviewer.Contains(customerRole.Id))
                    newCustomerRoles.Add(customerRole);
            if (ModelState.IsValid)
            {
                candiatesresume = model.ToEntity(candiatesresume);
                candiatesresume.MobileNo = model.MobileNumber;
                await _candiatesResumesService.UpdateCandiatesResumesAsync(candiatesresume);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Candidates.Updated"));

                var currentCustomerRoleIds = await _candiatesResumesService.GetEmployeeIdsAsync(candiatesresume, true);

                foreach (var customerRole in employee)
                {
                    if (model.SelectedInterviewer.Contains(customerRole.Id))
                    {
                        //Add Interwiewer
                        if (currentCustomerRoleIds.All(roleId => roleId != customerRole.Id))
                            await _candiatesResumesService.AddCustomerRoleMappingAsync(new CandidateInterviewerMapping { CandidatesId = candiatesresume.Id, EmployeeId = customerRole.Id });
                    }
                    else
                    {
                        //remove Interwiewer
                        if (currentCustomerRoleIds.Any(roleId => roleId == customerRole.Id))
                            await _candiatesResumesService.RemoveInterviewerMappingAsync(candiatesresume, customerRole);
                    }
                }

                if (!continueEditing)
                    return RedirectToAction("List");


                return RedirectToAction("Edit", new { id = candiatesresume.Id });
            }

            foreach (var p in employee)
            {
                model.Employee.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString(),
                    Selected = model.SelectedInterviewer.Contains(p.Id)
                });
            }
            model = await _candiatesResumesModelFactory.PrepareResultModelAsync(model.Id);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/ManageResume/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var candidatesResumes = await _candiatesResumesService.GetCandiatesResumesByIdAsync(id);
            if (candidatesResumes == null)
                return RedirectToAction("List");

            await _candiatesResumesService.DeleteCandiatesResumesAsync(candidatesResumes);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Candidates.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageJobPosting))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _candiatesResumesService.GetCandiatesResumesByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _candiatesResumesService.DeleteCandiatesResumesAsync(item);
            }
            return Json(new { Result = true });
        }
        public async Task<IActionResult> TakeInterview(CandiatesResumesModel model, int Id)
        {
            var trainee = await _candiatesResumesService.GetCandiatesResumesByIdAsync(Id);
            var typeId = 1;
            var questions = _candiatesResumesService.GetAllQuestions(typeId);

            if (trainee != null)
            {
                model.CandidateName = trainee.FirstName + " " + trainee.LastName;

                model.Interviewer = string.Join(", ",
                  (await _candiatesResumesService.GetCustomerRoleBySystemNameAsync(trainee)).Select(role => role.FirstName + role.LastName));
            }

            model = await _candiatesResumesModelFactory.PrepareCandiatesResumesModelAsync(model, trainee, true);
            model.Id = Id;

            return View("~/Areas/Admin/Views/Extension/ManageResume/TakeInterviewDatail.cshtml", model);

        }

        [HttpPost]
        public async Task<IActionResult> ResultCreate(CandiatesResumesModel model)
        {
            var timeSheet = model.ToEntity<CandidatesResult>();
            var typeId = 1;
            var questions = _candiatesResumesService.GetAllQuestions(typeId);
            timeSheet.ResultData = JsonConvert.SerializeObject(questions);
            timeSheet.CandidateId = model.Id;
            await _candiatesResumesService.InsertCandiatesResultAsync(timeSheet);
            await _workflowMessageService.SendInterviewertoHrMessageAsync(timeSheet, _localizationSettings.DefaultAdminLanguageId);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Interviewer.SendToHr"));
            if (timeSheet == null)
                return RedirectToAction("Edit");
            model = await _candiatesResumesModelFactory.PrepareCandiatesResultModelAsync(model, timeSheet, true);
            return View("/Areas/Admin/Views/Extension/ManageResume/Edit.cshtml", model);

        }

    }
}