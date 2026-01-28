using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Common
{
    public partial record CommonStatisticsModel : BaseNopModel
    {
  
        public int NumberOfCustomers { get; set; }

    }
}