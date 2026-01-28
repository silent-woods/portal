using App.Core.Domain.WeeklyQuestion;
using App.Web.Areas.Admin.Models.WeeklyQuestions;
using System.Threading.Tasks;



namespace App.Web.Areas.Admin.WeeklyQuestion
{
    /// <summary>
    /// Represents the store pickup point models factory
    /// </summary>
    public interface IWeeklyQuestionsModelFactory
    {
        Task<WeeklyQuestionsSearchModel> PrepareWeeklyQuestionsSearchModelAsync(WeeklyQuestionsSearchModel searchModel);

        Task<WeeklyQuestionsListModel> PrepareWeeklyQuestionsListModelAsync(WeeklyQuestionsSearchModel searchModel);

       Task<WeeklyQuestionsModel> PrepareWeeklyQuestionsModelAsync(WeeklyQuestionsModel model, WeeklyQuestions weeklyQuestions);

    }
}