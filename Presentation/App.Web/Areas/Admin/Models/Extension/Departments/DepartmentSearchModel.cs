using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Departments

{
    /// <summary>
    /// Represents a department search model
    /// </summary>
    public partial record DepartmentSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Department.Fields.SearchName")]
        public string SearchName { get; set; }

        //[NopResourceDisplayName("Admin.Extension.Departments.Fields.CreatedOnUtc")]
        //public DateTime SearchCreatedOnUtc { get; set; }

        #endregion
    }
}