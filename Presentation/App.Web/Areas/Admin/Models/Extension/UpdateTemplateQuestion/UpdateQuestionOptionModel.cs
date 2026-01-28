using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion
{
    public partial record UpdateQuestionOptionModel : BaseNopEntityModel
    {
        public UpdateQuestionOptionModel()
        {
        }

        public int UpdateTemplateQuestionId { get; set; }

        [NopResourceDisplayName("Admin.UpdateQuestionOption.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.UpdateQuestionOption.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        
        [NopResourceDisplayName("Admin.UpdateQuestionOption.Fields.IsPreSelected")]
        public bool IsPreSelected { get; set; }
        [NopResourceDisplayName("Admin.UpdateQuestionOption.Fields.IsRequired")]
        public bool IsRequired { get; set; }
        public int ControlType { get; set; }


    }
}
