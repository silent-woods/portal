using System;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer activity log model
    /// </summary>
    public partial record CustomerActivityLogModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.ActivityLogType")]
        public string ActivityLogTypeName { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.Comment")]
        public string Comment { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.ActivityLog.IpAddress")]
        public string IpAddress { get; set; }

        #endregion
    }
}