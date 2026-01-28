using App.Core.Domain.ManageResumes;
using App.Core.Domain.result;
using App.Web.Areas.Admin.Models.ManageResumes;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Manageresumes
{
    /// <summary>
    /// Represents the CandiatesResumes model factory
    /// </summary>
    public partial interface ICandiatesResumesModelFactory
    {
        Task<CandiatesResumesSearchModel> PrepareCandiatesResumesSearchModelAsync(CandiatesResumesSearchModel searchModel);

        Task<CandiatesResumesListModel> PrepareCandiatesResumesListModelAsync
         (CandiatesResumesSearchModel searchModel);

        Task<CandiatesResumesModel> PrepareCandiatesResumesModelAsync(CandiatesResumesModel model, CandidatesResumes  candiatesResumes, bool excludeProperties = false);
        Task<CandiatesResumesModel> PrepareCandiatesResultModelAsync(CandiatesResumesModel model, CandidatesResult candiatesResumes, bool excludeProperties = false);
        Task<CandiatesResumesModel> PrepareResultModelAsync(int Id);
    }
}