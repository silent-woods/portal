using App.Data;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class SalaryComponentConfigService : ISalaryComponentConfigService
{
    #region Fields

    protected readonly IRepository<SalaryComponentConfig> _componentRepository;
    protected readonly IRepository<EmployeePayrollInfo> _payrollInfoRepository;

    #endregion

    #region Ctor

    public SalaryComponentConfigService(IRepository<SalaryComponentConfig> componentRepository,
        IRepository<EmployeePayrollInfo> payrollInfoRepository)
    {
        _componentRepository = componentRepository;
        _payrollInfoRepository = payrollInfoRepository;
    }

    #endregion

    #region Methods

    public virtual async Task<IList<SalaryComponentConfig>> GetAllActiveComponentsAsync(bool showHidden = false)
    {
        return await _componentRepository.GetAllAsync(query =>
        {
            if (!showHidden)
                query = query.Where(c => c.IsActive);
            return query.OrderBy(c => c.ComponentTypeId).ThenBy(c => c.DisplayOrder);
        }) ?? new List<SalaryComponentConfig>();
    }

    public virtual async Task<SalaryComponentConfig> GetComponentByIdAsync(int id)
    {
        return await _componentRepository.GetByIdAsync(id);
    }

    public virtual async Task InsertComponentAsync(SalaryComponentConfig component)
    {
        ArgumentNullException.ThrowIfNull(component);
        component.CreatedOnUtc = DateTime.UtcNow;
        component.UpdatedOnUtc = DateTime.UtcNow;
        await _componentRepository.InsertAsync(component);
    }

    public virtual async Task UpdateComponentAsync(SalaryComponentConfig component)
    {
        ArgumentNullException.ThrowIfNull(component);
        component.UpdatedOnUtc = DateTime.UtcNow;
        await _componentRepository.UpdateAsync(component);
    }

    public virtual async Task DeleteComponentAsync(SalaryComponentConfig component)
    {
        ArgumentNullException.ThrowIfNull(component);
        await _componentRepository.DeleteAsync(component);
    }

    public virtual async Task<EmployeePayrollInfo> GetEmployeePayrollInfoAsync(int employeeId)
    {
        var records = await _payrollInfoRepository.GetAllAsync(query =>
            query.Where(p => p.EmployeeId == employeeId));
        return records.FirstOrDefault();
    }

    public virtual async Task SaveEmployeePayrollInfoAsync(EmployeePayrollInfo info)
    {
        ArgumentNullException.ThrowIfNull(info);
        var existing = await GetEmployeePayrollInfoAsync(info.EmployeeId);
        if (existing == null)
        {
            info.CreatedOnUtc = DateTime.UtcNow;
            info.UpdatedOnUtc = DateTime.UtcNow;
            await _payrollInfoRepository.InsertAsync(info);
        }
        else
        {
            existing.CTC = info.CTC;
            existing.PanCardNumber = info.PanCardNumber;
            existing.BankName = info.BankName;
            existing.BankAccountNumber = info.BankAccountNumber;
            existing.IFSCCode = info.IFSCCode;
            existing.BeneficiaryName = info.BeneficiaryName;
            existing.UpdatedOnUtc = DateTime.UtcNow;
            await _payrollInfoRepository.UpdateAsync(existing);
        }
    }

    #endregion
}
