using App.Core;
using App.Core.Domain.Common;
using Satyanam.Plugin.Misc.AccountManagement.Domain.Enums;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class ExpenseCategory : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public string Name { get; set; }

    public string Description { get; set; }

    public int CategoryTypeId { get; set; }

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion

    #region Enum Accessors

    public ExpenseCategoryTypeEnum CategoryType
    {
        get => (ExpenseCategoryTypeEnum)CategoryTypeId;
        set => CategoryTypeId = (int)value;
    }

    #endregion
}
