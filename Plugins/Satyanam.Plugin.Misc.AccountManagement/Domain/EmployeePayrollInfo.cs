using App.Core;
using System;

namespace Satyanam.Plugin.Misc.AccountManagement.Domain;

public partial class EmployeePayrollInfo : BaseEntity
{
    #region Properties

    public int EmployeeId { get; set; }
    public string CTC { get; set; }
    public string PanCardNumber { get; set; }
    public string BankName { get; set; }
    public string BankAccountNumber { get; set; }
    public string IFSCCode { get; set; }
    public string BeneficiaryName { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime UpdatedOnUtc { get; set; }

    #endregion
}
