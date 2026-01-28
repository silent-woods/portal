using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.Activities
{
    public partial record ActivityModel : BaseNopEntityModel
    {

    
       public ActivityModel()
        {
            Projects= new List<SelectListItem>();
           
        


           

        AvailableEmployees = new List<SelectListItem>();

        AvailableTasks = new List<SelectListItem>();

        AvailableProjects = new List<SelectListItem>();


        }
    [Required(ErrorMessage = "Please select Project")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Project")]
        [NopResourceDisplayName("Admin.Activity.Fields.ProjectId")]
        public int ProjectId { get; set; }

        [Required(ErrorMessage = "Please select Task")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Task")]
        [NopResourceDisplayName("Admin.Activity.Fields.TaskId")]
        public int TaskId { get; set; }

        [Required(ErrorMessage = "Please select Activity")]
        [NopResourceDisplayName("Admin.Activity.Fields.ActivityName")]

        public string ActivityName { get; set; }

        public string ProjectName { get; set; }

        public IList<SelectListItem> Projects { get; set; }
        [NopResourceDisplayName("Admin.Activity.Fields.TaskTitle")]

   

        public string TaskTitle { get; set; }




        [NopResourceDisplayName("Admin.Activity.Fields.SpentTime")]
        [Required(ErrorMessage = "Spent Hours Must be Positive Value")]
        [Range(0, double.MaxValue, ErrorMessage = "Spent Hours Must be Positive Value")]
        public int SpentHours { get; set; }
        public int SpentMinutes { get; set; }

        [NopResourceDisplayName("Admin.Activity.Fields.SpentTime")]

        public string SpentTime { get; set; }


      

        public string QualityComments { get; set; }

        public string EmployeeName { get; set; }


        [Required(ErrorMessage = "Please select Employee")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Employee")]
        [NopResourceDisplayName("Admin.Activity.Fields.EmployeeId")]
        public int EmployeeId { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }

        public IList<SelectListItem> AvailableProjects { get; set; }


        public IList<SelectListItem> AvailableTasks { get; set; }


        public DateTime CreatedOn { get; set; }
        public DateTime UpdateOn { get; set; }


        public DateTime CreatedOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }


    }
}
