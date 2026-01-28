using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer activity log search model
    /// </summary>
    public partial record CustomerActivityLogSearchModel : BaseSearchModel
    {
        #region Properties

        public int CustomerId { get; set; }

        #endregion
    }
}