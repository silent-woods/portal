using App.Core;
using App.Data;
using App.Data.Extensions;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Models.Extension.UpdateTemplate;
using App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion;
using App.Web.Framework.Models.Extensions;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class UpdateTemplateController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IUpdateTemplateModelFactory _updateTemplateModelFactory;
        private readonly IUpdateTemplateService _updateTemplateService;
        private readonly IWorkContext _workContext;
        private readonly IEmployeeService _employeeService;
        private readonly IUpdateTemplateQuestionModelFactory _updateTemplateQuestionModelFactory;
        private readonly IUpdateTemplateQuestionService _updateTemplateQuestionService;
        private readonly IUpdateQuestionOptionService _updateQuestionOptionService;
        private readonly IRepository<UpdateSubmission> _submissionRepository;
        private readonly IRepository<UpdateSubmissionAnswer> _submissionAnswerRepository;
        private readonly IRepository<UpdateSubmissionComment> _submissionCommentRepository;
        private readonly IRepository<UpdateSubmissionReviewer> _submissionReviewerRepository;
        private readonly IRepository<UpdateTemplateQuestion> _questionRepository;
        private readonly IRepository<UpdateQuestionOption> _questionOptionRepository;
        private readonly IRepository<UpdateTemplatePeriod> _templatePeriodRepository;

        #endregion

        #region Ctor

        public UpdateTemplateController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService
,
            IUpdateTemplateModelFactory updateTemplateModelFactory,
            IUpdateTemplateService updateTemplateService,
            IWorkContext workContext,
            IEmployeeService employeeService,
            IUpdateTemplateQuestionModelFactory updateTemplateQuestionModelFactory,
            IUpdateTemplateQuestionService updateTemplateQuestionService,
            IUpdateQuestionOptionService updateQuestionOptionService,
            IRepository<UpdateSubmission> submissionRepository,
            IRepository<UpdateSubmissionAnswer> submissionAnswerRepository,
            IRepository<UpdateSubmissionComment> submissionCommentRepository,
            IRepository<UpdateSubmissionReviewer> submissionReviewerRepository,
            IRepository<UpdateTemplateQuestion> questionRepository,
            IRepository<UpdateQuestionOption> questionOptionRepository,
            IRepository<UpdateTemplatePeriod> templatePeriodRepository)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _updateTemplateModelFactory = updateTemplateModelFactory;
            _updateTemplateService = updateTemplateService;
            _workContext = workContext;
            _employeeService = employeeService;
            _updateTemplateQuestionModelFactory = updateTemplateQuestionModelFactory;
            _updateTemplateQuestionService = updateTemplateQuestionService;
            _updateQuestionOptionService = updateQuestionOptionService;
            _submissionRepository = submissionRepository;
            _submissionAnswerRepository = submissionAnswerRepository;
            _submissionCommentRepository = submissionCommentRepository;
            _submissionReviewerRepository = submissionReviewerRepository;
            _questionRepository = questionRepository;
            _questionOptionRepository = questionOptionRepository;
            _templatePeriodRepository = templatePeriodRepository;
        }

        #endregion

        #region Utilities
        public virtual async Task<UpdateQuestionOptionListModel> PrepareQuestionOptionListModelAsync(UpdateQuestionOptionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var deals = await _updateQuestionOptionService.GetOptionByQuestionIdAsync(
                UpdateTemplateQuestionId: searchModel.UpdateTemplateQuestionId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize
            );

            var model = await ModelExtensions.PrepareToGridAsync<UpdateQuestionOptionListModel, UpdateQuestionOptionModel, UpdateQuestionOption>(
                new UpdateQuestionOptionListModel(),
                searchModel,
                deals,
                () => deals.Select(d => new UpdateQuestionOptionModel
                {
                    Id = d.Id,
                    Name = d.Name,
                    DisplayOrder = d.DisplayOrder,
                    IsPreSelected = d.IsPreSelected,
                    IsRequired = d.IsRequired,
                    UpdateTemplateQuestionId = d.UpdateTemplateQuestionId
                }).ToAsyncEnumerable()
            );

            return model;
        }
        public virtual async Task<UpdateQuestionOptionModel> PrepareQuestionOptionModelAsync(UpdateQuestionOptionModel model, UpdateQuestionOption updateQuestionOption, bool excludeProperties = false)
        {
            if (updateQuestionOption != null)
            {
                if (model == null)
                {
                    model = new UpdateQuestionOptionModel();
                    model.Id = updateQuestionOption.Id;
                    model.Name = updateQuestionOption.Name;
                    model.DisplayOrder = updateQuestionOption.DisplayOrder;
                    model.IsPreSelected = updateQuestionOption.IsPreSelected;
                    model.IsRequired = updateQuestionOption.IsRequired;
                    model.UpdateTemplateQuestionId = updateQuestionOption.UpdateTemplateQuestionId;
                }

            }
            if (model != null && model.UpdateTemplateQuestionId > 0)
            {
                var question = await _updateTemplateQuestionService
                    .GetByIdAsync(model.UpdateTemplateQuestionId);

                if (question != null)
                    model.ControlType = question.ControlTypeId;
            }
            return model;
        }

        private async Task<bool> HasAnotherPreSelectedAsync(int questionId,int? excludeOptionId = null)
        {
            var query = (await _updateQuestionOptionService.GetByQuestionIdAsync(questionId))
                .Where(x => x.IsPreSelected);

            if (excludeOptionId.HasValue)
                query = query.Where(x => x.Id != excludeOptionId.Value);

            return query.Any();
        }
        private bool IsSingleSelect(int controlTypeId)
        {
            return controlTypeId == (int)ControlTypeEnum.DropdownList
                || controlTypeId == (int)ControlTypeEnum.RadioList;
        }

        private async Task<int> CopyTemplateUtilityAsync(int templateId)
        {
            var original = await _updateTemplateService.GetByIdAsync(templateId);
            if (original == null)
                return 0;

            // 1️ Create new template
            var newTemplate = new UpdateTemplate
            {
                Title = original.Title+" (Copy)",
                Description = original.Description,
                FrequencyId = original.FrequencyId,
                RepeatEvery = original.RepeatEvery,
                RepeatType = original.RepeatType,
                SelectedWeekDays = original.SelectedWeekDays,
                OnDay = original.OnDay,
                DueDate = original.DueDate,
                DueTime = original.DueTime,
                ReminderBeforeMinutes = original.ReminderBeforeMinutes,
                SubmitterUserIds = original.SubmitterUserIds,
                ViewerUserIds = original.ViewerUserIds,
                IsFileAttachmentRequired = original.IsFileAttachmentRequired,
                IsEditingAllowed = original.IsEditingAllowed,
                CreatedByUserId = original.CreatedByUserId,
                IsActive = original.IsActive,
                CreatedOnUTC = DateTime.UtcNow
            };

            await _updateTemplateService.InsertAsync(newTemplate);

            // 2️ Copy questions
            var questions = await _questionRepository.GetAllAsync(q =>
                q.Where(x => x.UpdateTemplateId == templateId));

            foreach (var question in questions)
            {
                var newQuestion = new UpdateTemplateQuestion
                {
                    UpdateTemplateId = newTemplate.Id,
                    QuestionText = question.QuestionText,
                    IsRequired = question.IsRequired,
                    ControlTypeId = question.ControlTypeId,
                    DisplayOrder = question.DisplayOrder,
                    ValidationMinLength = question.ValidationMinLength,
                    ValidationMaxLength = question.ValidationMaxLength,
                    ValidationFileMaximumSize = question.ValidationFileMaximumSize,
                    ValidationFileAllowedExtensions = question.ValidationFileAllowedExtensions,
                    DefaultValue = question.DefaultValue
                };

                await _questionRepository.InsertAsync(newQuestion);

                // 3️ Copy options
                var options = await _questionOptionRepository.GetAllAsync(o =>
                    o.Where(x => x.UpdateTemplateQuestionId == question.Id));

                foreach (var option in options)
                {
                    var newOption = new UpdateQuestionOption
                    {
                        UpdateTemplateQuestionId = newQuestion.Id,
                        Name = option.Name,
                        DisplayOrder = option.DisplayOrder,
                        IsPreSelected = option.IsPreSelected,
                        IsRequired = option.IsRequired
                    };

                    await _questionOptionRepository.InsertAsync(newOption);
                }
            }
            return newTemplate.Id;
        }


        #endregion

        #region Update Tamplate List/Create/Edit/Delete
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _updateTemplateModelFactory.PrepareUpdateTempleteSearchModelAsync(new UpdateTemplateSearchModel());

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(UpdateTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _updateTemplateModelFactory.PrepareUpdateTemplateListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _updateTemplateModelFactory.PrepareUpdateTemplateModelAsync(new UpdateTemplateModel(), null);

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/Create.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(UpdateTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Add))
                return AccessDeniedView();
            // Parse hidden input manually
            var submitterString = Request.Form["SelectedSubmitterIds"].ToString();
            model.SelectedSubmitterIds = string.IsNullOrWhiteSpace(submitterString)
                ? new List<int>()
                : submitterString.Split(',').Select(int.Parse).ToList();

            var viewerString = Request.Form["SelectedViewerIds"].ToString();
            model.SelectedViewerIds = string.IsNullOrWhiteSpace(viewerString)
                ? new List<int>()
                : viewerString.Split(',').Select(int.Parse).ToList();
            if (model.FrequencyId == (int)UpdatedFrequency.Weekly && string.IsNullOrEmpty(model.SelectedWeekdays))
            {
                ModelState.AddModelError(nameof(model.SelectedWeekdays), "Please select the week days.");
            }

            if (model.FrequencyId == (int)UpdatedFrequency.OneTime &&  !model.DueDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.DueDate), "Please enter a due date.");
            }
            if (model.FrequencyId == (int)UpdatedFrequency.Daily &&
        model.RepeatType == "Week" &&
        string.IsNullOrEmpty(model.SelectedWeekdays))
            {
                ModelState.AddModelError(nameof(model.SelectedWeekdays), "Please select the week days.");
            }
            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                // Convert reminder fields (days + hours + minutes) to total minutes
                var totalReminderMinutes = model.ReminderBeforeDays * 1440 +
                                           model.ReminderBeforeHours * 60 +
                                           model.ReminderBeforeMinutesOnly;
                //TimeSpan? dueTime = null;
                //if (!string.IsNullOrEmpty(model.DueTime) &&
                //    TimeSpan.TryParseExact(model.DueTime, "hh\\:mm", CultureInfo.InvariantCulture, out var parsedTime))
                //{
                //    dueTime = parsedTime;
                //}

                var entity = new UpdateTemplate
                {
                    Title = model.Title,
                    Description = model.Description,
                    FrequencyId = model.FrequencyId,
                    RepeatEvery = model.RepeatEvery,
                    RepeatType = model.RepeatType,
                    SelectedWeekDays = model.SelectedWeekdays,
                    OnDay = model.OnDay,
                    DueDate = model.DueDate?.Date,
                    DueTime = model.DueTime,
                    SubmitterUserIds = string.Join(",", model.SelectedSubmitterIds ?? new()),
                    ViewerUserIds = string.Join(",", model.SelectedViewerIds ?? new()),
                    IsFileAttachmentRequired = model.IsFileAttachmentRequired,
                    IsEditingAllowed = model.IsEditingAllowed,
                    ReminderBeforeMinutes = totalReminderMinutes,
                    IsActive = model.IsActive,
                    CreatedOnUTC = DateTime.UtcNow,
                    CreatedByUserId = employee?.Id ?? 0
                };


                await _updateTemplateService.InsertAsync(entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplate.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = entity.Id });
            }
            //prepare model
            model = await _updateTemplateModelFactory.PrepareUpdateTemplateModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            var activity = await _updateTemplateService.GetByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _updateTemplateModelFactory.PrepareUpdateTemplateModelAsync(null, activity);

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(UpdateTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a update template with the specified id
            var activity = await _updateTemplateService.GetByIdAsync(model.Id);
            if (activity == null)
                return RedirectToAction("List");
            var submitterString = Request.Form["SelectedSubmitterIds"].ToString();
            model.SelectedSubmitterIds = string.IsNullOrWhiteSpace(submitterString)
                ? new List<int>()
                : submitterString.Split(',').Select(int.Parse).ToList();

            var viewerString = Request.Form["SelectedViewerIds"].ToString();
            model.SelectedViewerIds = string.IsNullOrWhiteSpace(viewerString)
                ? new List<int>()
                : viewerString.Split(',').Select(int.Parse).ToList();
            if (model.FrequencyId == (int)UpdatedFrequency.Weekly && string.IsNullOrEmpty(model.SelectedWeekdays))
            {
                ModelState.AddModelError(nameof(model.SelectedWeekdays), "Please select the week days.");
            }

            if (model.FrequencyId == (int)UpdatedFrequency.OneTime && !model.DueDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.DueDate), "Please enter a due date.");
            }
            if (model.FrequencyId == (int)UpdatedFrequency.Daily &&
        model.RepeatType == "Week" && 
        string.IsNullOrEmpty(model.SelectedWeekdays))
            {
                ModelState.AddModelError(nameof(model.SelectedWeekdays), "Please select the week days.");
            }


            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                // Calculate total reminder minutes from parts
                var totalReminderMinutes = model.ReminderBeforeDays * 1440 +
                                           model.ReminderBeforeHours * 60 +
                                           model.ReminderBeforeMinutesOnly;
                //TimeSpan? dueTime = null;
                //if (!string.IsNullOrEmpty(model.DueTime) &&
                //    TimeSpan.TryParseExact(model.DueTime, "hh\\:mm", CultureInfo.InvariantCulture, out var parsedTime))
                //{
                //    dueTime = parsedTime;
                //}



                activity.Title = model.Title;
                activity.Description = model.Description;
                activity.FrequencyId = model.FrequencyId;
                activity.RepeatEvery = model.RepeatEvery;
                activity.RepeatType = model.RepeatType;
                activity.SelectedWeekDays = model.SelectedWeekdays;
                activity.OnDay = model.OnDay;
                activity.DueDate = model.DueDate?.Date;
                activity.DueTime = model.DueTime;
                activity.SubmitterUserIds = string.Join(",", model.SelectedSubmitterIds ?? new());
                activity.ViewerUserIds = string.Join(",", model.SelectedViewerIds ?? new());
                activity.IsFileAttachmentRequired = model.IsFileAttachmentRequired;
                activity.IsEditingAllowed = model.IsEditingAllowed;
                activity.ReminderBeforeMinutes = totalReminderMinutes;
                activity.IsActive = model.IsActive;
                activity.CreatedByUserId = employee?.Id ?? 0;

                await _updateTemplateService.UpdateAsync(activity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplate.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = activity.Id });
            }
            //prepare model
            model = await _updateTemplateModelFactory.PrepareUpdateTemplateModelAsync(model, null, true);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/UpdateTemplate/Edit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var activity = await _updateTemplateService.GetByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            await _updateTemplateService.DeleteAsync(activity);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplate.Deleted"));

            return RedirectToAction("List");
        }
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _updateTemplateService.GetByIdsAsync(selectedIds.ToArray());

            foreach (var template in data)
            {
                // 1. Delete submissions and their children
                var submissions = await _submissionRepository.GetAllAsync(query =>
            query.Where(s => s.UpdateTemplateId == template.Id));
                foreach (var submission in submissions)
                {
                    await _submissionAnswerRepository.DeleteAsync(x => x.UpdateSubmissionId == submission.Id);
                    await _submissionCommentRepository.DeleteAsync(x => x.UpdateSubmissionId == submission.Id);
                    await _submissionReviewerRepository.DeleteAsync(x => x.UpdateSubmissionId == submission.Id);

                    await _submissionRepository.DeleteAsync(submission);
                }

                // 2. Delete questions and their options
                var questions = await _questionRepository.GetAllAsync(query =>
            query.Where(q => q.UpdateTemplateId == template.Id));
                foreach (var question in questions)
                {
                    await _questionOptionRepository.DeleteAsync(x => x.UpdateTemplateQuestionId == question.Id);
                    await _questionRepository.DeleteAsync(question);
                }

                // 3. Delete template periods
                await _templatePeriodRepository.DeleteAsync(x => x.UpdateTemplateId == template.Id);

                // 4. Finally delete the template
                await _updateTemplateService.DeleteAsync(template);
            }

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> CopyTemplate(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            var newTemplateId = await CopyTemplateUtilityAsync(id);

            if (newTemplateId == 0)
            {
                _notificationService.ErrorNotification("Admin.UpdateTemplate.Unabletocopytemplate");
                return RedirectToAction("List");
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplate.Templatecopiedsuccessfully"));

            return RedirectToAction("Edit", new { id = newTemplateId });
        }
        [HttpPost]
        public async Task<IActionResult> CopySelected(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            if (string.IsNullOrEmpty(selectedIds))
            {
                _notificationService.WarningNotification("Nothing selected to copy.");
                return RedirectToAction("List");
            }

            var ids = selectedIds.Split(',').Select(int.Parse).ToList();

            foreach (var id in ids)
            {
                var template = await _updateTemplateService.GetByIdAsync(id);
                if (template == null)
                    continue;

                var newTemplate = new UpdateTemplate
                {
                    Title = template.Title + " (Copy)",
                    Description = template.Description,
                    FrequencyId = template.FrequencyId,
                    RepeatEvery = template.RepeatEvery,
                    RepeatType = template.RepeatType,
                    SelectedWeekDays = template.SelectedWeekDays,
                    OnDay = template.OnDay,
                    DueDate = template.DueDate,
                    DueTime = template.DueTime,
                    SubmitterUserIds = template.SubmitterUserIds,
                    ViewerUserIds = template.ViewerUserIds,
                    ReminderBeforeMinutes = template.ReminderBeforeMinutes,
                    IsFileAttachmentRequired = template.IsFileAttachmentRequired,
                    IsEditingAllowed = template.IsEditingAllowed,
                    IsActive = template.IsActive,
                    CreatedByUserId = template.CreatedByUserId,
                    CreatedOnUTC = DateTime.UtcNow
                };

                await _updateTemplateService.InsertAsync(newTemplate);

                var questions = await _questionRepository.Table
                    .Where(x => x.UpdateTemplateId == template.Id)
                    .ToListAsync();

                foreach (var question in questions)
                {
                    var newQuestion = new UpdateTemplateQuestion
                    {
                        UpdateTemplateId = newTemplate.Id,
                        QuestionText = question.QuestionText,
                        IsRequired = question.IsRequired,
                        ControlTypeId = question.ControlTypeId,
                        DisplayOrder = question.DisplayOrder,
                        ValidationMinLength = question.ValidationMinLength,
                        ValidationMaxLength = question.ValidationMaxLength,
                        ValidationFileMaximumSize = question.ValidationFileMaximumSize,
                        ValidationFileAllowedExtensions = question.ValidationFileAllowedExtensions,
                        DefaultValue = question.DefaultValue
                    };

                    await _questionRepository.InsertAsync(newQuestion);

                    var options = await _questionOptionRepository.Table
                        .Where(o => o.UpdateTemplateQuestionId == question.Id)
                        .ToListAsync();

                    foreach (var option in options)
                    {
                        await _questionOptionRepository.InsertAsync(new UpdateQuestionOption
                        {
                            UpdateTemplateQuestionId = newQuestion.Id,
                            Name = option.Name,
                            DisplayOrder = option.DisplayOrder,
                            IsPreSelected = option.IsPreSelected,
                            IsRequired = option.IsRequired
                        });
                    }
                }
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplate.Templatecopiedsuccessfully"));

            return RedirectToAction("List");
        }



        #endregion

        #region Update Question List/Create/Edit/Delete
        //public virtual async Task<IActionResult> QuestionList()
        //{
        //    if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateTemplate))
        //        return AccessDeniedView();

        //    //prepare model
        //    var model = await _updateTemplateQuestionModelFactory.PrepareUpdateTempleteQuestionSearchModelAsync(new UpdateTemplateQuestionSearchModel());

        //    return View("/Areas/Admin/Views/Extension/UpdateTemplateQuestion/List.cshtml", model);
        //}

        [HttpPost]
        public virtual async Task<IActionResult> QuestionList(UpdateTemplateQuestionSearchModel searchModel, int UpdateTemplateId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionTemplate, PermissionAction.View))
                return AccessDeniedView();
            searchModel.UpdateTemplateId = UpdateTemplateId;
            //prepare model
            var model = await _updateTemplateQuestionModelFactory.PrepareUpdateTemplateQuestionListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> QuestionCreate(int UpdateTemplateId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionTemplate, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _updateTemplateQuestionModelFactory.PrepareUpdateTemplateQuestionModelAsync(new UpdateTemplateQuestionModel(), null);
            model.UpdateTemplateId = UpdateTemplateId;
            ViewBag.UpdateTemplateId = UpdateTemplateId;
            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> QuestionCreate(UpdateTemplateQuestionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionTemplate, PermissionAction.Add))
                return AccessDeniedView();
            if (string.IsNullOrWhiteSpace(model.QuestionText))
            {
                ModelState.AddModelError(nameof(model.QuestionText), "Please provide the question.");
            }
            if (ModelState.IsValid)
            {
                var entity = new UpdateTemplateQuestion
                {
                    Id = model.Id,
                    UpdateTemplateId = model.UpdateTemplateId,
                    QuestionText = model.QuestionText,
                    IsRequired = model.IsRequired,
                    ControlTypeId = model.ControlTypeId,
                    DisplayOrder = model.DisplayOrder,

                    ValidationMinLength = model.ValidationMinLength,
                    ValidationMaxLength = model.ValidationMaxLength,
                    ValidationFileMaximumSize = model.ValidationFileMaximumSize,
                    ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions,
                    DefaultValue = model.DefaultValue
                };


                await _updateTemplateQuestionService.InsertAsync(entity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplateQuestion.Added"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "UpdateTemplate", new { id = entity.UpdateTemplateId });

                return RedirectToAction("QuestionEdit", new { id = entity.Id, updateTemplateId = entity.UpdateTemplateId });
            }
            //prepare model
            model = await _updateTemplateQuestionModelFactory.PrepareUpdateTemplateQuestionModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionCreate.cshtml", model);
        }

        public virtual async Task<IActionResult> QuestionEdit(int UpdateTemplateId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            var activity = await _updateTemplateQuestionService.GetByIdAsync(id);
            if (activity == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _updateTemplateQuestionModelFactory.PrepareUpdateTemplateQuestionModelAsync(null, activity);
            model.UpdateTemplateId = UpdateTemplateId;
            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionEdit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> QuestionEdit(UpdateTemplateQuestionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a update template with the specified id
            var activity = await _updateTemplateQuestionService.GetByIdAsync(model.Id);
            if (activity == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                activity.Id = model.Id;
                activity.UpdateTemplateId = model.UpdateTemplateId;
                activity.QuestionText = model.QuestionText;
                activity.IsRequired = model.IsRequired;
                activity.ControlTypeId = model.ControlTypeId;
                activity.DisplayOrder = model.DisplayOrder;

                activity.ValidationMinLength = model.ValidationMinLength;
                activity.ValidationMaxLength = model.ValidationMaxLength;
                activity.ValidationFileMaximumSize = model.ValidationFileMaximumSize;
                activity.ValidationFileAllowedExtensions = model.ValidationFileAllowedExtensions;
                activity.DefaultValue = model.DefaultValue;
                await _updateTemplateQuestionService.UpdateAsync(activity);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplateQuestion.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "UpdateTemplate", new { id = activity.UpdateTemplateId });

                return RedirectToAction("QuestionEdit", new { id = activity.Id, updateTemplateId = activity.UpdateTemplateId });
            }
            //prepare model
            model = await _updateTemplateQuestionModelFactory.PrepareUpdateTemplateQuestionModelAsync(model, null, true);
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionEdit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> QuestionDelete(int id,int updateTemplateId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionTemplate, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var activity = await _updateTemplateQuestionService.GetByIdAsync(id);
            if (activity == null)
                return RedirectToAction("Edit", new { id = updateTemplateId });

            //return RedirectToAction("Edit", new { id = updateTemplateId });

            await _updateTemplateQuestionService.DeleteAsync(activity);

           // _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.UpdateTemplateQuestion.Deleted"));
            // return Json(new { success = true });
            // return RedirectToAction("Edit", new { id = updateTemplateId });
            return new NullJsonResult();
        }



        //public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        //{
        //    if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageTitles))
        //        return AccessDeniedView();

        //    if (selectedIds == null || selectedIds.Count == 0)
        //        return NoContent();

        //    var data = await _updateTemplateQuestionService.GetByIdsAsync(selectedIds.ToArray());

        //    foreach (var item in data)
        //    {
        //        await _updateTemplateQuestionService.DeleteAsync(item);
        //    }

        //    return Json(new { Result = true });
        //}
        #endregion

        #region Update Question Option List/Create/Edit/Delete

        [HttpPost]
        public async Task<IActionResult> QuestionOptionList(UpdateQuestionOptionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionOptionTemplate, PermissionAction.View))
                return AccessDeniedView();

            // Prepare model
            var model = await PrepareQuestionOptionListModelAsync(searchModel);

            return Json(model);
        }
        public async Task<IActionResult> UpdateQuestionAttributeCreatePopup(int updateTemplateQuestionId, string formId, string btnId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionOptionTemplate, PermissionAction.Add))
                return AccessDeniedView();

            var model = await PrepareQuestionOptionModelAsync(new UpdateQuestionOptionModel(), null);
            model.UpdateTemplateQuestionId = updateTemplateQuestionId;
            var question = await _updateTemplateQuestionService.GetByIdAsync(updateTemplateQuestionId);
            if (question != null)
            {
                model.ControlType = question.ControlTypeId; // <-- set control type
            }
            ViewBag.FormId = formId;
            ViewBag.BtnId = btnId;
            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionAttributeCreatePopup.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> UpdateQuestionAttributeCreatePopup(UpdateQuestionOptionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionOptionTemplate, PermissionAction.Add))
                return AccessDeniedView();

            var updateQuestionOption = new UpdateQuestionOption();
            model = await PrepareQuestionOptionModelAsync(model, updateQuestionOption, true);

            ViewBag.FormId = Request.Form["formId"];
            ViewBag.BtnId = Request.Form["btnId"];

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Please provide the name.");
            }

            //  PRE-SELECT VALIDATION (ONLY FOR SINGLE SELECT)
            if (model.IsPreSelected && IsSingleSelect(model.ControlType))
            {
                var alreadySelected = await HasAnotherPreSelectedAsync(
                    model.UpdateTemplateQuestionId);

                if (alreadySelected)
                {
                    ModelState.AddModelError(
                        nameof(model.IsPreSelected),
                        "Is pre selected is already selected in another option."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionAttributeCreatePopup.cshtml", model);
            }

            updateQuestionOption.UpdateTemplateQuestionId = model.UpdateTemplateQuestionId;
            updateQuestionOption.Name = model.Name;
            updateQuestionOption.DisplayOrder = model.DisplayOrder;
            updateQuestionOption.IsPreSelected = model.IsPreSelected;
            updateQuestionOption.IsRequired = model.IsRequired;

            await _updateQuestionOptionService.InsertAsync(updateQuestionOption);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.UpdateQuestionOption.Added")
            );

            ViewBag.RefreshPage = true;

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionAttributeCreatePopup.cshtml", model);
        }



        public virtual async Task<IActionResult> UpdateQuestionAttributeValueEditPopup(int id, int updateTemplateQuestionId, string formId, string btnId)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionOptionTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            var questionOption = await _updateQuestionOptionService.GetByIdAsync(id);
            if (questionOption == null)
                return RedirectToAction("QuestionOptionList");

            //prepare model
            var model = await PrepareQuestionOptionModelAsync(null, questionOption);
            model.UpdateTemplateQuestionId = updateTemplateQuestionId;
            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionAttributeValueEditPopup.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> UpdateQuestionAttributeValueEditPopup(UpdateQuestionOptionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionOptionTemplate, PermissionAction.Edit))
                return AccessDeniedView();

            var updateQuestionOption = await _updateQuestionOptionService.GetByIdAsync(model.Id);
            if (updateQuestionOption == null)
                return RedirectToAction("QuestionOptionList");

            ViewBag.FormId = Request.Form["formId"];
            ViewBag.BtnId = Request.Form["btnId"];

            //  PRE-SELECT VALIDATION (ONLY FOR SINGLE SELECT)
            if (model.IsPreSelected && IsSingleSelect(model.ControlType))
            {
                var alreadySelected = await HasAnotherPreSelectedAsync(
                    model.UpdateTemplateQuestionId,
                    model.Id);

                if (alreadySelected)
                {
                    ModelState.AddModelError(
                        nameof(model.IsPreSelected),
                        "Is pre selected is already selected in another option."
                    );
                }
            }

            if (!ModelState.IsValid)
            {
                return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionAttributeValueEditPopup.cshtml", model);
            }

            //  UPDATE EXISTING ENTITY (NO RECREATE)
            updateQuestionOption.Name = model.Name;
            updateQuestionOption.DisplayOrder = model.DisplayOrder;
            updateQuestionOption.IsPreSelected = model.IsPreSelected;
            updateQuestionOption.IsRequired = model.IsRequired;

            await _updateQuestionOptionService.UpdateAsync(updateQuestionOption);

            _notificationService.SuccessNotification(
                await _localizationService.GetResourceAsync("Admin.UpdateQuestionOption.Updated")
            );

            if (!continueEditing)
                ViewBag.RefreshPage = true;

            return View("/Areas/Admin/Views/Extension/UpdateTemplate/QuestionAttributeValueEditPopup.cshtml", model);
        }


        [HttpPost]
        public virtual async Task<IActionResult> QuestionOptionDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(SatyanamPermissionProvider.ManageUpdateQuestionOptionTemplate, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a deals with the specified id
            var updateQuestionOption = await _updateQuestionOptionService.GetByIdAsync(id);
            if (updateQuestionOption == null)
                return RedirectToAction("QuestionOptionList");

            await _updateQuestionOptionService.DeleteAsync(updateQuestionOption);

            return new NullJsonResult();

        }
        #endregion

    }
}