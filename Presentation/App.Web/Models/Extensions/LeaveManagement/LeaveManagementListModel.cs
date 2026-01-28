using System;
using System.Collections.Generic;
using App.Web.Framework.Models;


namespace App.Web.Models.Extensions.LeaveManagement
{
    public class LeaveManagementListModel
    {
        public LeaveManagementListModel()
        {
            LeaveManagements = new List<LeaveManagementModel>();
            //LeaveSummery = new List<LeaveTypeModel>();
        }

        public IList<LeaveManagementModel> LeaveManagements { get; set; }

        //public IList<LeaveTypeModel> LeaveSummery { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }

    }
}
