using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a kpimaster search model
    /// </summary>
    public partial record KPIMasterSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.KPIMaster.List.kpiName")]
        public string kpiName { get; set; }

        #endregion
    }
}