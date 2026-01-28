using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.TaskAlerts.TaskAlertReason;
using App.Web.Framework.Validators;
using FluentValidation;

namespace App.Web.Areas.Admin.Validators.TaskAlerts;

public class TaskAlertReasonValidator : BaseNopValidator<TaskAlertReasonModel>
{
    #region Methods

    public TaskAlertReasonValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.TaskAlert.TaskAlertReason.Fields.Name.Required"));
    }

    #endregion
}
