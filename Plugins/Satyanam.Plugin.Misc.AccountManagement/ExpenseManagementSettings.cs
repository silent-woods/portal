using App.Core.Configuration;

namespace Satyanam.Plugin.Misc.AccountManagement;

public partial class ExpenseManagementSettings : ISettings
{
    #region Properties
    public int SalaryProcessingDay { get; set; }
    public int SalaryExpenseCategoryId { get; set; }
    public bool SendEmailAfterSalaryProcessing { get; set; }
    public string SalaryProcessingNotifyEmails { get; set; }
    public string CompanyName { get; set; }
    public string CompanyAddress { get; set; }
    public string CompanyCIN { get; set; } 
    public int SalaryAccountGroupId { get; set; }
    public int RecurringExpenseAccountGroupId { get; set; }
    public string HrPersonName { get; set; }
    public int HrSignaturePictureId { get; set; }
    #endregion
}
