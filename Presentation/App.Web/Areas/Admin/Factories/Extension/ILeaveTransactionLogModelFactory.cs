using System.Threading.Tasks;
using App.Core.Domain.Leaves;
using App.Web.Areas.Admin.Models;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Areas.Admin.Models.Leavetypes;

namespace App.Web.Areas.Admin.Factories
{
    public partial interface ILeaveTransactionLogModelFactory
    {
        Task<LeaveTransactionLogSearchModel> PrepareLeaveTransactionLogSearchModelAsync(LeaveTransactionLogSearchModel searchModel);
        Task<LeaveTransactionLogListModel> PrepareLeaveTransactionListModelAsync(LeaveTransactionLogSearchModel searchModel);

        Task<LeaveTransactionLogModel> PrepareLeaveTransactionLogModelAsync(LeaveTransactionLogModel model, LeaveTransactionLog leaveTransactionLog, bool excludeProperties = false);

        Task<LeaveTransactionLogModel> PrepareAddMonthlyLeave(LeaveTransactionLogModel model);
    }
}