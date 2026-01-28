using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Projects
{
    /// <summary>
    /// Represents a project search model
    /// </summary>
    public partial record ProjectSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Project.List.SearchProjectTitle")]
        public string SearchProjectTitle { get; set; }

        #endregion
    }
}