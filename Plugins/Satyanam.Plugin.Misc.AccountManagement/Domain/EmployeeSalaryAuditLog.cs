using App.Core;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class EmployeeSalaryAuditLog : BaseEntity
{
    #region Properties

    public int EmployeeSalaryRecordId { get; set; }
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public int ChangedByEmployeeId { get; set; }
    public DateTime ChangedOnUtc { get; set; }

    #endregion
}
