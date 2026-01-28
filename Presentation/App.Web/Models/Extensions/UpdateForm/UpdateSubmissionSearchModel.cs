using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{

    public partial record UpdateSubmissionSearchModel: BaseSearchModel
    {
        #region Properties

        [NopResourceDisplayName("Admin.UpdateTemplateListItemModel.Fields.Title")]
        public string Title { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateListItemModel.Fields.IsSubmitted")]
        public bool? IsSubmitted { get; set; }
        public IList<SelectListItem> AvailableSubmittedFilter { get; set; } = new List<SelectListItem>();
        #endregion
    }


}
