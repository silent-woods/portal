using System.Threading.Tasks;
using App.Core.Domain.Leaves;
using App.Web.Areas.Admin.Models.LeaveManagement;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface ILeaveManagementModelFactory
    {
        Task<LeaveManagementSearchModel> PrepareLeaveManagementSearchModelAsync(LeaveManagementSearchModel searchModel);

        Task<LeaveManagementListModel> PrepareLeaveManagementListModelAsync(LeaveManagementSearchModel searchModel);

        Task<LeaveManagementModel> PrepareLeaveManagementModelAsync(LeaveManagementModel model, LeaveManagement leaveManagement, bool excludeProperties = false);
    }
}