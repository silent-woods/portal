using App.Core.Domain.Employees;
using App.Web.Models.Employee;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the customer model factory
    /// </summary>
    public partial interface IEmployeeEducationModelFactory
    {
        Task<EmployeeEducationModel> PrepareEmployeeEducationModelAsync(EmployeeEducationModel model, Education education);
    }
}