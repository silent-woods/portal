namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;

public class HdfcCsvRowModel
{
    public string TransactionType { get; set; }
    public int SeqNo { get; set; }
    public string AccountNumber { get; set; }
    public string Amount { get; set; }
    public string BeneficiaryName { get; set; }
    public string Narration { get; set; } 
    public string PaymentDate { get; set; }
    public string IFSCCode { get; set; }
    public string Email { get; set; }
}
