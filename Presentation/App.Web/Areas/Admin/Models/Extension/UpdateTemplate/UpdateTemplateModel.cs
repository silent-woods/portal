using App.Web.Areas.Admin.Models.Extension.UpdateTemplateQuestion;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.UpdateTemplate
{
    public partial record UpdateTemplateModel : BaseNopEntityModel
    {
        public UpdateTemplateModel()
        {
            AvailableFrequencies = new List<SelectListItem>();
            AvailableUser = new List<SelectListItem>();
            SubmitterUsers = new List<SelectListItem>();
            ViewerUsers = new List<SelectListItem>();
            AvailableRepeatTypes = new List<SelectListItem>();
            SelectedSubmitterIds = new List<int>();
            SelectedViewerIds = new List<int>();
            updateTemplateQuestionSearchModel = new UpdateTemplateQuestionSearchModel();
            updateTemplateQuestionModels = new List<UpdateTemplateQuestionModel>();
        }

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.Title")]
        [Required(ErrorMessage = "Please enter a title.")]
        public string Title { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.FrequencyId")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a frequency.")]
        public int FrequencyId { get; set; }
        public IList<SelectListItem> AvailableFrequencies { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.Frequency")]
        public string FrequencyName { get; set; }
        
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.DueDate")]
        [UIHint("DateNullable")]
        public DateTime? DueDate { get; set; }
        [Required(ErrorMessage = "Please select a due time.")]
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.DueTime")]
        [UIHint("Time")]
        [DataType(DataType.Time)]
        public string DueTime { get; set; }
        public string DueTimeString { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.ReminderBeforeMinutes")]
        public int ReminderBeforeMinutes { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.ReminderBeforeDays")]
        public int ReminderBeforeDays { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.ReminderBeforeHours")]
        public int ReminderBeforeHours { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.ReminderBeforeMinutesOnly")]
        public int ReminderBeforeMinutesOnly { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.ReminderBefore")]
        public string ReminderBefore { get; set; }

        // ========== Permissions & Options ==========
        
       
        

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.IsFileAttachmentRequired")]
        public bool IsFileAttachmentRequired { get; set; }

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.IsEditingAllowed")]
        public bool IsEditingAllowed { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.IsActive")]
        public bool IsActive { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.CreatedByUserId")]
        public int CreatedByUserId { get; set; }
        public IList<SelectListItem> AvailableUser { get; set; }
        public DateTime CreatedOnUTC { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.RepeatEvery")]
        public int RepeatEvery { get; set; } // e.g., 1 (every 1 day)
        public IList<SelectListItem> AvailableRepeatTypes { get; set; }
        public string RepeatType { get; set; } // e.g., "Day", "Week", "Month"
        public DateTime? RepeatTime { get; set; } // Time of the day for the update.
        

        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.SelectWeekdays")]
        public string SelectedWeekdays { get; set; }
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.OnDay")]
        public int? OnDay { get; set; }
        [Required(ErrorMessage = "Please select at least one submitter.")]
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.SubmitterUserIds")]
        public List<int> SelectedSubmitterIds { get; set; } = new();
        public IList<SelectListItem> SubmitterUsers { get; set; }
        [Required(ErrorMessage = "Please select at least one viewer.")]
        [NopResourceDisplayName("Admin.UpdateTemplate.Fields.ViewerUserIds")]
        public List<int> SelectedViewerIds { get; set; } = new();
        public IList<SelectListItem> ViewerUsers { get; set; }
        public string DueDateFormatted => DueDate?.ToString("MM/dd/yyyy");

        public UpdateTemplateQuestionSearchModel updateTemplateQuestionSearchModel { get; set; }
        public List<UpdateTemplateQuestionModel> updateTemplateQuestionModels { get; set; }
    }
}
