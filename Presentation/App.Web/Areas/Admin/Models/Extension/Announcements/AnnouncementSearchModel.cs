using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.Announcements
{
    public partial record AnnouncementSearchModel : BaseSearchModel
    {
        public AnnouncementSearchModel() {
          
        }
        #region Properties

        [NopResourceDisplayName("Admin.Announcement.Fields.SearchTitle")]

        public string SearchTitle { get; set; }



        #endregion
    }
}
