using App.Core.Domain.JobPostings;
using App.Core.Domain.TimeSheets;
using App.Web.Areas.Admin.Models.JobPostings;
using App.Web.Areas.Admin.Models.TimeSheets;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.JobPostings
{
    /// <summary>
    /// Represents the itimesheet model factory
    /// </summary>
    public partial interface IJobPostingsModelFactory
    {
        Task<JobPostingSearchModel> PrepareJobPostingsSearchModelAsync(JobPostingSearchModel searchModel);

        Task<JobPostingListModel> PrepareJobPostingsListModelAsync
          (JobPostingSearchModel searchModel);

        Task<JobPostingModel> PrepareJobPostingsModelAsync(JobPostingModel model, JobPosting jobPosting, bool excludeProperties = false);
    }
}