using App.Web.Framework.Models;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;

public partial record EmployeeSalaryAuditLogModel : BaseNopEntityModel
{
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string ChangedByName { get; set; }
    public DateTime ChangedOnUtc { get; set; }
    public string ChangedOnDisplay => ChangedOnUtc.ToLocalTime().ToString("dd MMM yyyy hh:mm tt");
}
