using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.JobPostingApiModels
{
    public record JobPostingApiModel : BaseNopEntityModel
    {
        #region Properties

        public string Title { get; set; }
        public string Description { get; set; }

        #endregion
    }
}