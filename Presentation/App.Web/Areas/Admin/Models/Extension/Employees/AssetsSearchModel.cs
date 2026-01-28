using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Employees
{
    /// <summary>
    /// Represents a Assets search model
    /// </summary>
    public partial record AssetsSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Assets.List.Assets")]
        public string Assets { get; set; }
        public int employeeId { get; set; }

        #endregion
    }
}