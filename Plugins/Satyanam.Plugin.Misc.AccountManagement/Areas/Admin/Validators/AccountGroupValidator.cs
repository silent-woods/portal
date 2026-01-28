using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.AccountGroups;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class AccountGroupValidator : BaseNopValidator<AccountGroupModel>
{
    #region Ctor

    public AccountGroupValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.AccountGroup.Fields.Name.Required"));
    }

    #endregion
}
