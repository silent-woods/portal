using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.PaymentTerms;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class PaymentTermValidator : BaseNopValidator<PaymentTermModel>
{
    #region Ctor

    public PaymentTermValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Name).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.PaymentTerm.Fields.Name.Required"));
    }

    #endregion
}
