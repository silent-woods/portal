using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record ProjectTaskSettingsModel : BaseNopModel, ISettingsModel
    {

        public int ActiveStoreScopeConfiguration { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.ProjectTask.IsShowSelctAllCheckList")]
        public bool IsShowSelctAllCheckList { get; set; }

        public bool IsShowSelctAllCheckList_OverrideForStore { get; set; }



    }
}
