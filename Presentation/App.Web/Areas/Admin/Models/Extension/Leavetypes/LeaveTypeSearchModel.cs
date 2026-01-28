using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Leavetypes
{
    /// <summary>
    /// Represents a LeaveType search model
    /// </summary>
    public partial record LeaveTypeSearchModel : BaseSearchModel
    {
        #region Properties
        [NopResourceDisplayName("Admin.Extension.Leavetypes.List.SearchLeavetypesName")]
        public string SearchLeavetypesName { get; set; }
        #endregion
    }
}