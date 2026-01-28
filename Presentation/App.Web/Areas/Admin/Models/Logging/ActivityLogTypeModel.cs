using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Logging
{
    /// <summary>
    /// Represents an activity log type model
    /// </summary>
    public partial record ActivityLogTypeModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Customers.ActivityLogType.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.Customers.ActivityLogType.Fields.Enabled")]
        public bool Enabled { get; set; }

        #endregion
    }
}