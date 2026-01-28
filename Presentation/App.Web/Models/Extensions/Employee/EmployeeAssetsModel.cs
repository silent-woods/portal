using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Models.Employee

{
    /// <summary>
    /// Represents a Assets model
    /// </summary>
    public partial record EmployeeAssetsModel : BaseNopEntityModel
    {

        public EmployeeAssetsModel()
        {
            Assets = new List<EmployeeAssetsModel>();
            Employess = new List<SelectListItem>();
            Types = new List<SelectListItem>();
        }
        #region Properties

        [NopResourceDisplayName("Account.Employee.Assets.Fields.EmployeeID")]
        public int EmployeeID { get; set; }

        [NopResourceDisplayName("Account.Employee.Assets.Fields.EmployeeName")]
        public string EmployeeName { get; set; }
        public IList<SelectListItem> Employess { get; set; }

        [NopResourceDisplayName("Account.Employee.Assets.Fields.TypeId")]
        public int TypeId { get; set; }
        public IList<SelectListItem> Types { get; set; }

        [NopResourceDisplayName("Account.Employee.Assets.Fields.Type")]
        public string Type { get; set; }


        [NopResourceDisplayName("Account.Employee.Assets.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Account.Employee.Assets.Fields.DocumentId")]
        [UIHint("Download")]
        public int DocumentId { get; set; }

        [NopResourceDisplayName("Account.Employee.Assets.Fields.Description")]
        public string Description { get; set; }

        public IList<EmployeeAssetsModel> Assets { get; set; }

        #endregion
    }
}