using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record DataConfigModel : BaseNopModel, IConfigModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.Configuration.AppSettings.Data.ConnectionString")]
        public string ConnectionString { get; set; }

        [NopResourceDisplayName("Admin.Configuration.AppSettings.Data.DataProvider")]
        public int DataProvider { get; set; }
        public SelectList DataProviderTypeValues { get; set; }

        [NopResourceDisplayName("Admin.Configuration.AppSettings.Data.SQLCommandTimeout")]
        [UIHint("Int32Nullable")]
        public int? SQLCommandTimeout { get; set; }

        #endregion
    }
}
