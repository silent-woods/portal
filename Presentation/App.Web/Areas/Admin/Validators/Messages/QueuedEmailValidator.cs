using FluentValidation;
using App.Core.Domain.Messages;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Messages
{
    public partial class QueuedEmailValidator : BaseNopValidator<QueuedEmailModel>
    {
        public QueuedEmailValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.From).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.System.QueuedEmails.Fields.From.Required"));
            RuleFor(x => x.To).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.System.QueuedEmails.Fields.To.Required"));

            RuleFor(x => x.SentTries).NotNull().WithMessageAwait(localizationService.GetResourceAsync("Admin.System.QueuedEmails.Fields.SentTries.Required"))
                                    .InclusiveBetween(0, 99999).WithMessageAwait(localizationService.GetResourceAsync("Admin.System.QueuedEmails.Fields.SentTries.Range"));

            SetDatabaseValidationRules<QueuedEmail>(mappingEntityAccessor);

        }
    }
}