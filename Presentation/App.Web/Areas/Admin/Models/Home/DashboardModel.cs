using App.Web.Areas.Admin.Models.Common;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Home
{
    /// <summary>
    /// Represents a dashboard model
    /// </summary>
    public partial record DashboardModel : BaseNopModel
    {
        #region Ctor

        public DashboardModel()
        {
            PopularSearchTerms = new PopularSearchTermSearchModel();
            EmployeeOnLeave = new EmployeeOnLeaveSearchModel();
        }

        #endregion

        #region Properties

        public bool IsLoggedInAsVendor { get; set; }

        public PopularSearchTermSearchModel PopularSearchTerms { get; set; }

        public EmployeeOnLeaveSearchModel EmployeeOnLeave { get; set; }


        #endregion
    }
}