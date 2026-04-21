namespace Satyanam.Plugin.Misc.AccountManagement;

public static partial class AccountManagementDefaults
{
    #region Static System Names for Defult Billing Rate

    public const int BillingRate = 5;

    #endregion

    #region Static System Names for Message Templates

    public const string SendInvoiceNotification = "Invoice.CustomerNotification";

    public const string SendInvoiceReminderNotification = "InvoiceReminder.CustomerNotification";

    public const string SendInvoiceSubject = "Your Invoice is ready.";

    #endregion

    #region Static System Names for a Invoice Section

    public static string Test = "test";

    public static string CompanyName = "Satyanam Info Solutions Pvt Ltd";

    public static string CompanyAddress = "601, Satyamev Eminence, Science City, 380060, Ahmedabad, India";

    public static string CompanyWebsite = "https://www.satyanamsoft.com/";

    #endregion

    #region Static System Names for a ZIP

    public static string InvoiceAndTimesheet = "Invoice.zip";

    public static string Invoice = "Invoice";

    public static string TimeSummaryReport = "Time Report";

    #endregion

    #region Static System Name for a Invoice Reminder

    public const string InvoiceReminderScheduleTaskType = "Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.ScheduleTasks.InvoiceReminderMailTask";

    #endregion
}
