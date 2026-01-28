using System;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.PerformanceMeasurements
{
    /// <summary>
    /// Represents a KPIMaster model
    /// </summary>
    public partial record KPIMasterModel : BaseNopEntityModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.KPIMaster.Fields.Name")]
        [Required(ErrorMessage = "Please enter a name.")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.KPIMaster.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        [NopResourceDisplayName("Admin.KPIMaster.Fields.CreateOn")]
        public DateTime CreateOn { get; set; }

        [NopResourceDisplayName("Admin.KPIMaster.Fields.UpdateOn")]
        public DateTime UpdateOn { get; set; }

        #endregion
    }
    public class KPIMasterModels
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Rating { get; set; }
        public int MonthId { get; set; }
    }
}