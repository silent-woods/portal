using App.Web.Areas.Admin.Models.Extension.Candidates;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface ICandidateModelFactory
    {
        Task<CandidateSearchModel> PrepareCandidateSearchModelAsync(CandidateSearchModel searchModel);
        Task<CandidateListModel> PrepareCandidateListModelAsync(CandidateSearchModel searchModel);
        Task<CandidateModel> PrepareCandidateModelAsync(CandidateModel model, Candidate candidate, bool excludeProperties = false);
    }
}
