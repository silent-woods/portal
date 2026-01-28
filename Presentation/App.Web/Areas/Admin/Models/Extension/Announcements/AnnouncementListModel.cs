using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.Extension.Announcements

{
    /// <summary>
    /// Represents a project list model
    /// </summary>
    public partial record AnnouncementListModel : BasePagedListModel<AnnouncementModel>
    {
    
    }
}
