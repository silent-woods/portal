using App.Core;
using Satyanam.Plugin.Misc.AccountManagement.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Satyanam.Plugin.Misc.AccountManagement.Services;

public partial interface IEmployeeSalaryService
{
    Task InsertSalaryRecordAsync(EmployeeMonthlySalary salaryRecord);

    Task UpdateSalaryRecordAsync(EmployeeMonthlySalary salaryRecord);

    Task DeleteSalaryRecordAsync(EmployeeMonthlySalary salaryRecord);

    Task<EmployeeMonthlySalary> GetSalaryRecordByIdAsync(int id);

    Task<EmployeeMonthlySalary> GetSalaryRecordAsync(int employeeId, int monthId, int yearId);

    Task<EmployeeMonthlySalary> GetSalaryRecordByAccountTransactionIdAsync(int accountTransactionId);

    Task<IPagedList<EmployeeMonthlySalary>> GetAllSalaryRecordsAsync(int employeeId = 0, int monthId = 0, int yearId = 0, int statusId = 0, string searchEmployeeIds = null, int pageIndex = 0, int pageSize = int.MaxValue);

    Task ProcessMonthlySalariesAsync(int monthId, int yearId, int submittedByEmployeeId = 0);

    Task FinalizeSalaryRecordAsync(EmployeeMonthlySalary salaryRecord, int submittedByEmployeeId);

    Task MarkSalaryAsPaidAsync(EmployeeMonthlySalary salaryRecord, int submittedByEmployeeId);

    Task InsertSalaryAuditLogAsync(EmployeeSalaryAuditLog auditLog);

    Task<IList<EmployeeSalaryAuditLog>> GetSalaryAuditLogsAsync(int salaryRecordId);
}
