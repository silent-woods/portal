using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.TaskChangeLogs
{
    public partial record TaskChangeLogModel : BaseNopEntityModel
    {

    
       public TaskChangeLogModel()
        {
        
        }
        public int TaskId { get; set; }

        public string TaskName { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; }


        public int AssignedTo { get; set; }

        public string AssignedToName { get; set; }


        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; }


        public int LogTypeId { get; set; }

        public string LogTypeName { get; set; }


        public string Notes { get; set; }


        public DateTime CreatedOn { get; set; }
    }
}
