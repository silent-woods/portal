using App.Core.Domain.PerformanceMeasurements;
using App.Data.Extensions;
using App.Services;
using App.Services.Helpers;
using App.Services.Media;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.InterviewQeations.Models;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Framework.Models.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Services.Recruitements;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace App.Web.Areas.Admin.InterviewQeations
{
    /// <summary>
    /// Represents store pickup point models factory implementation
    /// </summary>
    public class QuestionsModelFactory : IQuestionsModelFactory
    {
        #region Fields

        private readonly IRecruitementService _recruitementservice;
        private readonly IDownloadService _downloadService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor

        public QuestionsModelFactory(
            IRecruitementService recruitementservice,
            IDownloadService downloadService, IDateTimeHelper dateTimeHelper)
        {
            _recruitementservice = recruitementservice;
            _downloadService = downloadService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare store pickup point list model
        /// </summary>
        /// <param name="searchModel">Store pickup point search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store pickup point list model
        /// </returns>
        /// 

        public async Task<RecruitementList> PreparerecruitementListModelAsync(RecruitementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            //get student
            var question = await _recruitementservice.GetPagedReqlistAsync(Category: searchModel.Category, pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //get sort options
            var sortOptions = Enum.GetValues(typeof(CategoryEnum)).OfType<CategoryEnum>().ToList().ToPagedList(searchModel);

            var sortQeationLevelOptions = Enum.GetValues(typeof(QuestionLevelEnum)).OfType<QuestionLevelEnum>().ToList().ToPagedList(searchModel);

            var sortQeationTypeOptions = Enum.GetValues(typeof(questiontypeEnum)).OfType<questiontypeEnum>().ToList().ToPagedList(searchModel);

            var model = await new RecruitementList().PrepareToGridAsync(searchModel, question, () =>

            {
                return question.SelectAwait(async student =>
                {

                    var selectedCategoryOption = student.CategoryId;

                    var selectedqeationTypeOption = student.QuestionTypeId;

                    var selectedqeationlevelOption = student.QuestionLevelId;
                   
                    var questionmodel = new RecruitementModel();
                    questionmodel.Id = student.Id;
                    questionmodel.DownloadId = student.DownloadId;
                    questionmodel.Question = student.Question;
                    questionmodel.Question_answers = student.Question_answers;
                    questionmodel.Category =
                    ((CategoryEnum)selectedCategoryOption).ToString();

                    questionmodel.QuestionLevel = ((QuestionLevelEnum)selectedCategoryOption).ToString();

                    // questionmodel.Documents = student.Documents;

                    questionmodel.questiontype =
                   ((questiontypeEnum)selectedqeationTypeOption).ToString();

                    questionmodel.DisplayOrder = student.DisplayOrder;
                    questionmodel.Published = true;
                    questionmodel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(student.CreatedOn, DateTimeKind.Utc);
                    questionmodel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(student.UpdatedOn, DateTimeKind.Utc);
                    questionmodel.DownloadGuid = (await _downloadService.GetDownloadByIdAsync(student.DownloadId))?.DownloadGuid ?? Guid.Empty;
                    await _downloadService.GetDownloadByIdAsync(student.DownloadId);
                    return questionmodel;
                });
            });

            return model;
        }
        #endregion

        public Task<RecruitementSearchModel> PreparerecruitementSearchModelAsync(RecruitementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);

        }

        public virtual async Task<RecruitementModel> PrepareRecruitementModelAsync(RecruitementModel model, Questions questions)
        {

            //whether to fill in some of properties
            //if (model == null)


            if (questions != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = questions.ToModel<RecruitementModel>();

                }
            }
            var category = await CategoryEnum.All.ToSelectListAsync();
            foreach (var AvailableCategory in category)
                model.AvailableCategory.Add(AvailableCategory);

            // questiontypeEnum

            var questiontype = await questiontypeEnum.All.ToSelectListAsync();
            foreach (var AvailablequestiontypeId in questiontype)
                model.AvailablequestiontypeId.Add(AvailablequestiontypeId);

            //QuestionLevelEnum

            var QuestionLevel = await QuestionLevelEnum.All.ToSelectListAsync();
            foreach (var AvailableQuestionLevelId in QuestionLevel)
                model.AvailableQuestionLevelId.Add(AvailableQuestionLevelId);

            //model.AvailablequestiontypeId.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Checkout.DeliveryMethod.SelectDeliveryDay"), Value = "0" });

            return model;
        }
    }
}
