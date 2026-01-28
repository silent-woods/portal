using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Security
{
    public partial record PermissionUserRecordModel : BaseNopEntityModel
    {
        #region Ctor

        public PermissionUserRecordModel()
        {
            AvailableUsers = new List<SelectListItem>();
            AvailablePermissions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.PermissionId")]
        public int PermissionId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.UserId")]
        public int UserId { get; set; }

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.Add")]
        public bool Add { get; set; }

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.Edit")]
        public bool Edit { get; set; }

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.Delete")]
        public bool Delete { get; set; }

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.View")]
        public bool View { get; set; }

        [NopResourceDisplayName("Admin.Configuration.UserPermissions.Fields.Full")]
        public bool Full { get; set; }

        public IList<SelectListItem> AvailablePermissions { get; set; }

        public IList<SelectListItem> AvailableUsers { get; set; }

        #endregion
    }
}
