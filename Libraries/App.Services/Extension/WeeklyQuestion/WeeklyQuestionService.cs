using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Projects;
using App.Core.Domain.result;
using App.Core.Domain.WeeklyQuestion;
using App.Data;
using App.Services.WeeklyQuestion;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Services.TimeSheets
{
    /// <summary>
    /// TimeSheet service
    /// </summary>
    public partial class WeeklyQuestionService : IWeeklyQuestionService
    {
        #region Fields

        private readonly IRepository<WeeklyQuestions> _weeklyQuestionsRepository;
        private readonly IRepository<Employee> _employeeRepository;
        private readonly IRepository<Project> _projectRepository;
        #endregion

        #region Ctor

        public WeeklyQuestionService(IRepository<WeeklyQuestions> weeklyQuestionsRepository,
            IRepository<Employee> employeeRepository,
            IRepository<Project> projectRepository
           )
        {
            _weeklyQuestionsRepository = weeklyQuestionsRepository;
            _employeeRepository = employeeRepository;
            _projectRepository = projectRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all WeeklyQuestions
        /// </summary>
        /// <param name="timeSheetName"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="showHidden"></param>
        /// <param name="overridePublished"></param>
        /// <returns></returns>
        public virtual async Task<IPagedList<WeeklyQuestions>> GetWeeklyQuestionAsync(int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false, bool? overridePublished = null)
        {

            var query = await _weeklyQuestionsRepository.GetAllAsync(async query =>
            {
                return query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.CreatedOn);
            });
            //paging
            return new PagedList<WeeklyQuestions>(query, pageIndex, pageSize);
        }


        /// <summary>
        /// Get WeeklyQuestions by id
        /// </summary>
        /// <param name="weeklyQuestionsId"></param>
        /// <returns></returns>
        public virtual async Task<WeeklyQuestions> GetWeeklyQuestionsByIdAsync(int weeklyQuestionsId)
        {
            return await _weeklyQuestionsRepository.GetByIdAsync(weeklyQuestionsId, cache => default);
        }

        /// <summary>
        /// Get WeeklyQuestions by ids
        /// </summary>
        /// <param name="weeklyQuestionsIds"></param>
        /// <returns></returns>
        public virtual async Task<IList<WeeklyQuestions>> GetWeeklyQuestionsByIdsAsync(int[] weeklyQuestionsIds)
        {
            return await _weeklyQuestionsRepository.GetByIdsAsync(weeklyQuestionsIds, cache => default, false);
        }

        /// <summary>
        /// Insert WeeklyQuestions
        /// </summary>
        /// <param name="weeklyQuestions"></param>
        /// <returns></returns>
        public virtual async Task InsertWeeklyQuestionsAsync(WeeklyQuestions weeklyQuestions)
        {
            await _weeklyQuestionsRepository.InsertAsync(weeklyQuestions);
        }

        /// <summary>
        /// Update WeeklyQuestions
        /// </summary>
        /// <param name="weeklyQuestions"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual async Task UpdateWeeklyQuestionsAsync(WeeklyQuestions weeklyQuestions)
        {
            if (weeklyQuestions == null)
                throw new ArgumentNullException(nameof(weeklyQuestions));

            await _weeklyQuestionsRepository.UpdateAsync(weeklyQuestions);
        }

        /// <summary>
        /// delete WeeklyQuestions by record
        /// </summary>
        /// <param name="weeklyQuestions"></param>
        /// <returns></returns>
        public virtual async Task DeleteWeeklyQuestionsAsync(WeeklyQuestions weeklyQuestions)
        {
            await _weeklyQuestionsRepository.DeleteAsync(weeklyQuestions, false);
        }

        public IList<WeeklyQuestions> GetAllWquestion()
        {
            return _weeklyQuestionsRepository.Table.ToList();
        }
        #endregion
    }
}