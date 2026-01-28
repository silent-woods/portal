using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertConfiguration;
using App.Web.Framework.Validators;
using FluentValidation;

namespace App.Web.Areas.Admin.Validators.TaskAlerts;

public class TaskAlertConfigurationValidator : BaseNopValidator<TaskAlertConfigurationModel>
{
    #region Methods

    public TaskAlertConfigurationValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TaskAlert.TaskAlertConfiguration.Fields.Message.Required"));
    }

    #endregion
}
