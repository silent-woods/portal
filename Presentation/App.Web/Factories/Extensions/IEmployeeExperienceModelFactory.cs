using App.Core.Domain.Employees;
using App.Web.Models.Employee;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the Experience model factory
    /// </summary>
    public partial interface IEmployeeExperienceModelFactory
    {
        Task<EmployeeExperienceModel> PrepareEmployeeExperienceModelAsync(EmployeeExperienceModel model);
    }
}