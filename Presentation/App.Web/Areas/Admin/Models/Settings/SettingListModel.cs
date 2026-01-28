using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a setting list model
    /// </summary>
    public partial record SettingListModel : BasePagedListModel<SettingModel>
    {
    }
}