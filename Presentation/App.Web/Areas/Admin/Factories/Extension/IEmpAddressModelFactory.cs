using App.Core.Domain.Employees;
using App.Web.Areas.Admin.Models.Employees;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Address model factory
    /// </summary>
    public partial interface IEmpAddressModelFactory
    {
        Task<EmpAddressSearchModel> PrepareAddressSearchModelAsync(EmpAddressSearchModel searchModel,EmpAddress empAddress);
        Task<EmpAddressListModel> PrepareAddressListModelAsync(EmpAddressSearchModel searchModel,Employee employee);

        Task<EmpAddressModel> PrepareAddressModelAsync(EmpAddressModel model, EmpAddress address, bool excludeProperties = false);
    }
}