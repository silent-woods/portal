using App.Core.Domain.Employees;
using App.Web.Models.Employee;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the Assets model factory
    /// </summary>
    public partial interface IEmployeeAssetsModelFactory
    {
        Task<EmployeeAssetsModel> PrepareEmployeeAssetsModelAsync(EmployeeAssetsModel model, Assets assets);
    }
}