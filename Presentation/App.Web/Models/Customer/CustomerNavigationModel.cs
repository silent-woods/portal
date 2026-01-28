using System.Collections.Generic;
using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record CustomerNavigationModel : BaseNopModel
    {
        public CustomerNavigationModel()
        {
            CustomerNavigationItems = new List<CustomerNavigationItemModel>();
        }

        public IList<CustomerNavigationItemModel> CustomerNavigationItems { get; set; }

        public int SelectedTab { get; set; }
    }

    public partial record CustomerNavigationItemModel : BaseNopModel
    {
        public string RouteName { get; set; }
        public string Title { get; set; }
        public int Tab { get; set; }
        public string ItemClass { get; set; }
    }

    public enum CustomerNavigationEnum
    {
        Info = 1,
        EmployeeInfo = 0,
        Addresses = 10,
        EmployeeAddresses = 11,
        Orders = 20,
        EmployeeEducation = 21,
        BackInStockSubscriptions = 30,
        EmployeeExperience = 31,
        ReturnRequests = 40,
        EmployeeAssets = 41,
        DownloadableProducts = 50,
        MonthlyReview = 51,
        RewardPoints = 60,
        YearlyReview = 61,
        ChangePassword = 70,
        Avatar = 80,
        ForumSubscriptions = 90,
        ProductReviews = 100,
        VendorInfo = 110,
        GdprTools = 120,
        CheckGiftCardBalance = 130,
        MultiFactorAuthentication = 140,
        Weeklyreport = 150,
        LeaveCreate = 32,
        TimeSheetList=160,
        UpdateTimeSheet = 161,
        TimeSummaryReport = 162,
        EmployeePerformanceReport= 163,
        LeaveManagement = 164,
        EmployeeAttendanceReport =165,
        AddRatings = 166,
        ProjectLeaderReview=167,
        TaskList= 168,
        ViewUpdateList =169,
        ProjectManagement =170

    }
}