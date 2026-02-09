using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public partial record UpdateSubmissionListModel : BaseNopEntityModel
    {
        public UpdateSubmissionListModel()
        {
            AvailableSubmitters = new List<SelectListItem>();
            AvailableReportingPeriods = new List<SelectListItem>();
            Submissions = new List<SubmissionCardModel>();
            AvailableTemplates = new List<SelectListItem>();
            AvailablePeriods = new List<SelectListItem>();
        }

        #region Properties

        [NopResourceDisplayName("Admin.UpdateSubmissionListModel.Fields.SelectedSubmitterId")]

        public int? SelectedSubmitterId { get; set; }

        [NopResourceDisplayName("Admin.UpdateSubmissionListModel.Fields.SelectedPeriod")]
        public string SelectedPeriod { get; set; } // e.g. "2025-07-07|2025-07-10"

        public IList<SelectListItem> AvailableSubmitters { get; set; }
        public IList<SelectListItem> AvailableReportingPeriods { get; set; }

        public IList<SubmissionCardModel> Submissions { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionListModel.Fields.FromDate")]
        public DateTime? FromDate { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionListModel.Fields.ToDate")]
        public DateTime? ToDate { get; set; }
        public bool CanSeeSubmitterDropdown { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionListModel.Fields.SelectedTemplateId")]
        public int? SelectedTemplateId { get; set; }
        public IList<SelectListItem> AvailableTemplates { get; set; }
        [NopResourceDisplayName("Admin.UpdateSubmissionListModel.Fields.SelectedPeriodId")]
        public int? SelectedPeriodId { get; set; }
        public IList<SelectListItem> AvailablePeriods { get; set; }

        public List<string> NotSubmittedNames { get; set; } = new();
        public string TemplateTitle { get; set; }

        #endregion
    }


}
