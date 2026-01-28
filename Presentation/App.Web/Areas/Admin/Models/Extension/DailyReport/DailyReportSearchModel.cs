using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.TimeSheets
{
    /// <summary>
    /// Represents a timesheet search model
    /// </summary>
    public partial record DailyReportSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.TimeSheet.List.ProjectName")]
        public string ProjectName { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.List.EmployeeName")]
        public string EmployeeName { get; set; }
        #endregion
    }
}