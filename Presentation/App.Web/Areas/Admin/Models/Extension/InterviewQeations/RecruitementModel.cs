using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.InterviewQeations.Models
{
    public record RecruitementModel : BaseNopEntityModel
    {


        public RecruitementModel()
        {
            AvailableCategory = new List<SelectListItem>();
            AvailablequestiontypeId = new List<SelectListItem>();
            AvailableQuestionLevelId = new List<SelectListItem>();


        }
        [Required(ErrorMessage = "Please enter Question")]
        [NopResourceDisplayName("Admin.Question.Fields.Question")]
        public string Question { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.Question_answers")]
        [Required(ErrorMessage = "Please enter Question Answer")]
        public string Question_answers { get; set; }



        public bool DocumentsDownload { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.Documents")]
        [UIHint("Download")]
        public int DownloadId { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.Documents")]

        public Guid DownloadGuid { get; set; }

        public bool HasDownload { get; set; }

        [NopResourceDisplayName("Admin.Question.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.Published")]
        public bool Published { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.CreatedOn")]

        public DateTime CreatedOn { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.UpdatedOn")]
        public DateTime UpdatedOnUtc { get; set; }

        [Required(ErrorMessage = "Please select Category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Category.")]
        public int CategoryId { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.Category")]
        public string Category { get; set; }



        public IList<SelectListItem> AvailableCategory { get; set; }

        [Required(ErrorMessage = "Please select Question Type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Question Type.")]
        public int questiontypeId { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.QuestionType")]
        public string questiontype { get; set; }

        public IList<SelectListItem> AvailablequestiontypeId { get; set; }

        [Required(ErrorMessage = "Please select Question Level.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Question Level.")]
        public int QuestionLevelId { get; set; }
        [NopResourceDisplayName("Admin.Question.Fields.QuestionLevel")]
        public string QuestionLevel { get; set; }

        public IList<SelectListItem> AvailableQuestionLevelId { get; set; }

    }
}