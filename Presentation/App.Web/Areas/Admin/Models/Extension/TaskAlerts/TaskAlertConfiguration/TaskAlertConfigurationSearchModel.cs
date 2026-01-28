using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertConfiguration;

public partial record TaskAlertConfigurationSearchModel : BaseSearchModel
{
    #region Ctor

    public TaskAlertConfigurationSearchModel()
    {
        AvailableTaskAlertTypes = new List<SelectListItem>();
    }

    #endregion

    #region Properties

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.SearchTaskAlertTypeId")]
    public int SearchTaskAlertTypeId { get; set; }

    public IList<SelectListItem> AvailableTaskAlertTypes { get; set; }

    #endregion
}
