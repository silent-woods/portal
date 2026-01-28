using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Employees
{
    /// <summary>
    /// Represents a Experience search model
    /// </summary>
    public partial record ExperienceSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Experience.List.Experience")]
        public string Experience { get; set; }
        public int employeeId { get; set; }
        #endregion
    }
}