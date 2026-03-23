using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial interface IEmployeeSalaryCustomComponentService
{
    Task<IList<EmployeeSalaryCustomComponent>> GetComponentsBySalaryRecordIdAsync(int salaryRecordId);
    Task<EmployeeSalaryCustomComponent> GetComponentByIdAsync(int id);
    Task InsertComponentAsync(EmployeeSalaryCustomComponent component);
    Task DeleteComponentAsync(EmployeeSalaryCustomComponent component);
}
