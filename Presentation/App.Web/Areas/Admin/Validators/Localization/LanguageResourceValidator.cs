using FluentValidation;
using App.Core.Domain.Localization;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Localization;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Localization
{
    public partial class LanguageResourceValidator : BaseNopValidator<LocaleResourceModel>
    {
        public LanguageResourceValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            //if validation without this set rule is applied, in this case nothing will be validated
            //it's used to prevent auto-validation of child models
            RuleSet(NopValidationDefaults.ValidationRuleSet, () =>
            {
                RuleFor(model => model.ResourceName)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Languages.Resources.Fields.Name.Required"));

                RuleFor(model => model.ResourceValue)
                    .NotEmpty()
                    .WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Languages.Resources.Fields.Value.Required"));

                SetDatabaseValidationRules<LocaleStringResource>(mappingEntityAccessor);
            });
        }
    }
}