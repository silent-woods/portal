using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace App.Web.Areas.Admin.Models.Settings
{
    public partial record TeamPerformanceSettingsModel : BaseNopModel, ISettingsModel
    {

        public TeamPerformanceSettingsModel()
        {
            AvailableFeedBackShow = new List<SelectListItem>();

            AvailableDates = new List<SelectListItem>();
        }
        public int ActiveStoreScopeConfiguration { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.TeamPerformance.FeedBack.FeedbackShowId")]
       
        public int FeedbackShowId { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.TeamPerformance.FeedBack.StartReminderDate")]
        public int StartReminderDate { get; set; }

        public bool StartReminderDate_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.TeamPerformance.FeedBack.StartCCDate")]
        public int StartCCDate { get; set; }

        public bool StartCCDate_OverrideForStore { get; set; }



        public bool FeedbackShowId_OverrideForStore { get; set; }

        
        public IList<SelectListItem> AvailableFeedBackShow { get; set; }

        public IList<SelectListItem> AvailableDates { get; set; }

       




    }
}
