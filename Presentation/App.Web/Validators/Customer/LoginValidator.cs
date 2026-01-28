using FluentValidation;
using App.Core.Domain.Customers;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Customer;

namespace App.Web.Validators.Customer
{
    public partial class LoginValidator : BaseNopValidator<LoginModel>
    {
        public LoginValidator(ILocalizationService localizationService, CustomerSettings customerSettings)
        {
            if (!customerSettings.UsernamesEnabled)
            {
                //login by email
                RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Account.Login.Fields.Email.Required"));
                RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            }
        }
    }
}