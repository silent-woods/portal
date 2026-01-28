using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;

public partial record ProjectBillingModel : BaseNopEntityModel
{
    #region Ctor

    public ProjectBillingModel()
    {
        AvailableProjects = new List<SelectListItem>();
        AvailableCompanies = new List<SelectListItem>();
        AvailablePaymentTerms = new List<SelectListItem>();
        AvailableBillingTypes = new List<SelectListItem>();
        AvailableCurrencies = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingName")]
    public string BillingName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.ProjectId")]
    public int ProjectId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.ProjectId")]
    public string ProjectName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.CompanyId")]
    public int CompanyId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.CompanyId")]
    public string CompanyName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentTermId")]
    public int PaymentTermId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentTermId")]
    public string PaymentTerm { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingTypeId")]
    public int BillingTypeId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingTypeId")]
    public string BillingType { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingRate")]
    public int BillingRate { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PrimaryCurrencyId")]
    public int PrimaryCurrencyId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentCurrencyId")]
    public int PaymentCurrencyId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public IList<SelectListItem> AvailableProjects { get; set; }

    public IList<SelectListItem> AvailableCompanies { get; set; }

    public IList<SelectListItem> AvailablePaymentTerms { get; set; }

    public IList<SelectListItem> AvailableBillingTypes { get; set; }

    public IList<SelectListItem> AvailableCurrencies { get; set; }

    #endregion
}
