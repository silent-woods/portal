using System.Collections.Generic;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Security
{
    /// <summary>
    /// Represents a permission mapping model
    /// </summary>
    public partial record PermissionMappingModel : BaseNopModel
    {
        #region Ctor

        public PermissionMappingModel()
        {
            AvailablePermissions = new List<PermissionRecordModel>();
            AvailableCustomerRoles = new List<CustomerRoleModel>();
            Allowed = new Dictionary<string, IDictionary<int, bool>>();
            ActionPermissions = new Dictionary<string, Dictionary<int, HashSet<string>>>();
            AvailableActions = new List<string>();
        }

        #endregion

        #region Properties

        public IList<PermissionRecordModel> AvailablePermissions { get; set; }

        public IList<CustomerRoleModel> AvailableCustomerRoles { get; set; }

        //[permission system name] / [customer role id] / [allowed]
        public IDictionary<string, IDictionary<int, bool>> Allowed { get; set; }

        public Dictionary<string, Dictionary<int, HashSet<string>>> ActionPermissions { get; set; }

        public IList<string> AvailableActions { get; set; }

        #endregion
    }
}