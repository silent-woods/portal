using FluentValidation;
using App.Core.Domain.Topics;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Templates;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Templates
{
    public partial class TopicTemplateValidator : BaseNopValidator<TopicTemplateModel>
    {
        public TopicTemplateValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.System.Templates.Topic.Name.Required"));
            RuleFor(x => x.ViewPath).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.System.Templates.Topic.ViewPath.Required"));

            SetDatabaseValidationRules<TopicTemplate>(mappingEntityAccessor);
        }
    }
}