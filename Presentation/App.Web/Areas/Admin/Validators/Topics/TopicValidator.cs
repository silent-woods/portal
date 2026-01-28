using FluentValidation;
using App.Core.Domain.Topics;
using App.Data.Mapping;
using App.Services.Localization;
using App.Services.Seo;
using App.Web.Areas.Admin.Models.Topics;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Topics
{
    public partial class TopicValidator : BaseNopValidator<TopicModel>
    {
        public TopicValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.SeName)
                .Length(0, NopSeoDefaults.ForumTopicLength)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.SEO.SeName.MaxLengthValidation"), NopSeoDefaults.ForumTopicLength);

            RuleFor(x => x.Password)
                .NotEmpty()
                .When(x => x.IsPasswordProtected)
                .WithMessageAwait(localizationService.GetResourceAsync("Validation.Password.IsNotEmpty"));

            SetDatabaseValidationRules<Topic>(mappingEntityAccessor);
        }
    }
}
