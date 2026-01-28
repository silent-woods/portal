using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.TaskChangeLogs
{
    public partial record TaskChangeLogSearchModel : BaseSearchModel
    {
        public TaskChangeLogSearchModel() {
          
        }
        #region Properties

        
        public int SearchTaskId { get; set; }

        public int SearchEmployeeId { get; set; }

        public int SearchStatusId { get; set; }

        public int SearchLogTypeId { get; set; }

        public int SearchAssignedToId { get; set; }



        public DateTime? From { get; set; }

        public DateTime? To { get; set; }



        #endregion
    }
}
