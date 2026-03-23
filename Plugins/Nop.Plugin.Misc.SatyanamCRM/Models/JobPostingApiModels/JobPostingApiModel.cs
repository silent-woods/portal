using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.JobPostingApiModels
{
    public record JobPostingApiModel : BaseNopEntityModel
    {
        #region Properties

        public string Title { get; set; }
        public string Description { get; set; }
        public int CandidateTypeId { get; set; }
        public int PositionId { get; set; }

        #endregion
    }
}