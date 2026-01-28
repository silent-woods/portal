using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Models.Extensions.UpdateForm
{
    [Bind(nameof(QuestionId), nameof(AnswerText), nameof(ControlType), nameof(UploadedFile))]
    public partial record SubmitUpdateQuestionModel : BaseNopEntityModel
    {
        public SubmitUpdateQuestionModel()
        {
            Options = new List<SelectListItem>();
            RequiredOptionValues = new List<string>();
        }

        #region Properties

        [NopResourceDisplayName("Admin.SubmitUpdateQuestionModel.Fields.QuestionId")]

        public int QuestionId { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateQuestionModel.Fields.QuestionText")]
        public string QuestionText { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateQuestionModel.Fields.IsRequired")]
        public bool IsRequired { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateQuestionModel.Fields.ControlType")]
        public string ControlType { get; set; } // TextBox, Dropdown, RadioButton, etc.
        [NopResourceDisplayName("Admin.SubmitUpdateQuestionModel.Fields.DefaultValue")]
        public string DefaultValue { get; set; }
        [NopResourceDisplayName("Admin.SubmitUpdateQuestionModel.Fields.AnswerText")]
        public string AnswerText { get; set; }
        public string ExistingFilePath { get; set; }
        public int DisplayOrder { get; set; }
        public List<SelectListItem> Options { get; set; } = new();
        public bool IsSelected { get; set; }
        public IFormFile UploadedFile { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public int? MaximumFileSizeKb { get; set; }
        public string AllowedFileExtensions { get; set; }
        public string FileName { get; set; }
        public IList<string> RequiredOptionValues { get; set; }
        #endregion
    }


}
