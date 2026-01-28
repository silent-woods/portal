using App.Core;
using App.Core.Domain.Common;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class BankAccount : BaseEntity, ISoftDeletedEntity
{
    #region Properties

    public string Title { get; set; }

    public string BankName { get; set; }

    public string AccountNo { get; set; }

    public string AccountName { get; set; }

    public string SwiftCode { get; set; }

    public string IFSCCode { get; set; }

    public string AccountType { get; set; }

    public string Branch { get; set; }

    public string Address { get; set; }

    public string Currency { get; set; }

    public string Notes { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public bool Deleted { get; set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
