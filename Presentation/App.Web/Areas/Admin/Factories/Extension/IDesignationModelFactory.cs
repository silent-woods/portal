using App.Core.Domain.Designations;
using App.Web.Areas.Admin.Models.Designation;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial interface IDesignationModelFactory
    {
        
        Task<DesignationSearchModel> PrepareDesignationSearchModelAsync(DesignationSearchModel searchModel);

        Task<DesignationListModel> PrepareDesignationListModelAsync(DesignationSearchModel searchModel);

        Task<DesignationModel> PrepareDesignationModelAsync(DesignationModel model, Designation designations, bool excludeProperties = false);
    }
}