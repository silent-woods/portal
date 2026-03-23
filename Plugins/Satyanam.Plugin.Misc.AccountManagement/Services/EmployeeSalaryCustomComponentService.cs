using App.Data;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial class EmployeeSalaryCustomComponentService : IEmployeeSalaryCustomComponentService
{
    #region Fields

    private readonly IRepository<EmployeeSalaryCustomComponent> _repository;

    #endregion

    #region Ctor

    public EmployeeSalaryCustomComponentService(IRepository<EmployeeSalaryCustomComponent> repository)
    {
        _repository = repository;
    }

    #endregion

    #region Methods

    public virtual async Task<IList<EmployeeSalaryCustomComponent>> GetComponentsBySalaryRecordIdAsync(int salaryRecordId)
    {
        return await _repository.GetAllAsync(query =>
            query.Where(c => c.SalaryRecordId == salaryRecordId)
                 .OrderBy(c => c.CreatedOnUtc)) ?? new List<EmployeeSalaryCustomComponent>();
    }

    public virtual async Task<EmployeeSalaryCustomComponent> GetComponentByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task InsertComponentAsync(EmployeeSalaryCustomComponent component)
    {
        component.CreatedOnUtc = DateTime.UtcNow;
        await _repository.InsertAsync(component);
    }

    public virtual async Task DeleteComponentAsync(EmployeeSalaryCustomComponent component)
    {
        await _repository.DeleteAsync(component);
    }

    #endregion
}
