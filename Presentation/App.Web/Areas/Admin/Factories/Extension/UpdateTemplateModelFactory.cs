using App.Core;
using App.Data.Extensions;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Extension.UpdateTemplate;
using App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion;
using App.Web.Framework.Models.Extensions;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the UpdateTemplate model factory implementation
    /// </summary>
    public partial class UpdateTemplateModelFactory : IUpdateTemplateModelFactory
    {
        #region Fields
        private readonly IUpdateTemplateService _updateTemplateService;
        private readonly IEmployeeService _employeeService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public UpdateTemplateModelFactory(IUpdateTemplateService updateTemplateService,
            IEmployeeService employeeService,
            ICustomerService customerService,
            IWorkContext workContext,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService)
        {
            _updateTemplateService = updateTemplateService;
            _employeeService = employeeService;
            _customerService = customerService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities
        public virtual async Task PrepareEmployeeListAsync(UpdateTemplateModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Get current logged-in employee
            var customer = await _workContext.GetCurrentCustomerAsync();
            int currEmployeeId = 0;

            if (!await _customerService.IsRegisteredAsync(customer))
                return; // or throw exception

            if (customer != null)
            {
                var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (currEmployee != null)
                    currEmployeeId = currEmployee.Id;
            }

            model.AvailableUser = new List<SelectListItem>
    {
        new SelectListItem { Text = "Select", Value = "0" }
    };

            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.AvailableUser.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString(),
                    Selected = (p.Id == model.CreatedByUserId || (model.CreatedByUserId == 0 && p.Id == currEmployeeId))
                });
            }

            // Set selected value (in case of create, use current; in edit, use model.CreatedByUserId)
            if (model.CreatedByUserId == 0 && currEmployeeId > 0)
                model.CreatedByUserId = currEmployeeId;
        }

        public virtual async Task<UpdateTemplateQuestionSearchModel> PrepareUpdateTemplateQuestionSearchModelAsync(int updateTemplateId)
        {
            var model = new UpdateTemplateQuestionSearchModel
            {
                UpdateTemplateId = updateTemplateId
            };

            model.AvailableControlTypes = Enum.GetValues(typeof(ControlTypeEnum))
                .Cast<ControlTypeEnum>()
                .Select(ct => new SelectListItem
                {
                    Text = ct.ToString(),
                    Value = ((int)ct).ToString()
                }).ToList();

            model.AvailableControlTypes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = ""
            });

            return model;
        }

        public virtual async Task PrepareSubmitterAndViewerUsersAsync(UpdateTemplateModel model)
        {
            var employees = await _employeeService.GetAllEmployeeNameAsync(""); // get all employees

            model.SubmitterUsers = employees.Select(emp => new SelectListItem
            {
                Text = emp.FirstName + " " + emp.LastName,
                Value = emp.Id.ToString(),
                Selected = model.SelectedSubmitterIds?.Contains(emp.Id) ?? false
            }).ToList();

            model.ViewerUsers = employees.Select(emp => new SelectListItem
            {
                Text = emp.FirstName + " " + emp.LastName,
                Value = emp.Id.ToString(),
                Selected = model.SelectedViewerIds?.Contains(emp.Id) ?? false
            }).ToList();

        }

        #endregion

        #region Methods

        public Task<UpdateTemplateSearchModel> PrepareUpdateTempleteSearchModelAsync(UpdateTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var frequencyList = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select",
                    Value = "0",
                    Selected = searchModel.FrequencyId == 0
                },
                new SelectListItem
                {
                    Text = "One time",
                    Value = ((int)UpdatedFrequency.OneTime).ToString(),
                    Selected = searchModel.FrequencyId == (int)UpdatedFrequency.OneTime
                },
                new SelectListItem
                {
                    Text = "Daily",
                    Value = ((int)UpdatedFrequency.Daily).ToString(),
                    Selected = searchModel.FrequencyId == (int)UpdatedFrequency.Daily
                },
                new SelectListItem
                {
                    Text = "Weekly",
                    Value = ((int)UpdatedFrequency.Weekly).ToString(),
                    Selected = searchModel.FrequencyId == (int)UpdatedFrequency.Weekly
                },
                new SelectListItem
                {
                    Text = "Monthly",
                    Value = ((int)UpdatedFrequency.Monthly).ToString(),
                    Selected = searchModel.FrequencyId == (int)UpdatedFrequency.Monthly
                }
            };

            searchModel.AvailableFrequencies = frequencyList;
            searchModel.SetGridPageSize();
            return Task.FromResult(searchModel);
        }

        public virtual async Task<UpdateTemplateListModel> PrepareUpdateTemplateListModelAsync(UpdateTemplateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // Get paged data using search filters
            var templates = await _updateTemplateService.GetAllUpdateTemplatesAsync(
                searchModel.Title,
                searchModel.FrequencyId,
                searchModel.IsActive,
                searchModel.DueDateTime,
                searchModel.DueTime,
                searchModel.Page - 1,
                searchModel.PageSize);

            // Prepare grid model
            var model = await ModelExtensions.PrepareToGridAsync<UpdateTemplateListModel, UpdateTemplateModel, UpdateTemplate>(
            new UpdateTemplateListModel(),
            searchModel,
            templates,
            () =>
            {
                return templates.SelectAwait(async template =>
                {
                    var itemModel = new UpdateTemplateModel
                    {
                        Id = template.Id,
                        Title = template.Title,
                        Description = template.Description,
                        FrequencyId = template.FrequencyId,
                        FrequencyName = Enum.GetName(typeof(UpdatedFrequency), template.FrequencyId),
                        RepeatEvery = template.RepeatEvery,
                        RepeatType = template.RepeatType,
                        SelectedWeekdays = template.SelectedWeekDays,
                        OnDay = template.OnDay,
                        DueDate = template.DueDate?.Date,
                        DueTime = template.DueTime,
                        SelectedSubmitterIds = template.SubmitterUserIds?
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToList() ?? new List<int>(),

                        SelectedViewerIds = template.ViewerUserIds?
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(int.Parse)
        .ToList() ?? new List<int>(),

                        IsFileAttachmentRequired = template.IsFileAttachmentRequired,
                        IsEditingAllowed = template.IsEditingAllowed,
                        ReminderBeforeMinutes = template.ReminderBeforeMinutes,
                        CreatedByUserId = template.CreatedByUserId,
                        IsActive = template.IsActive,
                        CreatedOnUTC = await _dateTimeHelper.ConvertToUserTimeAsync(template.CreatedOnUTC, DateTimeKind.Utc),
                        
                    };

                    return itemModel;
                });
            });


            return model;
        }


        public async Task<UpdateTemplateModel> PrepareUpdateTemplateModelAsync(UpdateTemplateModel model, UpdateTemplate entity, bool excludeProperties = false)
        {
            if (entity != null)
            {
                model ??= new UpdateTemplateModel();

                model.Id = entity.Id;
                model.Title = entity.Title;
                model.Description = entity.Description;
                model.FrequencyId = entity.FrequencyId;
                model.RepeatEvery = entity.RepeatEvery;
                model.RepeatType = entity.RepeatType;
                model.SelectedWeekdays = entity.SelectedWeekDays;
                model.OnDay = entity.OnDay;
                model.DueDate = entity.DueDate?.Date;
               
                model.IsFileAttachmentRequired = entity.IsFileAttachmentRequired;
                model.IsEditingAllowed = entity.IsEditingAllowed;

                model.ReminderBeforeMinutes = entity.ReminderBeforeMinutes;

                // Set helper properties for UI display
                model.ReminderBeforeDays = entity.ReminderBeforeMinutes / 1440;
                model.ReminderBeforeHours = (entity.ReminderBeforeMinutes % 1440) / 60;
                model.ReminderBeforeMinutesOnly = entity.ReminderBeforeMinutes % 60;
                
                model.IsActive = entity.IsActive;
                model.CreatedByUserId = entity.CreatedByUserId;
                model.CreatedOnUTC = entity.CreatedOnUTC;

                model.DueTime = entity.DueTime;

                model.updateTemplateQuestionSearchModel = await PrepareUpdateTemplateQuestionSearchModelAsync(entity.Id);
                model.SelectedSubmitterIds = entity.SubmitterUserIds?
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(int.Parse)
    .ToList() ?? new();

                model.SelectedViewerIds = entity.ViewerUserIds?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(int.Parse)
                    .ToList() ?? new();

            }
            await PrepareEmployeeListAsync(model);
            await PrepareSubmitterAndViewerUsersAsync(model);
            model.AvailableRepeatTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Day", Value = "Day", Selected = model.RepeatType == "Day" },
                new SelectListItem { Text = "Week", Value = "Week", Selected = model.RepeatType == "Week" },
                new SelectListItem { Text = "Month", Value = "Month", Selected = model.RepeatType == "Month" }
            };
            model.AvailableFrequencies = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Text = "Select",
                    Value = "0",
                    Selected = model.FrequencyId == 0
                },
                new SelectListItem
                {
                    Text = "One time",
                    Value = ((int)UpdatedFrequency.OneTime).ToString(),
                    Selected = model.FrequencyId == (int)UpdatedFrequency.OneTime
                },
                new SelectListItem
                {
                    Text = "Daily",
                    Value = ((int)UpdatedFrequency.Daily).ToString(),
                    Selected = model.FrequencyId == (int)UpdatedFrequency.Daily
                },
                new SelectListItem
                {
                    Text = "Weekly",
                    Value = ((int)UpdatedFrequency.Weekly).ToString(),
                    Selected = model.FrequencyId == (int)UpdatedFrequency.Weekly
                },
                new SelectListItem
                {
                    Text = "Monthly",
                    Value = ((int)UpdatedFrequency.Monthly).ToString(),
                    Selected = model.FrequencyId == (int)UpdatedFrequency.Monthly
                }
            };

            return model;
        }




        #endregion
    }
}


