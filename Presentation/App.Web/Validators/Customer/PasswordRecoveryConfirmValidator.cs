using FluentValidation;
using App.Core.Domain.Customers;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Customer;

namespace App.Web.Validators.Customer
{
    public partial class PasswordRecoveryConfirmValidator : BaseNopValidator<PasswordRecoveryConfirmModel>
    {
        public PasswordRecoveryConfirmValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            RuleFor(x => x.NewPassword).IsPassword(localizationService, customerSettings);            
            RuleFor(x => x.ConfirmNewPassword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.PasswordRecovery.ConfirmNewPassword.Required"));
            RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword).WithMessageAwait(localizationService.GetResourceAsync("Account.PasswordRecovery.NewPassword.EnteredPasswordsDoNotMatch"));
        }
    }
}