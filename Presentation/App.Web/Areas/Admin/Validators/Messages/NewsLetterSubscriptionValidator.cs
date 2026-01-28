using FluentValidation;
using App.Core.Domain.Messages;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Messages
{
    public partial class NewsLetterSubscriptionValidator : BaseNopValidator<NewsletterSubscriptionModel>
    {
        public NewsLetterSubscriptionValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotions.NewsLetterSubscriptions.Fields.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Admin.Common.WrongEmail"));

            SetDatabaseValidationRules<NewsLetterSubscription>(mappingEntityAccessor);
        }
    }
}