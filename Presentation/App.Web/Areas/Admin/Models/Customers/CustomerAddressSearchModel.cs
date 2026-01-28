using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Customers
{
    /// <summary>
    /// Represents a customer address search model
    /// </summary>
    public partial record CustomerAddressSearchModel : BaseSearchModel
    {
        #region Properties

        public int CustomerId { get; set; }

        #endregion
    }
}