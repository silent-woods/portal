using App.Core;
using App.Core.Domain.WeeklyQuestion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace App.Services.WeeklyQuestion
{
    /// <summary>
    /// WeeklyQuestions service interface
    /// </summary>
    public partial interface IWeeklyQuestionService
    {
        /// <summary>
        /// Gets all WeeklyQuestions
        /// </summary>
        Task<IPagedList<WeeklyQuestions>> GetWeeklyQuestionAsync( int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null);

        /// <summary>
        /// Get WeeklyQuestions by id
        /// </summary>
        /// <param name="weeklyQuestionsId"></param>
        /// <returns></returns>
        Task<WeeklyQuestions> GetWeeklyQuestionsByIdAsync(int weeklyQuestionsId);

        /// <summary>
        /// Insert weeklyQuestions
        /// </summary>
        /// <param name="weeklyQuestions"></param>
        /// <returns></returns>
        Task InsertWeeklyQuestionsAsync(WeeklyQuestions weeklyQuestions);

        /// <summary>
        /// Update WeeklyQuestions
        /// </summary>
        /// <param name="weeklyQuestions"></param>
        /// <returns></returns>
        Task UpdateWeeklyQuestionsAsync(WeeklyQuestions weeklyQuestions);

        /// <summary>
        /// Delete WeeklyQuestions
        /// </summary>
        /// <param name="weeklyQuestions"></param>
        /// <returns></returns>
        Task DeleteWeeklyQuestionsAsync(WeeklyQuestions weeklyQuestions);

        /// <summary>
        /// Get WeeklyQuestions by ids
        /// </summary>
        /// <param name="weeklyQuestionsIds"></param>
        /// <returns></returns>
        Task<IList<WeeklyQuestions>> GetWeeklyQuestionsByIdsAsync(int[] weeklyQuestionsIds);
        IList<WeeklyQuestions> GetAllWquestion();
    }
}