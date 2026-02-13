using App.Core;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Media;
using App.Core.Domain.Messages;
using App.Core.Domain.Security;
using App.Data;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Localization;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Controllers;
using App.Web.Models.Extensions.UpdateForm;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Web.Controllers
{
    public partial class UpdateSubmissionController : BasePublicController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IRepository<UpdateTemplate> _updateTemplateRepository;
        private readonly IRepository<UpdateTemplateQuestion> _questionRepository;
        private readonly IRepository<UpdateQuestionOption> _optionRepository;
        private readonly IRepository<UpdateSubmission> _submissionRepository;
        private readonly IRepository<UpdateSubmissionAnswer> _answerRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IEmployeeService _employeeService;
        private readonly ICustomerService _customerService;
        private readonly IUpdateSubmissionService _updateSubmissionService;
        private readonly IRepository<UpdateTemplatePeriod> _periodRepository;
        private readonly IRepository<UpdateSubmissionReviewer> _reviewerRepository;
        private readonly IRepository<UpdateTemplatePeriod> _updateTemplatePeriodRepository;
        private readonly IUpdateSubmissionCommentService _updateSubmissionCommentService;
        private readonly IRepository<UpdateSubmissionComment> _updateSubmissionCommentRepository;
        private readonly IStoreContext _storeContext;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDownloadService _downloadService;
        private readonly IRepository<Download> _downloadRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor

        public UpdateSubmissionController(IPermissionService permissionService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IRepository<UpdateTemplate> updateTemplateRepository,
            IRepository<UpdateTemplateQuestion> questionRepository,
            IRepository<UpdateQuestionOption> optionRepository,
            IRepository<UpdateSubmission> submissionRepository,
            IRepository<UpdateSubmissionAnswer> answerRepository,
            IEmployeeService employeeService,
            ICustomerService customerService,
            IUpdateSubmissionService updateSubmissionService,
            IRepository<Employee> employeeRepository,
            IRepository<UpdateTemplatePeriod> periodRepository,
            IRepository<UpdateSubmissionReviewer> reviewerRepository,
            IRepository<UpdateTemplatePeriod> updateTemplatePeriodRepository,
            IUpdateSubmissionCommentService updateSubmissionCommentService,
            IRepository<UpdateSubmissionComment> updateSubmissionCommentRepository,
            IStoreContext storeContext,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment,
            IDownloadService downloadService,
            IRepository<Download> downloadRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _updateTemplateRepository = updateTemplateRepository;
            _questionRepository = questionRepository;
            _optionRepository = optionRepository;
            _submissionRepository = submissionRepository;
            _answerRepository = answerRepository;
            _employeeService = employeeService;
            _customerService = customerService;
            _updateSubmissionService = updateSubmissionService;
            _employeeRepository = employeeRepository;
            _periodRepository = periodRepository;
            _reviewerRepository = reviewerRepository;
            _updateTemplatePeriodRepository = updateTemplatePeriodRepository;
            _updateSubmissionCommentService = updateSubmissionCommentService;
            _updateSubmissionCommentRepository = updateSubmissionCommentRepository;
            _storeContext = storeContext;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
            _downloadService = downloadService;
            _downloadRepository = downloadRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        #endregion

        #region Utilities
        private async Task<SubmitUpdateFormModel> PrepareSubmitModelAsync(int templateId)
        {
            var template = await _updateTemplateRepository.GetByIdAsync(templateId);
            if (template == null || !template.IsActive)
                return null;

            var model = new SubmitUpdateFormModel
            {
                UpdateTemplateId = template.Id,
                Title = template.Title,
                Description = template.Description,
                IsFileAttachmentRequired = template.IsFileAttachmentRequired
            };

            var questions = await _questionRepository.GetAllAsync(q => q
                .Where(x => x.UpdateTemplateId == template.Id)
                .OrderBy(x => x.DisplayOrder));

            foreach (var q in questions)
            {
                var controlType = ((ControlTypeEnum)q.ControlTypeId).ToString();
                var answerText = !string.IsNullOrWhiteSpace(q.DefaultValue) &&
                                 (q.DefaultValue.Trim() == "0" || q.DefaultValue.Trim() == "1")
                                 ? "" : q.DefaultValue;

                var qModel = new SubmitUpdateQuestionModel
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    IsRequired = q.IsRequired,
                    ControlType = controlType,
                    DefaultValue = q.DefaultValue,
                    DisplayOrder = q.DisplayOrder,
                    AnswerText = answerText,

                    MinLength = q.ValidationMinLength,
                    MaxLength = q.ValidationMaxLength,
                    MaximumFileSizeKb = q.ValidationFileMaximumSize,
                    AllowedFileExtensions = q.ValidationFileAllowedExtensions
                };

                if (q.ControlTypeId is (int)ControlTypeEnum.DropdownList or
                                       (int)ControlTypeEnum.RadioList or
                                       (int)ControlTypeEnum.Checkboxes or
                                       (int)ControlTypeEnum.ReadonlyCheckboxes)
                {
                    var options = await _optionRepository.GetAllAsync(o =>
                        o.Where(x => x.UpdateTemplateQuestionId == q.Id)
                         .OrderBy(x => x.DisplayOrder));
                    if (q.ControlTypeId == (int)ControlTypeEnum.Checkboxes)
                    {
                        qModel.Options = options.Select(o => new SelectListItem
                        {
                            Text = o.Name,
                            Value = o.Id.ToString(),       // ID REQUIRED
                            Selected = o.IsPreSelected
                        }).ToList();

                        qModel.RequiredOptionValues = options
                            .Where(o => o.IsRequired)
                            .Select(o => o.Id.ToString()) // MATCH ID
                            .ToList();
                    }
                    else
                    {
                        qModel.Options = options.Select(o => new SelectListItem
                        {
                            Text = o.Name,
                            Value = o.Name,
                            Selected = o.IsPreSelected
                        }).ToList();
                        qModel.RequiredOptionValues = options.Where(o => o.IsRequired).Select(o => o.Id.ToString()).ToList();
                    }
                    if (q.ControlTypeId == (int)ControlTypeEnum.ReadonlyCheckboxes)
                    {
                        var selectedValues = options.Select(o => o.Name);
                        qModel.AnswerText = string.Join(",", selectedValues);
                    }
                }

                model.Questions.Add(qModel);
            }

            model.Questions = model.Questions.OrderBy(x => x.DisplayOrder).ToList();
            return model;
        }

        private bool ValidateRequiredAnswers(SubmitUpdateFormModel model)
        {
            bool isValid = true;

            for (int i = 0; i < model.Questions.Count; i++)
            {
                var q = model.Questions[i];

                if (q.IsRequired)
                {
                    if (q.ControlType == "FileUpload" && q.UploadedFile == null)
                    {
                        var label = q.QuestionText?.TrimEnd(':') ?? $"Question {i + 1}";
                        ModelState.AddModelError($"Questions[{i}].UploadedFile", $"{label} is required.");
                        isValid = false;
                    }
                    else if (string.IsNullOrWhiteSpace(q.AnswerText))
                    {
                        var label = string.IsNullOrWhiteSpace(q.QuestionText) ? $"Question {i + 1}" : q.QuestionText;
                        var errorMessage = $"Please answer \"{label}\".";

                        // Special case for RadioList and Checkboxes – also add generic error for AnswerText-error fallback span
                        ModelState.AddModelError($"Questions[{i}].AnswerText", errorMessage);
                        ModelState.AddModelError($"Questions[{i}].AnswerText-error", errorMessage);
                        isValid = false;
                    }


                }
            }

            return isValid;
        }

        private async Task RebindQuestionOptionsAsync(SubmitUpdateFormModel model)
        {
            for (int i = 0; i < model.Questions.Count; i++)
            {
                var q = model.Questions[i];
                var questionEntity = await _questionRepository.GetByIdAsync(q.QuestionId);
                if (questionEntity == null) continue;

                var controlTypeId = questionEntity.ControlTypeId;

                if (controlTypeId is (int)ControlTypeEnum.DropdownList or
                                     (int)ControlTypeEnum.RadioList or
                                     (int)ControlTypeEnum.Checkboxes or
                                     (int)ControlTypeEnum.ReadonlyCheckboxes)
                {
                    var options = await _optionRepository.GetAllAsync(o =>
                        o.Where(x => x.UpdateTemplateQuestionId == q.QuestionId).OrderBy(x => x.DisplayOrder));

                    q.Options = options.Select(o => new SelectListItem
                    {
                        Text = o.Name,
                        Value = o.Name,
                        Selected = o.IsPreSelected
                    }).ToList();

                    if (controlTypeId == (int)ControlTypeEnum.ReadonlyCheckboxes)
                        q.AnswerText = string.Join(",", options.Select(o => o.Name));
                }
            }
        }

        public async Task<IList<SubmissionCardModel>> GetSubmissionCardsAsync(int currentUserId,int? filterSubmitterId,DateTime? from,DateTime? to,int? selectedTemplateId)
        {
            var submissionsQuery = _submissionRepository.Table;

            if (filterSubmitterId.HasValue)
                submissionsQuery = submissionsQuery.Where(x => x.SubmittedByCustomerId == filterSubmitterId.Value);

            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");

            DateTime? fromUtc = null;
            DateTime? toUtc = null;

            if (from.HasValue)
                fromUtc = TimeZoneInfo.ConvertTimeToUtc(from.Value, istTimeZone);

            if (to.HasValue)
            {
                var inclusiveTo = to.Value.Date.AddDays(1).AddTicks(-1);
                toUtc = TimeZoneInfo.ConvertTimeToUtc(inclusiveTo, istTimeZone);
            }

            if (fromUtc.HasValue)
                submissionsQuery = submissionsQuery.Where(x => x.SubmittedOnUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                submissionsQuery = submissionsQuery.Where(x => x.SubmittedOnUtc <= toUtc.Value);


            if (selectedTemplateId.HasValue)
                submissionsQuery = submissionsQuery.Where(x => x.UpdateTemplateId == selectedTemplateId.Value);

            var submissions = await submissionsQuery.ToListAsync();
            if (!submissions.Any())
                return new List<SubmissionCardModel>();

            var submissionIds = submissions.Select(s => s.Id).ToList();
            //var submitterIds = submissions.Select(s => s.SubmittedByCustomerId).Distinct().ToList();
            var submitterIds = submissions
            .Select(s => s.SubmittedByCustomerId)
            .Where(id => id.HasValue)
            .Select(id => id.Value)
            .Distinct()
            .ToList();
            var templateIds = submissions.Select(s => s.UpdateTemplateId).Distinct().ToList();

            // answers/questions/templates/submitters as earlier
            var allAnswers = await _answerRepository.Table
                .Where(a => submissionIds.Contains(a.UpdateSubmissionId))
                .ToListAsync();

            var questionIds = allAnswers.Select(a => a.UpdateTemplateQuestionId).Distinct().ToList();
            var allQuestions = await _questionRepository.Table
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();

            var allSubmitters = await _customerService.GetCustomersByIdsAsync(submitterIds.ToArray());
            var allTemplates = await _updateTemplateRepository.Table
                .Where(t => templateIds.Contains(t.Id))
                .ToListAsync();

            // load *all comments* for these submissions in one query
            var allComments = await _updateSubmissionCommentRepository.Table
                .Where(c => submissionIds.Contains(c.UpdateSubmissionId))
                .OrderBy(c => c.CreatedOnUtc)
                .ToListAsync();

            // Build models (iterate submissions in the desired ordering)
            var groupedSubmissions = submissions
                .GroupBy(s => s.SubmittedByCustomerId)
                .OrderByDescending(g => g.Max(x => x.SubmittedOnUtc))
                .SelectMany(g => g.OrderByDescending(x => x.SubmittedOnUtc))
                .ToList();

            var result = new List<SubmissionCardModel>();
            foreach (var sub in groupedSubmissions)
            {
                var answers = allAnswers.Where(a => a.UpdateSubmissionId == sub.Id).ToList();
                var questionModels = new List<SubmitUpdateQuestionModel>();

                foreach (var ans in answers)
                {
                    var question = allQuestions.FirstOrDefault(q => q.Id == ans.UpdateTemplateQuestionId);

                    var displayAnswer = ans.AnswerText;

                    // ✅ FIX FOR CHECKBOXES
                    if (question != null &&
                        (question.ControlTypeId == (int)ControlTypeEnum.Checkboxes ||
                         question.ControlTypeId == (int)ControlTypeEnum.ReadonlyCheckboxes))
                    {
                        displayAnswer = await GetCheckboxAnswerDisplayTextAsync(
                            question.Id,
                            ans.AnswerText);
                    }

                    questionModels.Add(new SubmitUpdateQuestionModel
                    {
                        QuestionId = ans.UpdateTemplateQuestionId,
                        QuestionText = question?.QuestionText ?? "[Deleted Question]",
                        AnswerText = displayAnswer,
                        ControlType = question?.ControlTypeId.ToString() ?? "TextBox",
                        FileName = ans.FileName
                    });
                }

                var submitter = allSubmitters.FirstOrDefault(c => c.Id == sub.SubmittedByCustomerId);
                var submitterName = submitter != null ? $"{submitter.FirstName} {submitter.LastName}".Trim() : "Unknown";
                var template = allTemplates.FirstOrDefault(t => t.Id == sub.UpdateTemplateId);
                var templateName = template?.Title ?? "Unknown Template";

                var model = new SubmissionCardModel
                {
                    Id = sub.Id,                  // ensures your view's submission.Id still works
                    SubmissionId = sub.Id,        // duplicate if you want explicit prop
                    SubmitterName = submitterName,
                    SubmittedOn = TimeZoneInfo.ConvertTimeFromUtc(sub.SubmittedOnUtc, istTimeZone),
                    TemplateName = templateName,
                    Questions = questionModels
                };

                // Get comment entities for this submission and map using PrepareCommentModels
                var commentsForThisSubmission = allComments.Where(c => c.UpdateSubmissionId == sub.Id).ToList();
                if (commentsForThisSubmission.Any())
                {
                    var commentModels = await PrepareCommentModels(commentsForThisSubmission);
                    model.Comments = commentModels;
                }
                else
                {
                    model.Comments = new List<UpdateSubmissionCommentModel>();
                }

                result.Add(model);
            }
            return result;
        }

        private async Task<List<UpdateSubmissionAnswer>> MapAnswersAsync(SubmitUpdateFormModel model)
        {
            var answers = new List<UpdateSubmissionAnswer>();

            foreach (var q in model.Questions)
            {
                string filePath = null;
                string fileName = null;
                string answerText = null;

                if (q.ControlType == "FileUpload" && q.UploadedFile != null && q.UploadedFile.Length > 0)
                {
                    // Generate unique stored file name
                    var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(q.UploadedFile.FileName)}";
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploadedfiles");
                    Directory.CreateDirectory(uploadsFolder);
                    var fullPath = Path.Combine(uploadsFolder, storedFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await q.UploadedFile.CopyToAsync(stream);
                    }

                    filePath = "/uploadedfiles/" + storedFileName;   // stored location
                    fileName = q.UploadedFile.FileName;              // original name
                }
                else
                {
                    // Non-file answers
                    answerText = q.AnswerText;
                }

                answers.Add(new UpdateSubmissionAnswer
                {
                    UpdateTemplateQuestionId = q.QuestionId,
                    AnswerText = answerText,  // only for text-type answers
                    FilePath = filePath,      // only for file-type answers
                    FileName = fileName       // only for file-type answers
                });
            }
            return answers;
        }

        private async Task<List<UpdateSubmissionCommentModel>> PrepareCommentModels(IEnumerable<UpdateSubmissionComment> comments)
        {
            var commentModels = new List<UpdateSubmissionCommentModel>();
            var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            // Build dictionary of all comments for quick lookup
            var commentList = comments.ToList();

            // Top-level comments (ParentCommentId == null)
            foreach (var comment in commentList.Where(x => x.ParentCommentId == null).OrderBy(c => c.CreatedOnUtc))
            {
                var commenter = await _customerService.GetCustomerByIdAsync(comment.CommentedByCustomerId);

                var rootModel = new UpdateSubmissionCommentModel
                {
                    Id = comment.Id,
                    UpdateSubmissionId = comment.UpdateSubmissionId,
                    ParentCommentId = comment.ParentCommentId,
                    CommentText = comment.CommentText,
                    CommentedByCustomerId = comment.CommentedByCustomerId,
                    CommentedByName = commenter != null ? $"{commenter.FirstName} {commenter.LastName}".Trim() : "Unknown",
                    CreatedOnUtc = TimeZoneInfo.ConvertTimeFromUtc(comment.CreatedOnUtc,istTimeZone)
                };

                // Now find replies for this root comment and recursively map them
                rootModel.Replies = await BuildRepliesRecursive(commentList, comment.Id);

                commentModels.Add(rootModel);
            }

            return commentModels;
        }

        private async Task<List<UpdateSubmissionCommentModel>> BuildRepliesRecursive(List<UpdateSubmissionComment> allComments, int parentId)
        {
            var replies = new List<UpdateSubmissionCommentModel>();

            var children = allComments.Where(c => c.ParentCommentId == parentId)
                                      .OrderBy(c => c.CreatedOnUtc)
                                      .ToList();

            foreach (var child in children)
            {
                var replier = await _customerService.GetCustomerByIdAsync(child.CommentedByCustomerId);
                var childModel = new UpdateSubmissionCommentModel
                {
                    Id = child.Id,
                    UpdateSubmissionId = child.UpdateSubmissionId,
                    ParentCommentId = child.ParentCommentId,
                    CommentText = child.CommentText,
                    CommentedByCustomerId = child.CommentedByCustomerId,
                    CommentedByName = replier != null ? $"{replier.FirstName} {replier.LastName}".Trim() : "Unknown",
                    CreatedOnUtc = child.CreatedOnUtc
                };

                // recursion
                childModel.Replies = await BuildRepliesRecursive(allComments, child.Id);

                replies.Add(childModel);
            }

            return replies;
        }

        // helper: resolve reviewers for a submission, with a safe fallback to template.ViewerUserIds
        private async Task<List<Employee>> ResolveReviewersAsync(UpdateTemplate template)
        {
            var reviewers = new List<Employee>();

            if (!string.IsNullOrWhiteSpace(template?.ViewerUserIds))
            {
                foreach (var s in template.ViewerUserIds.Split(','))
                {
                    if (int.TryParse(s.Trim(), out var id) && id > 0)
                    {
                        var emp = await _employeeService.GetEmployeeByIdAsync(id);
                        if (!string.IsNullOrWhiteSpace(emp?.OfficialEmail))
                            reviewers.Add(emp);
                    }
                }
            }
            return reviewers;
        }

        public async Task NotifyOnNewCommentAsync(UpdateSubmissionComment comment)
        {
            // Load submission
            var submission = await _updateSubmissionService.GetSubmissionByIdAsync(comment.UpdateSubmissionId);
            if (submission == null) return;

            // Commenter
            var commenter = await _customerService.GetCustomerByIdAsync(comment.CommentedByCustomerId);
            bool isSubmitter = submission.SubmittedByCustomerId == commenter.Id;

            // Common bits
            var template = await _updateTemplateRepository.GetByIdAsync(submission.UpdateTemplateId);
            //var store = await _storeContext.GetCurrentStoreAsync();
            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null) return;

            var request = _httpContextAccessor.HttpContext?.Request;
            string baseUrl;

            if (request != null)
            {
                baseUrl = $"{request.Scheme}://{request.Host}".TrimEnd('/');
            }
            else
            {
                // fallback to store URL if request is unavailable (e.g. background task)
                var store = await _storeContext.GetCurrentStoreAsync();
                baseUrl = (store.Url ?? "").TrimEnd('/');
            }
            var authorName = $"{commenter?.FirstName} {commenter?.LastName}".Trim();
            var submissionTitle = template?.Title ?? "Submission";
            var safeComment = System.Net.WebUtility.HtmlEncode(comment.CommentText).Replace("\n", "<br/>");

            if (!isSubmitter)
            {
                // Reviewer commented → notify submitter only
                var submitter = await _customerService.GetCustomerByIdAsync(submission.SubmittedByCustomerId.Value);
                if (submitter == null || string.IsNullOrWhiteSpace(submitter.Email)) return;

                var formLink = $"{baseUrl}/UpdateSubmission/Submit/{submission.UpdateTemplateId}?periodId={submission.PeriodId}";
                var subject = $"New comment on your submission #{submission.Id}";
                var body = $@"
                <p><strong>{authorName}</strong> left a comment on <strong>{submissionTitle}</strong>.</p>
                <blockquote style=""border-left:3px solid #eee;padding-left:10px;margin:10px 0;"">{safeComment}</blockquote>
                <p><a href=""{formLink}"" target=""_blank"">Open the submission to reply</a></p>";

                await _queuedEmailService.InsertQueuedEmailAsync(new QueuedEmail
                {
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = submitter.Email,
                    ToName = $"{submitter.FirstName} {submitter.LastName}",
                    Subject = subject,
                    Body = body,
                    Priority = QueuedEmailPriority.High,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                });
                return;
            }

            // Submitter replied → notify reviewers (primary + CC), robustly resolved
            var reviewers = await ResolveReviewersAsync(template);
            if (!reviewers.Any()) return;

            var allComments = await _updateSubmissionService.GetCommentsBySubmissionIdAsync(submission.Id);
            var lastReviewerId = allComments
                .Where(c => c.CommentedByCustomerId != submission.SubmittedByCustomerId) // exclude submitter
                .OrderByDescending(c => c.CreatedOnUtc)
                .Select(c => c.CommentedByCustomerId)
                .FirstOrDefault();
            dynamic primaryReviewer = null;

            if (lastReviewerId > 0)
                primaryReviewer = reviewers.FirstOrDefault(r => r.Customer_Id == lastReviewerId);

            if (primaryReviewer == null)
                primaryReviewer = reviewers.FirstOrDefault(r => r.Customer_Id != commenter.Id);

            // If still null (no reviewer) -> nothing to do
            if (primaryReviewer == null) return;

            // Build CC: reviewers excluding the primary and excluding the submitter/commenter
            var ccReviewerEmails = reviewers
                .Where(r => r.Customer_Id != primaryReviewer.Customer_Id && r.Customer_Id != commenter.Id)
                .Select(r => r.OfficialEmail)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Distinct()
                .ToList();
            //i need to pass submitereid and templateid this following line
            //var reviewLink = $"{baseUrl}/UpdateSubmission/List/{submission.UpdateTemplateId}";
            var reviewLink = $"{baseUrl}/UpdateSubmission/List?selectedSubmitterId={submission.SubmittedByCustomerId}&selectedTemplateId={submission.UpdateTemplateId}";
            var subj = $"Submitter replied on submission";
            var bodyHtml = $@"
            <p><strong>{authorName}</strong> left a comment on <strong>{submissionTitle}</strong>.</p>
            <blockquote style=""border-left:3px solid #eee;padding-left:10px;margin:10px 0;"">{safeComment}</blockquote>
            <p><a href=""{reviewLink}"" target=""_blank"">Open the submission to reply</a></p>";

            await _queuedEmailService.InsertQueuedEmailAsync(new QueuedEmail
            {
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = primaryReviewer.OfficialEmail,
                ToName = $"{primaryReviewer.FirstName} {primaryReviewer.LastName}",
                Subject = subj,
                Body = bodyHtml,
                Priority = QueuedEmailPriority.High,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id,
                CC = string.Join(",", ccReviewerEmails)
            });
        }

        private async Task<string> GetCheckboxAnswerDisplayTextAsync(int questionId,string answerText)
        {
            if (string.IsNullOrWhiteSpace(answerText))
                return string.Empty;

            var selectedIds = answerText
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var id) ? id : 0)
                .Where(x => x > 0)
                .ToList();

            if (!selectedIds.Any())
                return answerText;

            var optionNames = await _optionRepository.Table
                .Where(o => o.UpdateTemplateQuestionId == questionId &&
                            selectedIds.Contains(o.Id))
                .OrderBy(o => o.DisplayOrder)
                .Select(o => o.Name)
                .ToListAsync();

            return string.Join(", ", optionNames);
        }

        private async Task<List<string>> GetRequiredOptionTextsByQuestionIdAsync(int questionId)
        {
            return await _optionRepository.Table
        .Where(o => o.UpdateTemplateQuestionId == questionId && o.IsRequired)
        .Select(o => o.Id.ToString())   // MUST match checkbox value
        .ToListAsync();
        }

        private async Task NotifyReviewersOnSubmissionAsync(UpdateSubmission submission,Customer submitter,bool isEdit)
        {
            var template = await _updateTemplateRepository.GetByIdAsync(submission.UpdateTemplateId);
            if (template == null || string.IsNullOrEmpty(template.ViewerUserIds))
                return;

            var reviewerIds = template.ViewerUserIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.Parse(x))
                .ToList();

            var emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();
            if (emailAccount == null) return;

            var request = _httpContextAccessor.HttpContext?.Request;

            string baseUrl = request != null
                ? $"{request.Scheme}://{request.Host}"
                : (await _storeContext.GetCurrentStoreAsync()).Url;

            var reviewLink =
                $"{baseUrl}/UpdateSubmission/List?selectedSubmitterId={submission.SubmittedByCustomerId}&selectedTemplateId={submission.UpdateTemplateId}";

            foreach (var reviewerId in reviewerIds)
            {
                var reviewer = await _employeeService.GetEmployeeByIdAsync(reviewerId);
                if (reviewer == null || string.IsNullOrWhiteSpace(reviewer.OfficialEmail))
                    continue;

                string subject;
                string body;

                if (!isEdit)
                {
                    //  FIRST SUBMIT
                    subject = $"New submission received - {template.Title}";

                    body = $@"
                <p>Hello {reviewer.FirstName},</p>

                <p>
                    <strong>{submitter.FirstName} {submitter.LastName}</strong>
                    submitted a NEW update for <strong>{template.Title}</strong>.
                </p>

                <p>Submitted On: {DateTime.UtcNow.ToLocalTime():M/d/yyyy h:mm tt}</p>

                <p><a href='{reviewLink}'>Open submission</a></p>";
                }
                else
                {
                    //  EDIT SUBMIT
                    subject = $"Submission updated - {template.Title}";

                    body = $@"
                <p>Hello {reviewer.FirstName},</p>

                <p>
                    <strong>{submitter.FirstName} {submitter.LastName}</strong>
                    UPDATED their submission for <strong>{template.Title}</strong>.
                </p>

                <p>Please review the changes.</p>

                <p>Updated On: {DateTime.UtcNow.ToLocalTime():M/d/yyyy h:mm tt}</p>

                <p><a href='{reviewLink}'>Open updated submission</a></p>";
                }

                await _queuedEmailService.InsertQueuedEmailAsync(new QueuedEmail
                {
                    From = emailAccount.Email,
                    FromName = emailAccount.DisplayName,
                    To = reviewer.OfficialEmail,
                    ToName = $"{reviewer.FirstName} {reviewer.LastName}",
                    Subject = subject,
                    Body = body,
                    Priority = QueuedEmailPriority.High,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                });
            }
        }


        #endregion

        #region Actions

        [HttpGet]
        public async Task<IActionResult> DownloadFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return NotFound();

            // Split into name + extension
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);

            // Lookup download record by Filename + Extension
            var download = await _downloadRepository.Table
                .FirstOrDefaultAsync(x => x.Filename == baseName && x.Extension == extension);

            if (download == null)
                return NotFound();

            if (download.DownloadBinary == null || download.DownloadBinary.Length == 0)
                return Content("Download data is not available any more.");

            var finalFileName = !string.IsNullOrWhiteSpace(download.Filename)
                ? download.Filename + download.Extension
                : $"{download.Id}{download.Extension}";

            var contentType = !string.IsNullOrWhiteSpace(download.ContentType)
                ? download.ContentType
                : "application/octet-stream";

            return File(download.DownloadBinary, contentType, finalFileName);
        }

        [HttpGet]
        public virtual async Task<IActionResult> SubmissionList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreViewUpdate, PermissionAction.View))
                return Challenge();

            var model = new UpdateSubmissionSearchModel();
            return View("~/Themes/DefaultClean/Views/Extension/UpdateForm/SubmissionList.cshtml", model);
        }

        public async Task<IActionResult> UpdateSubmissionList(UpdateSubmissionSearchModel searchModel)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var employee = await _employeeRepository.Table
                .FirstOrDefaultAsync(e => e.Customer_Id == customer.Id);

            var employeeIdStr = employee?.Id.ToString();

            if (employeeIdStr == null)
                return Json(Enumerable.Empty<object>()); // No employee record, return nothing

            var templates = await _updateTemplateRepository.Table.Where(t=>t.IsActive).ToListAsync();

            var userSubmissions = await _submissionRepository.Table
                .Where(x => x.SubmittedByCustomerId == customer.Id)
                .ToListAsync();

            var result = templates
                .Select(t =>
                {
                    // Get who can submit or review this template
                    var submitterIds = (t.SubmitterUserIds ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim());

                    var viewerIds = (t.ViewerUserIds ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim());

                    bool canSubmit = submitterIds.Contains(employeeIdStr);
                    bool canReview = viewerIds.Contains(employeeIdStr);

                    // If user has no permission at all, skip this row
                    if (!canSubmit && !canReview)
                        return null;

                    var submission = userSubmissions
                        .Where(s => s.UpdateTemplateId == t.Id)
                        .OrderByDescending(s => s.SubmittedOnUtc)
                        .FirstOrDefault();

                    return new
                    {
                        t.Id,
                        t.Title,
                        IsSubmitted = submission != null,
                        PeriodId = submission?.PeriodId ?? 0,
                        CanSubmit = canSubmit,
                        CanReview = canReview
                    };
                })
                .Where(x => x != null); // Remove nulls (unauthorized templates)
            // Filters
            if (!string.IsNullOrEmpty(searchModel.Title))
                result = result.Where(x => x.Title.Contains(searchModel.Title, StringComparison.OrdinalIgnoreCase));
            if (searchModel.IsSubmitted.HasValue)
                result = result.Where(x => x.IsSubmitted == searchModel.IsSubmitted.Value);
            return Json(result);
        }

        [HttpGet("UpdateSubmission/Submit/{id}")]
        public async Task<IActionResult> Submit(int? id, int? periodId)
        {
            if (!id.HasValue)
            {
                _notificationService.WarningNotification("Template ID is missing.");
                return RedirectToRoute("HomePage");
            }
            var customer = await _workContext.GetCurrentCustomerAsync();
            var updateTemplate = await _updateTemplateRepository.GetByIdAsync(id.Value);
            // Fetch all periods for the template
            var allPeriods = await _periodRepository.Table
                .Where(p => p.UpdateTemplateId == id)
                .OrderByDescending(p => p.PeriodStart)
                .ToListAsync();
            // Fetch user's previous    
            var userSubmissions = await _submissionRepository.Table
                .Where(x => x.UpdateTemplateId == id.Value && x.SubmittedByCustomerId == customer.Id)
                .ToListAsync();
            // Determine selected period
            UpdateTemplatePeriod selectedPeriod = null;
            if (periodId.HasValue)
            {
                selectedPeriod = allPeriods.FirstOrDefault(p => p.Id == periodId.Value);
            }
            if (selectedPeriod == null)
            {
                // Step 1: Try to get most recent submitted period
                selectedPeriod = allPeriods
                    .OrderByDescending(p => p.PeriodStart)
                    .FirstOrDefault(p => userSubmissions.Any(s => s.PeriodId == p.Id));

                // Step 2: If no submission found, fallback to latest available period
                if (selectedPeriod == null)
                {
                    selectedPeriod = allPeriods
                        .OrderByDescending(p => p.PeriodStart)
                        .FirstOrDefault();
                }
            }
            // Fetch submission for selected period
            var existingSubmission = await _submissionRepository.Table
                .Where(x => x.UpdateTemplateId == id.Value &&
                            x.SubmittedByCustomerId == customer.Id &&
                            x.PeriodId == selectedPeriod.Id)
                .FirstOrDefaultAsync();

            // Prepare form model
            var model = await PrepareSubmitModelAsync(id.Value);
            model.PeriodId = selectedPeriod.Id;
            model.HasAlreadySubmitted = existingSubmission != null;
            model.AllowEditingAfterSubmit = updateTemplate.IsEditingAllowed;

            if (existingSubmission != null)
            {
                var answers = await _answerRepository.Table
                    .Where(a => a.UpdateSubmissionId == existingSubmission.Id)
                    .ToListAsync();

                existingSubmission.Answers = answers;

                foreach (var q in model.Questions)
                {
                    var answer = answers.FirstOrDefault(a => a.UpdateTemplateQuestionId == q.QuestionId);
                    if (answer != null)
                    {
                        q.AnswerText = answer.AnswerText;
                        q.ExistingFilePath = answer.FilePath;
                        q.FileName = answer.FileName;
                    }
                    // Handle DropdownList, RadioList, Checkboxes
                    if (q.Options != null)
                    {
                        switch (q.ControlType)
                        {
                            case "DropdownList":
                                foreach (var opt in q.Options)
                                    opt.Selected = opt.Value == q.AnswerText;
                            break;

                            case "RadioList":
                                foreach (var opt in q.Options)
                                    opt.Selected = opt.Value == q.AnswerText;
                            break;

                            case "Checkboxes":
                                foreach (var opt in q.Options)
                                    opt.Selected = q.AnswerText?.Split(',').Contains(opt.Value) == true;
                            break;
                        }
                    }
                }

                var isCurrentUserReviewer = await _reviewerRepository.Table
                   .AnyAsync(r => r.UpdateSubmissionId == existingSubmission.Id &&
                   r.ReviewerCustomerId == customer.Id);

                var isCurrentUserSubmitter = existingSubmission.SubmittedByCustomerId == customer.Id;

                model.IsCurrentUserReviewer = isCurrentUserReviewer;
                model.IsCurrentUserSubmitter = isCurrentUserSubmitter;
                model.Id = existingSubmission.Id;

                var comments = await _updateSubmissionCommentRepository.Table
                    .Where(x => x.UpdateSubmissionId == existingSubmission.Id)
                    .OrderByDescending(x => x.CreatedOnUtc)
                    .ToListAsync();

                model.Comments = await PrepareCommentModels(comments);
            }
            else
            {
                // Default selection when no submission exists
                foreach (var q in model.Questions)
                {
                    if (q.Options != null && q.Options.Count > 0)
                    {
                        switch (q.ControlType)
                        {
                            case "DropdownList":
                            case "RadioList":
                                // Only set AnswerText if an option is already marked Selected
                                var selectedOption = q.Options.FirstOrDefault(o => o.Selected);
                                if (selectedOption != null)
                                    q.AnswerText = selectedOption.Value;
                            break;

                            case "Checkboxes":
                                // Only mark checkboxes that are already selected
                                var selectedValues = q.Options.Where(o => o.Selected).Select(o => o.Value).ToList();
                                if (selectedValues.Any())
                                    q.AnswerText = string.Join(",", selectedValues);
                            break;
                        }
                    }
                }
            }
            // Populate sidebar only with submitted periods
            model.Periods = allPeriods
                .Where(p => userSubmissions.Any(s => s.PeriodId == p.Id))
                .Select(p =>
                {
                    var submission = userSubmissions.FirstOrDefault(s => s.PeriodId == p.Id);
                    return new PeriodStatusModel
                    {
                        PeriodId = p.Id,
                        PeriodLabel = $"{p.PeriodStart:dd MMM, yyyy}",
                        PeriodStart = p.PeriodStart,
                        PeriodEnd = p.PeriodEnd,
                        Status = "Submitted",
                        SubmittedOnUtc = submission?.SubmittedOnUtc
                    };
                }).ToList();
            return View("~/Themes/DefaultClean/Views/Extension/UpdateForm/Submit.cshtml", model);
        }

        [HttpPost("UpdateSubmission/Submit/{id}")]
        // [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id, int periodId, SubmitUpdateFormModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            for (int i = 0; i < model.Questions.Count; i++)
            {
                var question = model.Questions[i];

                if (!string.IsNullOrEmpty(question.AnswerText))
                {
                    if (question.AnswerText.Length < question.MinLength ||
                        (question.MaxLength > 0 && question.AnswerText.Length > question.MaxLength))
                    {
                        ModelState.AddModelError($"Questions[{i}].AnswerText",
                          string.Format(await _localizationService.GetResourceAsync("UpdateForm.Validation.AnswerLength"),question.MinLength,question.MaxLength)
                        );

                    }
                }
                if (question.ControlType == "Checkboxes")
                {
                    // 1️ Get required option IDs from DB
                    var requiredOptionValues =
                        await GetRequiredOptionTextsByQuestionIdAsync(question.QuestionId);
                    // IMPORTANT: these MUST match checkbox values (Id.ToString())

                    if (requiredOptionValues.Any())
                    {
                        // 2️ Get selected checkbox values
                        var formKey = $"Questions[{i}].AnswerText";

                        var selectedValues = Request.Form[formKey]
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                            .Select(x => x.Trim())
                            .ToList();

                        // Store properly
                        question.AnswerText = string.Join(",", selectedValues);


                        // Case 1: nothing selected
                        if (!selectedValues.Any())
                        {
                            ModelState.AddModelError($"Questions[{i}].AnswerText",await _localizationService.GetResourceAsync("UpdateForm.Validation.RequiredCheckbox"));
                        }
                        else
                        {
                            // Case 2: some required options missing
                            var missingRequiredOptions = requiredOptionValues
                                .Where(req => !selectedValues.Contains(req))
                                .ToList();

                            if (missingRequiredOptions.Any())
                            {
                                ModelState.AddModelError($"Questions[{i}].AnswerText",await _localizationService.GetResourceAsync("UpdateForm.Validation.MissingRequiredCheckbox"));
                            }
                        }
                    }
                }


                if (question.ControlType == "FileUpload")
                {
                    var uploadedFile = Request.Form.Files[$"Questions[{i}].UploadedFile"];

                    if (uploadedFile != null && uploadedFile.Length > 0)
                    {
                        // Check max size
                        if (question.MaximumFileSizeKb > 0 &&
                            uploadedFile.Length > question.MaximumFileSizeKb * 1024)
                        {
                            ModelState.AddModelError($"Questions[{i}].UploadedFile",string.Format(await _localizationService.GetResourceAsync("UpdateForm.Validation.FileSizeExceeded"),question.MaximumFileSizeKb));
                        }

                        // Check allowed extensions
                        if (!string.IsNullOrWhiteSpace(question.AllowedFileExtensions))
                        {
                            var allowedExt = question.AllowedFileExtensions
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim().ToLower())
                                .ToList();

                            var fileExt = Path.GetExtension(uploadedFile.FileName).ToLower();
                            if (!allowedExt.Contains(fileExt))
                            {
                                ModelState.AddModelError($"Questions[{i}].UploadedFile",string.Format(await _localizationService.GetResourceAsync("UpdateForm.Validation.InvalidFileType"),string.Join(", ", allowedExt)));
                            }
                        }
                        if (ModelState.IsValid)
                        {
                            using (var ms = new MemoryStream())
                            {
                                await uploadedFile.CopyToAsync(ms);

                                var download = new Download
                                {
                                    DownloadGuid = Guid.NewGuid(),
                                    ContentType = uploadedFile.ContentType,
                                    Filename = Path.GetFileNameWithoutExtension(uploadedFile.FileName),
                                    Extension = Path.GetExtension(uploadedFile.FileName),
                                    DownloadBinary = ms.ToArray(),
                                    IsNew = true
                                };

                                await _downloadService.InsertDownloadAsync(download);

                                // Save Guid string as AnswerText (so you can fetch it later for download)
                                question.AnswerText = download.DownloadGuid.ToString();

                                // Store original file name in new column
                                question.FileName = $"{download.Filename}{download.Extension}";

                            }
                        }
                    }
                    else if (question.IsRequired)
                    {
                        // If required but no file uploaded
                        ModelState.AddModelError($"Questions[{i}].UploadedFile",await _localizationService.GetResourceAsync("UpdateForm.Validation.FileRequired"));
                    }
                }

            }
            if (!ModelState.IsValid || !ValidateRequiredAnswers(model))
            {
                await RebindQuestionOptionsAsync(model); // repopulate dropdowns etc.
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("UpdateForm.Notification.CorrectForm"));
                return View("~/Themes/DefaultClean/Views/Extension/UpdateForm/Submit.cshtml", model);
            }

            var now = DateTime.UtcNow;
            var templateId = model.UpdateTemplateId;
            UpdateSubmission submission = null;

            var existingSubmission = await _submissionRepository.Table
                .Where(x => x.UpdateTemplateId == templateId &&
                            x.PeriodId == periodId &&
                            x.SubmittedByCustomerId == customer.Id)
                .FirstOrDefaultAsync();

            if (existingSubmission != null)
            {
                // Update submission metadata
                existingSubmission.SubmittedOnUtc = now;
                await _submissionRepository.UpdateAsync(existingSubmission);

                // Delete old answers
                var existingAnswers = await _answerRepository.Table
                    .Where(x => x.UpdateSubmissionId == existingSubmission.Id)
                    .ToListAsync();
                //await _answerRepository.DeleteAsync(existingAnswers);
                // Add new answers
                for (int i = 0; i < model.Questions.Count; i++)
                {
                    var q = model.Questions[i];

                    if (q.ControlType == "Checkboxes")
                    {
                        var key = $"Questions[{i}].AnswerText";
                        var values = Request.Form[key];

                        q.AnswerText = string.Join(",", values);
                    }
                }


                var newAnswers = await MapAnswersAsync(model);

                foreach (var newAns in newAnswers)
                {
                    var oldAns = existingAnswers.FirstOrDefault(a =>
                        a.UpdateTemplateQuestionId == newAns.UpdateTemplateQuestionId);

                    if (oldAns != null)
                    {
                        var question = model.Questions
                            .FirstOrDefault(q => q.QuestionId == newAns.UpdateTemplateQuestionId);

                        if (question != null && question.ControlType == "FileUpload")
                        {
                            //  NEW FILE UPLOADED
                            if (!string.IsNullOrEmpty(newAns.FileName))
                            {
                                oldAns.AnswerText = newAns.AnswerText;
                                oldAns.FileName = newAns.FileName;
                                oldAns.FilePath = newAns.FilePath;
                            }
                            // else - user did NOT upload file - keep old
                        }
                        else
                        {
                            // Normal text / dropdown answers
                            oldAns.AnswerText = newAns.AnswerText;
                            oldAns.FileName = newAns.FileName;
                            oldAns.FilePath = newAns.FilePath;
                        }
                        await _answerRepository.UpdateAsync(oldAns);
                    }
                    else
                    {
                        newAns.UpdateSubmissionId = existingSubmission.Id;
                        await _answerRepository.InsertAsync(newAns);
                    }
                }
                submission = existingSubmission;
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("UpdateForm.Notification.Updated"));
                
                await NotifyReviewersOnSubmissionAsync(submission, customer, true);
            }
            else
            {
                // Step 1: Find Active Period
                var currentPeriod = await _periodRepository.GetAllAsync(q =>q.Where(p => p.UpdateTemplateId == templateId && p.PeriodStart <= now && p.PeriodEnd >= now));
                var activePeriod = currentPeriod.FirstOrDefault();
                if (activePeriod == null)
                {
                    // Try fallback to latest period instead of redirecting
                    activePeriod = await _periodRepository.Table
                        .Where(p => p.UpdateTemplateId == templateId)
                        .OrderByDescending(p => p.PeriodStart)
                        .FirstOrDefaultAsync();

                    if (activePeriod == null)
                    {
                        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("UpdateForm.Notification.NoPeriod"));
                        return RedirectToRoute("HomePage");
                    }
                }
                // Step 2: Create Submission
                submission = new UpdateSubmission
                {
                    UpdateTemplateId = templateId,
                    PeriodId = activePeriod.Id,
                    SubmittedByCustomerId = customer.Id,
                    SubmittedOnUtc = now
                };
                await _submissionRepository.InsertAsync(submission);
                // Insert answers
                var answers = await MapAnswersAsync(model);
                foreach (var ans in answers)
                {
                    ans.UpdateSubmissionId = submission.Id;
                    await _answerRepository.InsertAsync(ans);
                }
                // Assign reviewers
                var template = await _updateTemplateRepository.GetByIdAsync(templateId);
                if (!string.IsNullOrEmpty(template?.ViewerUserIds))
                {
                    var reviewerIds = template.ViewerUserIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x, out var idVal) ? idVal : 0)
                        .Where(x => x > 0)
                        .Distinct()
                        .ToList();
                    
                    foreach (var reviewerId in reviewerIds)
                    {
                        var reviewer = new UpdateSubmissionReviewer
                        {
                            UpdateSubmissionId = submission.Id,
                            ReviewerCustomerId = reviewerId,
                            //HasReviewed = false
                        };
                        await _reviewerRepository.InsertAsync(reviewer);
                       
                    }
                }
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("UpdateForm.Notification.ThankYou"));
                await NotifyReviewersOnSubmissionAsync(submission, customer, false);
            }
            // Reload the form with submitted answers
            var reloadModel = await PrepareSubmitModelAsync(templateId);
            reloadModel.PeriodId = submission.PeriodId;
            reloadModel.Id = submission.Id;
            reloadModel.HasAlreadySubmitted = true;
            reloadModel.AllowEditingAfterSubmit = true;
            // Load answers
            var answersToShow = await _answerRepository.Table
                .Where(x => x.UpdateSubmissionId == submission.Id)
                .ToListAsync();
            foreach (var q in reloadModel.Questions)
            {
                var ans = answersToShow.FirstOrDefault(a => a.UpdateTemplateQuestionId == q.QuestionId);
                if (ans != null)
                {
                    q.AnswerText = ans.AnswerText;
                    q.ExistingFilePath = ans.FilePath;
                    q.FileName = ans.FileName;
                    if (q.ControlType == "DropdownList")
                    {
                        foreach (var opt in q.Options)
                        {
                            opt.Selected = opt.Value == q.AnswerText;
                        }
                    }
                    // CHECKBOXES / READONLY CHECKBOXES
                    else if (q.ControlType == "Checkboxes" ||
                             q.ControlType == "ReadonlyCheckboxes")
                    {
                        var selectedValues = string.IsNullOrWhiteSpace(q.AnswerText)
                            ? new List<string>()
                            : q.AnswerText
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToList();

                        foreach (var opt in q.Options)
                        {
                            opt.Selected = selectedValues.Contains(opt.Value);
                        }
                    }

                }
            }
            // Load all periods
            var allPeriods = await _periodRepository.Table
                .Where(p => p.UpdateTemplateId == templateId)
                .OrderByDescending(p => p.PeriodStart)
                .ToListAsync();
            // Load user's submissions
            var userSubmissions = await _submissionRepository.Table
                .Where(x => x.UpdateTemplateId == templateId &&x.SubmittedByCustomerId == customer.Id) .ToListAsync();
            reloadModel.Periods = allPeriods
                .Where(p => userSubmissions.Any(s => s.PeriodId == p.Id))
                .Select(p =>
                {
                    var sub = userSubmissions.FirstOrDefault(s => s.PeriodId == p.Id);
                    return new PeriodStatusModel
                    {
                        PeriodId = p.Id,
                        PeriodLabel = $"{p.PeriodStart:dd MMM, yyyy}",
                        PeriodStart = p.PeriodStart,
                        PeriodEnd = p.PeriodEnd,
                        Status = "Submitted",
                        SubmittedOnUtc = sub?.SubmittedOnUtc
                    };
                }).ToList();
            var commentsToShow = await _updateSubmissionCommentRepository.Table
                                        .Where(x => x.UpdateSubmissionId == submission.Id)
                                        .OrderByDescending(x => x.CreatedOnUtc)
                                        .ToListAsync();
            reloadModel.Comments = await PrepareCommentModels(commentsToShow);
            // Return same view so user stays on form
            return View("~/Themes/DefaultClean/Views/Extension/UpdateForm/Submit.cshtml", reloadModel);
        }

        [HttpGet("UpdateSubmission/GetPeriodsByTemplate")]
        public async Task<IActionResult> GetPeriodsByTemplate(int templateId)
        {
            var periods = await _updateTemplatePeriodRepository.Table
                .Where(p => p.UpdateTemplateId == templateId)
                .OrderByDescending(p => p.PeriodStart)
                .Take(10)
                .ToListAsync();
            var result = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select Period" },
                new SelectListItem { Value = "-1", Text = "Custom Range" }
            };
            result.AddRange(periods.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.PeriodStart:MMM dd} - {p.PeriodEnd:MMM dd, yyyy}"
            }));
            return Json(result);
        }

        [Authorize]
        [HttpGet("UpdateSubmission/List")]
        public async Task<IActionResult> List(int? selectedSubmitterId, int? selectedTemplateId, int? selectedPeriodId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerId = customer.Id;

            var employee = await _employeeRepository.Table
                .Where(e => e.Customer_Id == customerId)
                .FirstOrDefaultAsync();

            int? employeeId = employee?.Id;
            var isViewer = false;
            bool hasAccess = false;

            if (selectedTemplateId.HasValue && employeeId.HasValue)
            {
                var template = await _updateTemplateRepository.GetByIdAsync(selectedTemplateId.Value);

                if (template != null)
                {
                    var empIdStr = employeeId.Value.ToString();

                    var viewerIds = (template.ViewerUserIds ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim());

                    var submitterIds = (template.SubmitterUserIds ?? "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim());

                    isViewer = viewerIds.Contains(empIdStr);
                    bool isSubmitter = submitterIds.Contains(empIdStr);

                    hasAccess = isViewer || isSubmitter;
                }
            }

            if (!isViewer)
            {
                var returnUrl = Url.Action("List", "UpdateSubmission", new
                {
                    selectedSubmitterId,
                    selectedTemplateId,
                    selectedPeriodId
                });
                return RedirectToRoute("Login", new { returnUrl });
            }
            // If user is neither viewer nor submitter Access Denied
            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Security");
            }
            var model = new UpdateSubmissionListModel
            {
                SelectedSubmitterId = selectedSubmitterId,
                SelectedTemplateId = selectedTemplateId,
                SelectedPeriodId = selectedPeriodId,
                CanSeeSubmitterDropdown = isViewer
            };
            if (isViewer && selectedTemplateId.HasValue)
            {
                var template = await _updateTemplateRepository.GetByIdAsync(selectedTemplateId.Value);
                model.TemplateTitle = template?.Title;
                var submitterIds = new List<int>();
                if (!string.IsNullOrEmpty(template?.SubmitterUserIds))
                {
                    submitterIds = template.SubmitterUserIds
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => int.TryParse(x.Trim(), out var id) ? id : (int?)null)
                        .Where(x => x.HasValue)
                        .Select(x => x.Value)
                        .ToList();
                }
                if (submitterIds.Any())
                {
                    var employees = await _employeeRepository.Table
                        .Where(e => submitterIds.Contains(e.Id))
                        .Select(e => new
                        {
                            EmployeeId = e.Id,
                            CustomerId = e.Customer_Id,
                            CustomerFirstName = e.FirstName,
                            CustomerLastName = e.LastName
                        })
                        .ToListAsync();
                    model.AvailableSubmitters = employees
                        .Select(e => new SelectListItem
                        {
                            Value = e.CustomerId.ToString(),
                            Text = $"{e.CustomerFirstName} {e.CustomerLastName}".Trim()
                        })
                        .ToList();
                    model.AvailableSubmitters.Insert(0, new SelectListItem
                    {
                        Text = "Select",
                        Value = "",
                        Selected = selectedSubmitterId == null
                    });
                    // ADD THIS: Determine not-submitted employees
                    var periodIdToCheck = selectedPeriodId ?? 0;
                    var submittedCustomerIds = await _submissionRepository.Table
                        .Where(s => s.UpdateTemplateId == selectedTemplateId.Value &&
                                    (periodIdToCheck == 0 || s.PeriodId == periodIdToCheck))
                        .Select(s => s.SubmittedByCustomerId)
                        .Distinct()
                        .ToListAsync();
                    var notSubmittedNames = employees
                        .Where(e => !submittedCustomerIds.Contains(e.CustomerId))
                        .Select(e => $"{e.CustomerFirstName} {e.CustomerLastName}".Trim())
                        .ToList();
                    model.NotSubmittedNames = notSubmittedNames;
                }
                else
                {
                    model.AvailableSubmitters = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Text = "No Submitters Found",
                            Value = ""
                        }
                    };
                }
            }
            // Load available periods if template is selected
            if (selectedTemplateId.HasValue)
            {
                var periods = await _updateTemplatePeriodRepository.Table
                    .Where(p => p.UpdateTemplateId == selectedTemplateId.Value)
                    .OrderByDescending(p => p.PeriodStart)
                    .Take(10)
                    .ToListAsync();
                model.AvailablePeriods = periods
                    .Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = $"{p.PeriodStart:MMM dd} - {p.PeriodEnd:MMM dd, yyyy}",
                        Selected = p.Id == selectedPeriodId
                    }).ToList();
                model.AvailablePeriods.Insert(0, new SelectListItem
                {
                    Value = "-1",
                    Text = "Custom Range",
                    Selected = selectedPeriodId == -1
                });
                model.AvailablePeriods.Insert(0, new SelectListItem
                {
                    Value = "",
                    Text = "Select Period",
                    Selected = selectedPeriodId == null
                });
                if (selectedPeriodId.HasValue && selectedPeriodId != -1)
                {
                    var period = periods.FirstOrDefault(p => p.Id == selectedPeriodId.Value);
                    if (period != null)
                    {
                        model.FromDate = period.PeriodStart;
                        model.ToDate = period.PeriodEnd.AddDays(1).AddTicks(-1);
                    }
                }
            }
            if (!selectedTemplateId.HasValue)
            {
                model.AvailablePeriods = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select Period", Selected = true },
                    new SelectListItem { Value = "-1", Text = "Custom Range" }
                };
                model.FromDate = DateTime.UtcNow.Date.AddDays(-7);
                model.ToDate = DateTime.UtcNow.Date;
            }
            int? filterSubmitterId = null;
            if (model.CanSeeSubmitterDropdown && model.SelectedSubmitterId.HasValue)
            {
                filterSubmitterId = model.SelectedSubmitterId;
            }
            model.Submissions = await GetSubmissionCardsAsync(
                currentUserId: customer.Id,
                filterSubmitterId: filterSubmitterId,
                from: model.FromDate,
                to: model.ToDate,
                selectedTemplateId: model.SelectedTemplateId
            );
            return View("~/Themes/DefaultClean/Views/Extension/UpdateForm/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(UpdateSubmissionCommentModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.CommentText))
                return BadRequest("Invalid comment");
            var customer = await _workContext.GetCurrentCustomerAsync();
            var comment = new UpdateSubmissionComment
            {
                UpdateSubmissionId = model.UpdateSubmissionId,
                ParentCommentId = model.ParentCommentId,
                CommentText = model.CommentText,
                CommentedByCustomerId = customer.Id,
                CreatedOnUtc = DateTime.UtcNow
            };
            await _updateSubmissionCommentService.InsertCommentAsync(comment);
            await NotifyOnNewCommentAsync(comment);
            var comments = await _updateSubmissionService.GetCommentsBySubmissionIdAsync(model.UpdateSubmissionId);
            var commentModels = await PrepareCommentModels(comments);
            return PartialView("~/Themes/DefaultClean/Views/Extension/UpdateForm/_CommentsPartial.cshtml", commentModels);
        }
        #endregion
    }
}