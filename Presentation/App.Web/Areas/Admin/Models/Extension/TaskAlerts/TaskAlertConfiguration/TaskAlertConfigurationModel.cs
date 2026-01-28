using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertConfiguration;

public partial record TaskAlertConfigurationModel : BaseNopEntityModel
{
	#region Ctor

	public TaskAlertConfigurationModel()
	{
        AvailableTaskAlertTypes = new List<SelectListItem>();
    }

	#endregion

	#region Properties

	[NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.TaskAlertTypeId")]
	public int TaskAlertTypeId { get; set; }

    public string TaskAlertType { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.Message")]
    [Required(ErrorMessage = "Please enter a message for the alert.")]
    public string Message { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.Percentage")]
    public decimal Percentage { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableComment")]
    public bool EnableComment { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.CommentRequired")]
    public bool CommentRequired { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableReason")]
    public bool EnableReason { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.ReasonRequired")]
    public bool ReasonRequired { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableCoordinatorMail")]
    public bool EnableCoordinatorMail { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableLeaderMail")]
    public bool EnableLeaderMail { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableManagerMail")]
    public bool EnableManagerMail { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableDeveloperMail")]
    public bool EnableDeveloperMail { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.NewETA")]
    public bool NewETA { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.EnableOnTrack")]
    public bool EnableOnTrack { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.IsActive")]
    public bool IsActive { get; set; }

    [NopResourceDisplayName("Admin.TaskAlert.TaskAlertConfiguration.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

	public IList<SelectListItem> AvailableTaskAlertTypes { get; set; }

    #endregion
}
