using FluentValidation;
using App.Core.Domain.Messages;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Messages
{
    public partial class MessageTemplateValidator : BaseNopValidator<MessageTemplateModel>
    {
        public MessageTemplateValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Fields.Subject.Required"));
            RuleFor(x => x.Body).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.ContentManagement.MessageTemplates.Fields.Body.Required"));

            SetDatabaseValidationRules<MessageTemplate>(mappingEntityAccessor);
        }
    }
}