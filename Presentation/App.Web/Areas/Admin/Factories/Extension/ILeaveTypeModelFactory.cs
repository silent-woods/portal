using System.Threading.Tasks;
using App.Core.Domain.Leaves;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.Leavetypes;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface ILeaveTypeModelFactory
    {
        Task<LeaveTypeSearchModel> PrepareLeaveTypeSearchModelAsync(LeaveTypeSearchModel searchModel);

        Task<LeaveTypeListModel> PrepareLeaveTypeListModelAsync(LeaveTypeSearchModel searchModel);

        Task<LeaveTypeModel> PrepareLeaveTypeModelAsync(LeaveTypeModel model, Leave leaveTypes, bool excludeProperties = false);
    }
}