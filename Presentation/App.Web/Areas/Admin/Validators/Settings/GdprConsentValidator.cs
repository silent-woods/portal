using FluentValidation;
using App.Core.Domain.Gdpr;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Settings;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Settings
{
    public partial class GdprConsentValidator : BaseNopValidator<GdprConsentModel>
    {
        public GdprConsentValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Message).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.Message.Required"));
            RuleFor(x => x.RequiredMessage)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Settings.Gdpr.Consent.RequiredMessage.Required"))
                .When(x => x.IsRequired);

            SetDatabaseValidationRules<GdprConsent>(mappingEntityAccessor);
        }
    }
}