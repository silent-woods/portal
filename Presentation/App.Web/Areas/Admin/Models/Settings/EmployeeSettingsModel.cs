using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record EmployeeSettingsModel : BaseNopModel, ISettingsModel
    {
        public EmployeeSettingsModel()
        {
            AvailableRoles = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.Employee.OnBoardingEmail.Body")]
    
        public string OnBoardingEmail { get; set; }

        public bool OnBoardingEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.Employee.CoordinatorRoleId")]
        public int CoordinatorRoleId { get; set; }

        public bool CoordinatorRoleId_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableRoles { get; set; }
    }
}
