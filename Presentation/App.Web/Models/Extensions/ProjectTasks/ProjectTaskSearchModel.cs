using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.Extensions.ProjectTasks
{
    public partial record ProjectTaskSearchModel : BaseSearchModel
    {
        public ProjectTaskSearchModel() {
        
            AvailableStatus = new List<SelectListItem>();
            AvailableQaRequired = new List<SelectListItem>();
            AvailableTaskTypes= new List<SelectListItem>();
            SelectedProjectIds = new List<int>();
            SelectedEmployeeIds = new List<int>();
            AvailableProjects = new List<SelectListItem>();
            AvailableEmployees = new List<SelectListItem>();
            AvailableProcessWorkflow = new List<SelectListItem>();
            AvailableDeliveryOnTime = new List<SelectListItem>();
            AvailableParentTasks = new List<SelectListItem>();
        }
        #region Properties

        public int EmployeeId { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchProjectId")]

        public int SearchProjectId { get; set; }
      
        public int TaskId { get; set; }
        public string TaskName { get; set; }

        public string SearchProjectName { get; set; }
     

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchTaskTitle")]
        public string SearchTaskTitle { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchParentTask")]

        public int SearchParentTaskId { get; set; }
        public IList<SelectListItem> AvailableParentTasks { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchStatusId")]

        public int SearchStatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableProjects")]
        public IList<SelectListItem> AvailableProjects { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedProjectIds")]
        public IList<int> SelectedProjectIds { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchQaRequired")]

        public int SearchQaRequired { get; set; }
        public IList<SelectListItem> AvailableQaRequired { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchProcessWorkflow")]

        public int SearchProcessWorkflowId { get; set; }
        public IList<SelectListItem> AvailableProcessWorkflow { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchTaskTypeId")]

        public int SearchTaskTypeId { get; set; }
        public IList<SelectListItem> AvailableTaskTypes { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeIds")]
        public IList<int> SelectedEmployeeIds { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.List.From")]
        [UIHint("DateNullable")]

        public DateTime? From { get; set; }
        [NopResourceDisplayName("Admin.TimeSheet.List.To")]
        [UIHint("DateNullable")]


        public DateTime? To { get; set; }


        [NopResourceDisplayName("Admin.ProjectTasks.Fields.DueDate")]

        public DateTime? DueDate { get; set; }

        public string DueDateFormat { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchDeliveryOnTime")]

        public int SearchDeliveryOnTime { get; set; }
        public IList<SelectListItem> AvailableDeliveryOnTime { get; set; }



        #endregion
    }
}
