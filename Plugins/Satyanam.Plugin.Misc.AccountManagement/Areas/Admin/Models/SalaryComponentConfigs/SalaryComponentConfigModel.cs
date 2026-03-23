using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.SalaryComponentConfigs;

public partial record SalaryComponentConfigModel : BaseNopEntityModel
{
    #region Properties

    [Required]
    [StringLength(200)]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.Name")]
    public string Name { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.ComponentType")]
    public int ComponentTypeId { get; set; }
    public string ComponentTypeName { get; set; }
    public IList<SelectListItem> AvailableComponentTypes { get; set; } = new List<SelectListItem>();

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.IsPercentage")]
    public bool IsPercentage { get; set; }

    [Range(typeof(decimal), "0.0001", "99999999.9999", ErrorMessage = "Value must be greater than 0.")]
    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.Value")]
    public decimal Value { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.IsRemainder")]
    public bool IsRemainder { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.SalaryComponentConfig.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}
