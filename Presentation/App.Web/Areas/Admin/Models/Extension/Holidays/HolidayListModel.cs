using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Holidays

{
    /// <summary>
    /// Represents a holiday list model
    /// </summary>
    public partial record HolidayListModel : BasePagedListModel<HolidayModel>
    {
      
    }
}