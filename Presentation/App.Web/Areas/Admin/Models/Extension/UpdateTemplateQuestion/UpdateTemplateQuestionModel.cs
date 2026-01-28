using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion
{
    public partial record UpdateTemplateQuestionModel : BaseNopEntityModel
    {
        public UpdateTemplateQuestionModel()
        {
            AvailableUpdateTemplate = new List<SelectListItem>();
            AvailableControlTypes = new List<SelectListItem>();
            updateQuestionOptionSearchModel = new UpdateQuestionOptionSearchModel();
            UpdateQuestionOptionModel = new UpdateQuestionOptionModel();
        }

        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.UpdateTemplateId")]
        public int UpdateTemplateId { get; set; }
        public IList<SelectListItem> AvailableUpdateTemplate { get; set; }
        public string UpdateTemplateTitle { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.QuestionText")]
        
        public string QuestionText { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.IsRequired")]
        public bool IsRequired { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.ControlType")]
        public int ControlTypeId { get; set; }
        public IList<SelectListItem> AvailableControlTypes { get; set; }
        public string ControlTypeName { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.ValidationMinLength")]
        public int? ValidationMinLength { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.ValidationMaxLength")]
        public int? ValidationMaxLength { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.ValidationFileMaximumSize")]
        public int? ValidationFileMaximumSize { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.ValidationFileAllowedExtensions")]
        public string ValidationFileAllowedExtensions { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplateQuestions.Fields.DefaultValue")]
        public string DefaultValue { get; set; }

        public UpdateQuestionOptionSearchModel updateQuestionOptionSearchModel { get; set; }
        public UpdateQuestionOptionModel UpdateQuestionOptionModel { get; set; }
    }
}
