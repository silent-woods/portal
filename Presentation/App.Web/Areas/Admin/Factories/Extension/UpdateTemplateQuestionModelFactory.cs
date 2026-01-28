using App.Core;
using App.Data.Extensions;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion;
using App.Web.Framework.Models.Extensions;
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
    /// Represents the UpdateTemplateQuestion model factory implementation
    /// </summary>
    public partial class UpdateTemplateQuestionModelFactory : IUpdateTemplateQuestionModelFactory
    {
        #region Fields
        private readonly IUpdateTemplateQuestionService _updateTemplateQuestionService;
        private readonly IUpdateTemplateService _updateTemplateService;
        private readonly IWorkContext _workContext;
        private readonly IUpdateQuestionOptionService _optionService;
        private readonly ILocalizationService _localizationService;
        #endregion

        #region Ctor

        public UpdateTemplateQuestionModelFactory(IWorkContext workContext,
            IUpdateTemplateQuestionService updateTemplateQuestionService,
            IUpdateTemplateService updateTemplateService,
            IUpdateQuestionOptionService optionService,
            ILocalizationService localizationService)
        {
            _workContext = workContext;
            _updateTemplateQuestionService = updateTemplateQuestionService;
            _updateTemplateService = updateTemplateService;
            _optionService = optionService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private async Task PrepareControlTypeDropdownAsync(UpdateTemplateQuestionModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            // Fill control types dropdown
            model.AvailableControlTypes = Enum.GetValues(typeof(ControlTypeEnum))
                .Cast<ControlTypeEnum>()
                .Select(ct => new SelectListItem
                {
                    Text = ct.ToString(), // You can localize this if needed
                    Value = ((int)ct).ToString(),
                    Selected = ct == (ControlTypeEnum)model.ControlTypeId
                }).ToList();
        }
        

        #endregion

        #region Methods

        public virtual async Task<UpdateTemplateQuestionSearchModel> PrepareUpdateTempleteQuestionSearchModelAsync(UpdateTemplateQuestionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<UpdateTemplateQuestionListModel> PrepareUpdateTemplateQuestionListModelAsync(UpdateTemplateQuestionSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            searchModel.AvailableControlTypes = Enum.GetValues(typeof(ControlTypeEnum))
    .Cast<ControlTypeEnum>()
    .Select(ct => new SelectListItem
    {
        Text = ct.ToString(), // or use localization here
        Value = ((int)ct).ToString()
    }).ToList();

            searchModel.AvailableControlTypes.Insert(0, new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("Admin.Common.All"),
                Value = ""
            });

            // Fix: convert controlTypeId to null if it's 0
            int? controlTypeId = searchModel.ControlType > 0 ? searchModel.ControlType : null;

            // Get paged data using search filters
            var templates = await _updateTemplateQuestionService.GetAllUpdateTemplateQuestionAsync(
                UpdateTemplateId:searchModel.UpdateTemplateId,
                question: searchModel.Question,
                isRequired: searchModel.IsRequired,
                controlTypeId: controlTypeId,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = await new UpdateTemplateQuestionListModel().PrepareToGridAsync(searchModel, templates, () =>
            {
                return templates.SelectAwait(async weeklyQuestions =>
                {
                    var updateTemplate = await _updateTemplateService.GetByIdAsync(weeklyQuestions.UpdateTemplateId);

                    var updateTemplateQuestionModel = new UpdateTemplateQuestionModel();

                    var itemModel = new UpdateTemplateQuestionModel
                    {
                        Id = weeklyQuestions.Id,
                        UpdateTemplateId = weeklyQuestions.UpdateTemplateId,
                        UpdateTemplateTitle = updateTemplate?.Title,
                        QuestionText = weeklyQuestions.QuestionText,
                        ControlTypeId = weeklyQuestions.ControlTypeId,
                        ControlTypeName = Enum.GetName(typeof(ControlTypeEnum), weeklyQuestions.ControlTypeId),
                        IsRequired = weeklyQuestions.IsRequired,
                        DisplayOrder = weeklyQuestions.DisplayOrder,
                    };

                    return itemModel;
                });
            });

            return model;
        }


        public async Task<UpdateTemplateQuestionModel> PrepareUpdateTemplateQuestionModelAsync(UpdateTemplateQuestionModel model, UpdateTemplateQuestion updateTemplateQuestion, bool excludeProperties = false)
        {
            if (updateTemplateQuestion != null)
            {
                if (model == null)
                {


                    model ??= new UpdateTemplateQuestionModel();

                    model.Id = updateTemplateQuestion.Id;
                    model.UpdateTemplateId = updateTemplateQuestion.UpdateTemplateId;
                    model.QuestionText = updateTemplateQuestion.QuestionText;
                    model.ControlTypeId = updateTemplateQuestion.ControlTypeId;
                    model.IsRequired = updateTemplateQuestion.IsRequired;
                    model.DisplayOrder = updateTemplateQuestion.DisplayOrder;

                    model.ValidationMinLength = updateTemplateQuestion.ValidationMinLength;
                    model.ValidationMaxLength = updateTemplateQuestion.ValidationMaxLength;
                    model.ValidationFileMaximumSize = updateTemplateQuestion.ValidationFileMaximumSize;
                    model.ValidationFileAllowedExtensions = updateTemplateQuestion.ValidationFileAllowedExtensions;
                    model.DefaultValue = updateTemplateQuestion.DefaultValue;
                }
            }
            await PrepareControlTypeDropdownAsync(model);
            //await PrepareUpdateTemplateDropdownAsync(model);
            return model;
        }




        #endregion
    }
}


