using App.Core;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class SalaryComponentConfig : BaseEntity
{
    #region Properties

    public string Name { get; set; }

    public int ComponentTypeId { get; set; }

    public bool IsPercentage { get; set; }

    public decimal Value { get; set; }

    public bool IsRemainder { get; set; }

    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }

    #endregion

    #region Enum Accessors

    public SalaryComponentTypeEnum ComponentType
    {
        get => (SalaryComponentTypeEnum)ComponentTypeId;
        set => ComponentTypeId = (int)value;
    }

    #endregion
}
