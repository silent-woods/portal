using App.Core.Domain.Leaves;
using App.Web.Models.Extensions.LeaveManagement;
using System.Threading.Tasks;

namespace App.Web.Factories.Extensions
{
    public partial interface ILeaveManagementModelFactory
    {
        Task<LeaveManagementSearchModel> PrepareLeaveManagementSearchModelAsync(LeaveManagementSearchModel searchModel);
        Task<LeaveManagementListModel> PrepareLeaveManagementListModelAsync(LeaveManagementSearchModel searchModel);
        Task<LeaveManagementModel> PrepareLeaveManagementModelAsync(LeaveManagementModel model, LeaveManagement leaveManagement);
    }
}
