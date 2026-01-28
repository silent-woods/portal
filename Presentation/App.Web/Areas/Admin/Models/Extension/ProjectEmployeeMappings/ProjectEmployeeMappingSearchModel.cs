using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.ProjectEmployeeMappings
{
    /// <summary>
    /// Represents a projectemployeemapping search model
    /// </summary>
    public partial record ProjectEmployeeMappingSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.ProjectEmployeeMapping.List.SearchProjectemp")]
        public string SearchProjectemp { get; set; }


        public int ProjectId { get; set; }

        #endregion
    }
}