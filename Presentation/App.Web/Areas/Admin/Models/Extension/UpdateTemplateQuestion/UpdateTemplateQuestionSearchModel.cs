using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion
{
    public partial record UpdateTemplateQuestionSearchModel : BaseSearchModel
    {
        public UpdateTemplateQuestionSearchModel()
        {
            AvailableControlTypes = new List<SelectListItem>();
        }
        #region Properties
        public int UpdateTemplateId { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestionSearchModel.Fields.Question")]
        public string Question { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestionSearchModel.Fields.IsRequired")]
        public bool? IsRequired { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestionSearchModel.Fields.ControlType")]
        public int ControlType { get; set; }
        public IList<SelectListItem> AvailableControlTypes { get; set; }
        #endregion
    }
}
