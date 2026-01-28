using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a kpiweightage search model
    /// </summary>
    public partial record KPIWeightageSearchModel : BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.KPIWeightage.List.KPIName")]
        public string KPIName { get; set; }

        #endregion
    }
}