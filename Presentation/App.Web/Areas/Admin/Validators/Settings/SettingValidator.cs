using FluentValidation;
using App.Core.Domain.Configuration;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Settings;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Settings
{
    public partial class SettingValidator : BaseNopValidator<SettingModel>
    {
        public SettingValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Settings.AllSettings.Fields.Name.Required"));

            SetDatabaseValidationRules<Setting>(mappingEntityAccessor);
        }
    }
}