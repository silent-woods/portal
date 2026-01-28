using FluentValidation;
using App.Core.Domain.ScheduleTasks;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Tasks;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Tasks
{
    public partial class ScheduleTaskValidator : BaseNopValidator<ScheduleTaskModel>
    {
        public ScheduleTaskValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.System.ScheduleTasks.Name.Required"));
            RuleFor(x => x.Seconds).GreaterThan(0).WithMessageAwait(localizationService.GetResourceAsync("Admin.System.ScheduleTasks.Seconds.Positive"));

            SetDatabaseValidationRules<ScheduleTask>(mappingEntityAccessor);
        }
    }
}