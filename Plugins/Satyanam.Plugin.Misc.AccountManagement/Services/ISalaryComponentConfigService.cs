using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial interface ISalaryComponentConfigService
{
    Task<IList<SalaryComponentConfig>> GetAllActiveComponentsAsync(bool showHidden = false);

    Task<SalaryComponentConfig> GetComponentByIdAsync(int id);

    Task InsertComponentAsync(SalaryComponentConfig component);

    Task UpdateComponentAsync(SalaryComponentConfig component);

    Task DeleteComponentAsync(SalaryComponentConfig component);

    Task<EmployeePayrollInfo> GetEmployeePayrollInfoAsync(int employeeId);

    Task SaveEmployeePayrollInfoAsync(EmployeePayrollInfo info);
}
