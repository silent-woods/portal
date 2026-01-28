using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;

public partial record class ProjectBillingSearchModel : BaseSearchModel
{
    #region Ctor

    public ProjectBillingSearchModel()
    {
        AvailableProjects = new List<SelectListItem>();
        AvailableCompanies = new List<SelectListItem>();
        AvailablePaymentTerms = new List<SelectListItem>();
        AvailableBillingTypes = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.SearchBillingName")]
	public string SearchBillingName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.SearchProjectId")]
    public int SearchProjectId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.SearchCompanyId")]
    public int SearchCompanyId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.SearchPaymentTermId")]
    public int SearchPaymentTermId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.SearchBillingTypeId")]
    public int SearchBillingTypeId { get; set; }

    public IList<SelectListItem> AvailableProjects { get; set; }

    public IList<SelectListItem> AvailableCompanies { get; set; }

    public IList<SelectListItem> AvailablePaymentTerms { get; set; }

    public IList<SelectListItem> AvailableBillingTypes { get; set; }

    #endregion
}
