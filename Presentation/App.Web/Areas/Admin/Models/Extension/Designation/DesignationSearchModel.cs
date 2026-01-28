using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Designation
{
    /// <summary>
    /// Represents a customer search model
    /// </summary>
    public partial record DesignationSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Designation.List.SearchDesignationName")]
        public string SearchDesignationName { get; set; }

        #endregion
    }
}