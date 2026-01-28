using App.Core.Domain.TimeSheets;
using App.Web.Areas.Admin.Models.TimeSheets;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the itimesheet model factory
    /// </summary>
    public partial interface ITimeSheetModelFactory
    {
        Task<TimeSheetSearchModel> PrepareTimeSheetSearchModelAsync(TimeSheetSearchModel searchModel);

        Task<TimeSheetListModel> PrepareTimeSheetListModelAsync(TimeSheetSearchModel searchModel);

        Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false);
    }
}