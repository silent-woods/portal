using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Configuration;

public partial record ConfigurationModel : BaseNopEntityModel
{
    #region Ctor

    public ConfigurationModel()
    {
        AvailableExpenseCategories = new List<SelectListItem>();
        AvailableAccountGroups = new List<SelectListItem>();
        AvailableHrEmployees = new List<SelectListItem>();
        AvailableFinancialYearStartMonths = new List<SelectListItem>();
        AvailableEmailAccounts = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.EnablePlugin")]
    public bool EnablePlugin { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.InvoiceNumber")]
    public int InvoiceNumber { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.InvoiceLogoId")]
    public int InvoiceLogoId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.QARate")]
    public decimal QARate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.EmailAccountId")]
    public int EmailAccountId { get; set; }

    public IList<SelectListItem> AvailableEmailAccounts { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.SalaryProcessingDay")]
    public int SalaryProcessingDay { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.SalaryExpenseCategoryId")]
    public int SalaryExpenseCategoryId { get; set; }
    public IList<SelectListItem> AvailableExpenseCategories { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.SalaryAccountGroupId")]
    public int SalaryAccountGroupId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.RecurringExpenseAccountGroupId")]
    public int RecurringExpenseAccountGroupId { get; set; }

    public IList<SelectListItem> AvailableAccountGroups { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.CompanyName")]
    public string CompanyName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.CompanyAddress")]
    public string CompanyAddress { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.CompanyCIN")]
    public string CompanyCIN { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.HrPersonName")]
    public string HrPersonName { get; set; }

    [UIHint("Picture")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.HrSignaturePictureId")]
    public int HrSignaturePictureId { get; set; }

    public IList<SelectListItem> AvailableHrEmployees { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Configuration.Fields.FinancialYearStartMonth")]
    public int FinancialYearStartMonth { get; set; }
    public IList<SelectListItem> AvailableFinancialYearStartMonths { get; set; }

    #endregion
}
