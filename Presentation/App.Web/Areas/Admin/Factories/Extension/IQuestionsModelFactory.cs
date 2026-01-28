using App.Web.Areas.Admin.InterviewQeations.Models;
using Nop.Core.Domain.Catalog;
using System.Threading.Tasks;



namespace App.Web.Areas.Admin.InterviewQeations
{
    /// <summary>
    /// Represents the store pickup point models factory
    /// </summary>
    public interface IQuestionsModelFactory
    {
        /// <summary>
        /// Prepare store pickup point list model
        /// </summary>
        /// <param name="searchModel">Store pickup point search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the store pickup point list model
        /// </returns>
        //Task<RecruitementList> PreparerecruitementListModelAsync();
        Task<RecruitementList> PreparerecruitementListModelAsync(RecruitementSearchModel searchModel);
        Task<RecruitementSearchModel> PreparerecruitementSearchModelAsync(RecruitementSearchModel searchModel);
        /// <summary>
        /// Prepare Recruitement model
        /// </summary>
        /// <param name="model">Recruitement model</param>
        /// <param name="Recruitement">Recruitement</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the category model
        /// </returns>
        // Task PrepareRecruitementModelAsync(Questions questions, RecruitementModel model);
        //Task PrepareRecruitementModelAsync(RecruitementModel model);
        Task<RecruitementModel> PrepareRecruitementModelAsync(RecruitementModel model, Questions questions);



    }
}