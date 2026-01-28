using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a Assets model
    /// </summary>
    public partial record AssetsModel : BaseNopEntityModel
    {

        public AssetsModel()
        {
            Employess = new List<SelectListItem>();
            Types = new List<SelectListItem>();

            SelectedEmployeeId = new List<int>();
            AvailableEmployees = new List<SelectListItem>();
        }
        #region Properties
      
        [NopResourceDisplayName("Admin.Employee.Assets.Fields.EmployeeID")]
        public int EmployeeID { get; set; }

        [NopResourceDisplayName("Admin.Employee.Assets.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employess { get; set; }

        [Required(ErrorMessage = "Please select Type.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Type.")]
        [NopResourceDisplayName("Admin.Employee.Assets.Fields.TypeId")]
        public int TypeId { get; set; }
        public IList<SelectListItem> Types { get; set; }

        [NopResourceDisplayName("Admin.Employee.Assets.Fields.Type")]
        public string Type { get; set; }

        [Required(ErrorMessage = "Please enter Name")]
        [NopResourceDisplayName("Admin.Employee.Assets.Fields.Name")]
        public string Name { get; set; }

       
        [NopResourceDisplayName("Admin.Employee.Assets.Fields.DocumentId")]
        [UIHint("Download")]
        public int DocumentId { get; set; }


        public IList<SelectListItem> AvailableEmployees { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedEmployeeId")]
        public IList<int> SelectedEmployeeId { get; set; }
        public Guid DownloadGuid { get; set; }

        [Required(ErrorMessage = "Please enter Description")]
        [NopResourceDisplayName("Admin.Employee.Assets.Fields.Description")]
        public string Description { get; set; }
        #endregion
    }
}