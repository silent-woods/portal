namespace App.Web.Models.Extensions.LeaveManagement
{
    public class CombinedModel
    {
        public LeaveTypeModel LeaveTypeModel { get; set; }
        public LeaveManagementSearchModel LeaveManagementSearchModel { get; set; }

        public CombinedModel()
        {
            LeaveTypeModel = new LeaveTypeModel();
            LeaveManagementSearchModel = new LeaveManagementSearchModel();
        }
    }
}
