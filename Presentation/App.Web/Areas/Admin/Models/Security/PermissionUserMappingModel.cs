using System.Collections.Generic;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Security;

public partial record PermissionUserMappingModel : BaseNopModel
{
    #region Ctor

    public PermissionUserMappingModel()
    {
        Users = new List<CustomerModel>();
        UserPermissions = new Dictionary<int, List<PermissionUserMappingActionModel>>();
    }

    #endregion

    #region Properties

    public IList<CustomerModel> Users { get; set; }

    public Dictionary<int, List<PermissionUserMappingActionModel>> UserPermissions { get; set; }

    #endregion
}
