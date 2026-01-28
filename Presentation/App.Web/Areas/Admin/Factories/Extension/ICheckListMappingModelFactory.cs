using App.Web.Areas.Admin.Models.CheckListMappings;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the checklist mapping model factory
    /// </summary>
    public partial interface ICheckListMappingModelFactory
    {
        Task<CheckListMappingSearchModel> PrepareCheckListMappingSearchModelAsync(CheckListMappingSearchModel searchModel);

        Task<CheckListMappingListModel> PrepareCheckListMappingListModelAsync(CheckListMappingSearchModel searchModel);

        Task<CheckListMappingModel> PrepareCheckListMappingModelAsync(CheckListMappingModel model,
            CheckListMapping checkListMapping,
            bool excludeProperties = false);
    }
}
