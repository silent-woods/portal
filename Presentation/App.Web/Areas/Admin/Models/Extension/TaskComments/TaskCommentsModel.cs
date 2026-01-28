using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.TaskComments
{
    public partial record TaskCommentsModel : BaseNopEntityModel
    {

    
       public TaskCommentsModel()
        {
        
        }
        public int TaskId { get; set; }

        public string TaskName { get; set; }


        public int StatusId { get; set; }

        public string StatusName { get; set; }


        public string Description { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }


        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }
    }
}
