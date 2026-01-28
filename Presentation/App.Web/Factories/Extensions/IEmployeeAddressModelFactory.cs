using App.Core.Domain.Employees;
using App.Web.Models.Employee;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the Address model factory
    /// </summary>
    public partial interface IEmployeeAddressModelFactory
    {
        Task<EmployeeAddressModel> PrepareAddressAsync(EmployeeAddressModel model, EmpAddress address);
    }
}