using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.EmployeeSalaries;

public partial record EmployeeSalarySearchModel : BaseSearchModel
{
    #region Ctor

    public EmployeeSalarySearchModel()
    {
        AvailableMonths = new List<SelectListItem>();
        AvailableYears = new List<SelectListItem>();
        AvailableStatuses = new List<SelectListItem>();
        AvailableEmployees = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Search.Month")]
    public int SearchMonthId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Search.Year")]
    public int SearchYearId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Search.Status")]
    public int SearchStatusId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.EmployeeSalary.Search.Employees")]
    public string SearchEmployeeIds { get; set; }

    public IList<SelectListItem> AvailableMonths { get; set; }
    public IList<SelectListItem> AvailableYears { get; set; }
    public IList<SelectListItem> AvailableStatuses { get; set; }
    public IList<SelectListItem> AvailableEmployees { get; set; }

    #endregion
}
