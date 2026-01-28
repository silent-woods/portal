using App.Web.Areas.Admin.Models.Extension.TaskChangeLogs;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.ProjectEmployeeMappings;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.ProjectTasks
{
    public partial record ProjectTaskModel : BaseNopEntityModel
    {
    
       public  ProjectTaskModel()
        {
            Projects= new List<SelectListItem>();
            StatusList= new List<SelectListItem>();
            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
            TaskCommentsModel = new List<TaskCommentsModel>();
            TaskCommentsSearchModel = new TaskCommentsSearchModel();
            TaskChangeLogModel = new List<TaskChangeLogModel>();
            TaskChangeLogSearchModel = new TaskChangeLogSearchModel();
            AvailableProcessWorkflows = new List<SelectListItem>();
            AvailableParentTasks = new List<SelectListItem>();
            AvailableAssigntoEmployees = new List<SelectListItem>();
            AvailableTaskCategories = new List<SelectListItem>();
        }
        [Required(ErrorMessage = "Please select Project")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Project")]
        [NopResourceDisplayName("Admin.ProjectTasks.Fields.ProjectId")]
        public int ProjectId { get; set; }

        public string ProjectName { get; set; }

        public IList<SelectListItem> Projects { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.TaskTitle")]
        [Required(ErrorMessage = "Please Enter Task Title")]
        public string TaskTitle { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.Description")]
        public string Description { get; set; }
        [NopResourceDisplayName("Admin.ProjectTasks.Fields.StatusId")]
        public int StatusId { get; set; }
    
        public string Status { get; set; }
        public IList<SelectListItem> StatusList { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.EstimatedTime")]
        [Required(ErrorMessage = "Estimated Time Must be Positive Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Estimated Time Must be Positive Value")]
        public decimal EstimatedTime { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.EstimatedTime")]
        public string EstimationTimeHHMM { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SpentTime")]
        [Required(ErrorMessage = "Spent Hours Must be Positive Value")]
        [Range(0, int.MaxValue, ErrorMessage = "Spent Hours Must be Positive Value")]
        public int SpentHours { get; set; }

        public int SpentMinutes { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SpentTime")]
        public string SpentTime { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.BugCount")]
        [Required(ErrorMessage = "Bug Count Must be Positive Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Bug Count Must be Positive Value")]
        public int BugCount { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.QualityComments")]
        public string QualityComments { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.AssignedTo")]
        public int AssignedTo { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.ProjectTasks.Fields.From")]
        public DateTime? From { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.ProjectTasks.Fields.To")]
        public DateTime? To { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.EmployeeId")]
        public int EmployeeId { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }

        public IList<SelectListItem> AvailableAssigntoEmployees { get; set; }


        [NopResourceDisplayName("Admin.ProjectTasks.Fields.AssignedTo")]
        public IList<int> SelectedEmployeeId { get; set; }

        public bool DeliveryOnTime { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.IsManualDOT")]
        public bool IsManualDOT { get; set; }

        public string AssignedEmployee {  get; set; }
        public DateTime CreatedOnUtc { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.Tasktypeid")]
        [Required(ErrorMessage = "Please Select Task Type")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Task Type")]
        public int Tasktypeid { get; set; }

        public IList<SelectListItem> TaskTypeList { get; set; }

        public string TaskTypeName { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.IsQARequired")]
        public bool IsQARequired { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Admin.ProjectTasks.Fields.DueDate")]
        public DateTime? DueDate { get; set; }

        public string DueDateFormat { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.StatusChangeComment")]
        public string StatusChangeComment { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.ProcessWorkflowId")]
        public int ProcessWorkflowId { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.ParentTaskId")]
        public int ParentTaskId { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.IsSync")]
        public bool IsSync { get; set; }

        public string ParentTaskName { get; set; }

        public decimal? WorkQuality { get; set; }

        public string WorkQualityFormat { get; set; }


        public decimal? DOTPercentage { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.TaskCategoryId")]
        [Required(ErrorMessage = "Please Select Task Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please Select Task Category")]

        public int TaskCategoryId { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.DeveloperId")]
        public int DeveloperId { get; set; }

        public IList<SelectListItem> AvailableProcessWorkflows { get; set; }
        public IList<SelectListItem> AvailableParentTasks { get; set; }
        public IList<SelectListItem> AvailableTaskCategories{ get; set; }
        public IList<TaskCommentsModel> TaskCommentsModel { get; set; }
        public TaskCommentsSearchModel TaskCommentsSearchModel { get; set; }
        public IList<TaskChangeLogModel> TaskChangeLogModel { get; set; }
        public TaskChangeLogSearchModel TaskChangeLogSearchModel { get; set; }
    }
}
