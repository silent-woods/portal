using FluentValidation;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.PrivateMessages;

namespace App.Web.Validators.PrivateMessages
{
    public partial class SendPrivateMessageValidator : BaseNopValidator<SendPrivateMessageModel>
    {
        public SendPrivateMessageValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("PrivateMessages.SubjectCannotBeEmpty"));
            RuleFor(x => x.Message).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("PrivateMessages.MessageCannotBeEmpty"));
        }
    }
}