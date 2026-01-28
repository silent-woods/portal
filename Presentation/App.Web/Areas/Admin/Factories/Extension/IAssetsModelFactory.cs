using App.Core.Domain.Employees;
using App.Web.Areas.Admin.Models.Employees;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Assets model factory
    /// </summary>
    public partial interface IAssetsModelFactory
    {
        Task<AssetsSearchModel> PrepareAssetsSearchModelAsync(AssetsSearchModel searchModel);
        Task<AssetsListModel> PrepareAssetsListModelAsync(AssetsSearchModel searchModel,Employee employee);

        Task<AssetsModel> PrepareAssetsModelAsync(AssetsModel model, Assets assets, bool excludeProperties = false);
    }
}