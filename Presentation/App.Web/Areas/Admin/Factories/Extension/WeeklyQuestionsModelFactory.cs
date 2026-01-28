using App.Core.Domain.Designations;
using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.WeeklyQuestion;
using App.Data.Extensions;
using App.Services;
using App.Services.Designations;
using App.Services.Helpers;
using App.Services.WeeklyQuestion;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using App.Web.Areas.Admin.Models.WeeklyQuestions;
using App.Web.Areas.Admin.WeeklyQuestion;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the timesheet model factory implementation
    /// </summary>
    public partial class WeeklyQuestionsModelFactory : IWeeklyQuestionsModelFactory
    {
        #region Fields
        private readonly IWeeklyQuestionService _weeklyQuestionService;
        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public WeeklyQuestionsModelFactory(IWeeklyQuestionService weeklyQuestionService, IDesignationService designationService, IDateTimeHelper dateTimeHelper)
        {
            _weeklyQuestionService = weeklyQuestionService;
            _designationService = designationService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion
        #region Utilities

        #endregion

        #region Methods

        public virtual async Task<WeeklyQuestionsSearchModel> PrepareWeeklyQuestionsSearchModelAsync(WeeklyQuestionsSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<WeeklyQuestionsListModel> PrepareWeeklyQuestionsListModelAsync
          (WeeklyQuestionsSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get weeklyQuestion
            var weeklyQuestion = await _weeklyQuestionService.GetWeeklyQuestionAsync(
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new WeeklyQuestionsListModel().PrepareToGridAsync(searchModel, weeklyQuestion, () =>
            {
                return weeklyQuestion.SelectAwait(async weeklyQuestions =>
                {
                    //fill in model values from the entity
                    var selectedAvailableControlTypeOption = weeklyQuestions.ControlTypeId;

                    var weeklyQuestionsModel = weeklyQuestions.ToModel<WeeklyQuestionsModel>();
                    weeklyQuestionsModel.ControlType = ((ControlType)selectedAvailableControlTypeOption).ToString();
                    weeklyQuestionsModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(weeklyQuestions.CreatedOn, DateTimeKind.Utc);
                    Designation designation = new Designation();
                    designation = await _designationService.GetDesignationByIdAsync(weeklyQuestionsModel.DesignationId);
                    weeklyQuestionsModel.DesignationName = designation.Name;

                    return weeklyQuestionsModel;
                });
            });
            //prepare grid model
            return model;
        }
        public virtual async Task<WeeklyQuestionsModel> PrepareWeeklyQuestionsModelAsync(WeeklyQuestionsModel model, WeeklyQuestions weeklyQuestions)
        {
            var controlType = await ControlType.Select.ToSelectListAsync();
            if (weeklyQuestions != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = weeklyQuestions.ToModel<WeeklyQuestionsModel>();
                }
            }
            model.ControlTyeps = controlType.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.ControlTypeId.ToString() == store.Value
            }).ToList();

            model.Designations.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var designationsName = "";
            var designation = await _designationService.GetAllDesignationAsync(designationsName);
            foreach (var p in designation)
            {
                model.Designations.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }

            return model;
        }
        #endregion
    }
}
