using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.BankAccounts;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class BankAccountValidator : BaseNopValidator<BankAccountModel>
{
    #region Ctor

    public BankAccountValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Title).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.Title.Required"));

        RuleFor(x => x.BankName).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.BankName.Required"));

        RuleFor(x => x.AccountNo).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.AccountNo.Required"));

        RuleFor(x => x.AccountName).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.BankAccount.Fields.AccountName.Required"));
    }

    #endregion
}
