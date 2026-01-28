using FluentValidation;
using App.Core.Domain.Messages;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Messages
{
    public partial class EmailAccountValidator : BaseNopValidator<EmailAccountModel>
    {
        public EmailAccountValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Admin.Common.WrongEmail"));

            RuleFor(x => x.DisplayName).NotEmpty();

            SetDatabaseValidationRules<EmailAccount>(mappingEntityAccessor);
        }
    }
}