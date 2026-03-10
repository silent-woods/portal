using App.Web.Areas.Admin.Models.Extension.Technologys;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface ITechnologyModelFactory
    {
        Task<TechnologySearchModel> PrepareTechnologySearchModelAsync(TechnologySearchModel searchModel);
        Task<TechnologyListModel> PrepareTechnologyListModelAsync(TechnologySearchModel searchModel);
        Task<TechnologyModel> PrepareTechnologyModelAsync(TechnologyModel model, Technology technology, bool excludeProperties = false);

    }
}
