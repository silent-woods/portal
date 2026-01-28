using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.WeeklyQuestions
{
    /// <summary>
    /// Represents a WeeklyQuestions model
    /// </summary>
    public partial record WeeklyQuestionsModel : BaseNopEntityModel
    {
        public WeeklyQuestionsModel()
        {
            Designations = new List<SelectListItem>();
            ControlTyeps = new List<SelectListItem>();
        }
        public IList<SelectListItem> Designations { get; set; }
        #region Properties
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.QuestionText")]
        [Required(ErrorMessage = "Please enter Question Text")]
        public string QuestionText { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.ControlType")]

        [Required(ErrorMessage = "Please select Control Type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Control Type.")]
        public int ControlTypeId { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.ControlType")]
        public string ControlType { get; set; }

        public IList<SelectListItem> ControlTyeps { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.ControlValue")]

        [Required(ErrorMessage = "Please enter Control Value.")]
        public string ControlValue { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.DisplayOrder")]

        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.DesignationName")]

        [Required(ErrorMessage = "Please select Designation.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Designation.")]
        public int DesignationId { get; set; }
        [NopResourceDisplayName("Admin.WeeklyQuestions.Fields.DesignationName")]
        public string DesignationName { get; set; }
        #endregion
    }
}