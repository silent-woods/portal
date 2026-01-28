using App.Web.Areas.Admin.Models.Extension.CheckLists;
using Satyanam.Nop.Core.Domains;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the CheckListMaster model factory interface
    /// </summary>
    public partial interface ICheckListMasterModelFactory
    {
        /// <summary>
        /// Prepare search model
        /// </summary>
        Task<CheckListMasterSearchModel> PrepareCheckListMasterSearchModelAsync(CheckListMasterSearchModel searchModel);

        /// <summary>
        /// Prepare list model
        /// </summary>
        Task<CheckListMasterListModel> PrepareCheckListMasterListModelAsync(CheckListMasterSearchModel searchModel);

        /// <summary>
        /// Prepare model for create/edit
        /// </summary>
        Task<CheckListMasterModel> PrepareCheckListMasterModelAsync(CheckListMasterModel model, CheckListMaster entity, bool excludeProperties = false);
    }
}
