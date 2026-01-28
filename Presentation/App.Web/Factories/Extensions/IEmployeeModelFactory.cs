using System.Threading.Tasks;
using App.Core.Domain.Employees;
using App.Web.Models.Employee;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the interface of the employee model factory
    /// </summary>
    public partial interface IEmployeeModelFactory
    {
        /// <summary>
        /// Prepare the employee info model
        /// </summary>
        /// <param name="model">Employee info model</param>
        /// <param name="employee">Employee</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer info model
        /// </returns>
        Task<EmployeeInfoModel> PrepareEmployeeInfoModelAsync(EmployeeInfoModel model);       
    }
}
