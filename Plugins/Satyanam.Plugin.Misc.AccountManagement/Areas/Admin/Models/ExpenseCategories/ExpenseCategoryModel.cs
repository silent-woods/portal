using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ExpenseCategories;

public partial record ExpenseCategoryModel : BaseNopEntityModel
{
    #region Properties

    [Required]
    [StringLength(200)]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Fields.Description")]
    public string Description { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Fields.CategoryType")]
    public int CategoryTypeId { get; set; }
    public string CategoryTypeName { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.ExpenseCategory.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }
    public IList<SelectListItem> AvailableCategoryTypes { get; set; } = new List<SelectListItem>();

    #endregion
}
