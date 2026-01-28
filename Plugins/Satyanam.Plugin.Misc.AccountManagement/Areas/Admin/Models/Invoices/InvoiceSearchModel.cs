using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;

public partial record InvoiceSearchModel : BaseSearchModel
{
	#region Ctor

	public InvoiceSearchModel()
	{
        AvailableCompanies = new List<SelectListItem>();
        AvailableStatuses = new List<SelectListItem>();
        AvailableMonths = new List<SelectListItem>();
        AvailableYears = new List<SelectListItem>();
	}

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchCreatedOnFrom")]
    [UIHint("DateNullable")]
    public DateTime? SearchCreatedOnFrom { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchCreatedOnTo")]
    [UIHint("DateNullable")]
    public DateTime? SearchCreatedOnTo { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchCompanyId")]
    public int SearchCompanyId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchStatusId")]
    public int SearchStatusId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchMonthId")]
    public int SearchMonthId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchYearId")]
    public int SearchYearId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.SearchInvoiceNumber")]
    public int SearchInvoiceNumber { get; set; }

    public IList<SelectListItem> AvailableCompanies { get; set; }

    public IList<SelectListItem> AvailableStatuses { get; set; }

    public IList<SelectListItem> AvailableMonths { get; set; }

    public IList<SelectListItem> AvailableYears { get; set; }

    #endregion
}
