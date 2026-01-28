using System.Collections.Generic;
using App.Web.Areas.Admin.Models.Stores;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Settings
{
    /// <summary>
    /// Represents a store scope configuration model
    /// </summary>
    public partial record StoreScopeConfigurationModel : BaseNopModel
    {
        #region Ctor

        public StoreScopeConfigurationModel()
        {
            Stores = new List<StoreModel>();
        }

        #endregion

        #region Properties

        public int StoreId { get; set; }

        public IList<StoreModel> Stores { get; set; }

        #endregion
    }
}