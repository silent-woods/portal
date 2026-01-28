using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.BankAccounts;

public partial record BankAccountModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.Title")]
    public string Title { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.BankName")]
    public string BankName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.AccountNo")]
    public string AccountNo { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.AccountName")]
    public string AccountName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.SwiftCode")]
    public string SwiftCode { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.IFSCCode")]
    public string IFSCCode { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.AccountType")]
    public string AccountType { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.Branch")]
    public string Branch { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.Address")]
    public string Address { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.Currency")]
    public string Currency { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.Notes")]
    public string Notes { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.IsDefault")]
    public bool IsDefault { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}
