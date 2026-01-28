using App.Core.Domain.TimeSheets;
using App.Web.Models.Extensions.TimeSheets;
using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial interface ITimeSheetModelFactory
    {
        Task<TimeSheetSearchModel> PrepareTimeSheetSearchModelAsync(TimeSheetSearchModel searchModel);

        //Task<TimeSheetListModel> PrepareTimeSheetListModelAsync(TimeSheetSearchModel searchModel);

        Task<TimeSheetModel> PrepareTimeSheetModelAsync(TimeSheetModel model, TimeSheet timeSheet, bool excludeProperties = false);
    }
}
