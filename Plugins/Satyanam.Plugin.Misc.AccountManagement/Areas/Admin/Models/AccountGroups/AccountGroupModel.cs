using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountGroups;

public partial record AccountGroupModel : BaseNopEntityModel
{
    #region Ctor

    public AccountGroupModel()
    {
        AccountCategories = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Fields.Name")]
    public string Name { get; set; }

    public int AccountCategoryId { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Fields.AccountCategory")]
    public string AccountCategory { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    public IList<SelectListItem> AccountCategories { get; set; }

    #endregion
}
