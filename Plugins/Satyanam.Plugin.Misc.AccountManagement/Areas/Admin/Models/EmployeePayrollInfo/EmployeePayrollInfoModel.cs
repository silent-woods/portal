using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeePayrollInfo;

public partial record EmployeePayrollInfoModel : BaseNopModel
{
    public int EmployeeId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.Fields.CTC")]
    public string CTC { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.Fields.PanCardNumber")]
    public string PanCardNumber { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.Fields.BankName")]
    public string BankName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.Fields.BankAccountNumber")]
    public string BankAccountNumber { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.Fields.IFSCCode")]
    public string IFSCCode { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeePayrollInfo.Fields.BeneficiaryName")]
    public string BeneficiaryName { get; set; }
}
