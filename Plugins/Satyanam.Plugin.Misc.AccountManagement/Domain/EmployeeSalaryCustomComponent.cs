using App.Core;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class EmployeeSalaryCustomComponent : BaseEntity
{
    #region Properties

    public int SalaryRecordId { get; set; }

    public int TypeId { get; set; }

    public string Name { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    #endregion

    #region Enum Accessors

    public SalaryCustomComponentType ComponentType
    {
        get => (SalaryCustomComponentType)TypeId;
        set => TypeId = (int)value;
    }

    #endregion
}

public enum SalaryCustomComponentType
{
    Addition = 1,
    Deduction = 2
}
