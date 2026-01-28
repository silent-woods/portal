using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.Extension.ProjectTasks
{
    public partial record ProjectTaskSearchModel : BaseSearchModel
    {                                                                                 
        public ProjectTaskSearchModel() {
            AvailableProject = new List<SelectListItem>();
            AvailableStatus = new List<SelectListItem>();
            SelectedEmployeeIds = new List<int>();
            SelectedProjectIds = new List<int>();

            AvailableEmployees = new List<SelectListItem>();
            AvailableQaRequired = new List<SelectListItem>();
            AvailableTaskTypes = new List<SelectListItem>();
            AvailableProcessWorkflow = new List<SelectListItem>();
            AvailableDeliveryOnTime = new List<SelectListItem>();
            AvailableParentTasks = new List<SelectListItem>(); 

        }
        #region Properties

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchProjectId")]

        public int SearchProjectId { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchTaskId")]

        public int SearchTaskId { get; set; }

        public string SearchProjectName { get; set; }
        public IList<SelectListItem> AvailableProject { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedProjectIds")]
        public IList<int> SelectedProjectIds { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchTaskTitle")]
        public string SearchTaskTitle { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchStatusId")]

        public int SearchStatusId { get; set; }
        public IList<SelectListItem> AvailableStatus { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.AvailableEmployees")]
        public IList<SelectListItem> AvailableEmployees { get; set; }

        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeIds")]
        public IList<int> SelectedEmployeeIds { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.DueDate")]
        [UIHint("DateNullable")]

        public DateTime? DueDate { get; set; }

        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchQaRequired")]

        public int SearchQaRequired { get; set; }
        public IList<SelectListItem> AvailableQaRequired { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchDeliveryOnTime")]

        public int SearchDeliveryOnTime { get; set; }
        public IList<SelectListItem> AvailableDeliveryOnTime { get; set; }

        public int ProcessWorkflowId { get; set; }


        [NopResourceDisplayName("Admin.TimeSheet.Fields.SearchTaskTypeId")]

        public int SearchTaskTypeId { get; set; }
        public IList<SelectListItem> AvailableTaskTypes { get; set; }
        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchProcessWorkflowId")]

        public int SearchProcessWorkflowId { get; set; }
        public IList<SelectListItem> AvailableProcessWorkflow { get; set; }

        [NopResourceDisplayName("Admin.ProjectTasks.Fields.SearchParentTask")]

        public int SearchParentTaskId { get; set; }
        public IList<SelectListItem> AvailableParentTasks { get; set; }






        #endregion
    }
}
