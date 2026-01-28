using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReason;

public partial record TaskAlertReasonModel : BaseNopEntityModel
{
    #region Properties

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReason.Fields.Name")]
    [Required(ErrorMessage = "Please enter a name for the reason.")]
    public string Name { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReason.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertReason.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    #endregion
}
