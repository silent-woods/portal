using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.Activities
{
    public partial record ActivitySearchModel : BaseSearchModel
    {
        public ActivitySearchModel() {
            AvailableProject = new List<SelectListItem>();
            AvailableTask = new List<SelectListItem>();
            AvailableEmployee = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Admin.Activity.Fields.SearchProjectId")]

        public int SearchProjectId { get; set; }


        [NopResourceDisplayName("Admin.Activity.Fields.SearchEmployeeId")]

        public int SearchEmployeeId { get; set; }

        [NopResourceDisplayName("Admin.Activity.Fields.SearchActivityName")]

        public string SearchActivityName { get; set; }

        public string SearchEmployeeName { get; set; }


        public string SearchProjectName { get; set; }
        public IList<SelectListItem> AvailableProject { get; set; }

        public IList<SelectListItem> AvailableEmployee { get; set; }


        public IList<SelectListItem> AvailableTask { get; set; }


        [NopResourceDisplayName("Admin.Activity.Fields.SearchTaskTitle")]
        public string SearchTaskTitle { get; set; }


        [NopResourceDisplayName("Admin.Activity.Fields.SearchTaskId")]

        public int SearchTaskId { get; set; }






        #endregion
    }
}
