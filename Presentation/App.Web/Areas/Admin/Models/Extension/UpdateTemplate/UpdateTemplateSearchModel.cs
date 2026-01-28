using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.UpdateTemplate
{
    public partial record UpdateTemplateSearchModel : BaseSearchModel
    {
        public UpdateTemplateSearchModel()
        {
            AvailableFrequencies = new List<SelectListItem>();
        }
        #region Properties
        [NopResourceDisplayName("Admin.UpdateTemplateSearchModel.Fields.Title")]
        public string Title { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateSearchModel.Fields.FrequencyId")]
        public int FrequencyId { get; set; }
        public IList<SelectListItem> AvailableFrequencies { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateSearchModel.Fields.DueDateTime")]
        [UIHint("DateNullable")]
        public DateTime? DueDateTime { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateSearchModel.Fields.DueTime")]
        [UIHint("TimeNullable")]
        [DataType(DataType.Time)]
        public string DueTime { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateSearchModel.Fields.IsActive")]
        public bool? IsActive { get; set; }
        #endregion
    }
}
