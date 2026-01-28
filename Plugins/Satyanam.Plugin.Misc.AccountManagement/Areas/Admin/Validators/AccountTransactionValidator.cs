using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountTransactions;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class AccountTransactionValidator : BaseNopValidator<AccountTransactionModel>
{
    #region Ctor

    public AccountTransactionValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Currency).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.Currency.Required"));

        RuleFor(x => x.Amount).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.Amount.Required"));

        RuleFor(x => x.Amount).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.Amount.Required"));

        RuleFor(x => x.PaymentMethodId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.PaymentMethodId.Required"));

        RuleFor(x => x.PaymentMethodId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountTransaction.Fields.PaymentMethodId.Required"));
    }

    #endregion
}
