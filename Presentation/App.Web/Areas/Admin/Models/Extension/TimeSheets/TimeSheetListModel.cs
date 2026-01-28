using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.TimeSheets
{
    /// <summary>
    /// Represents a timesheet list model
    /// </summary>
    public partial record TimeSheetListModel : BasePagedListModel<TimeSheetModel>
    {
    }
}