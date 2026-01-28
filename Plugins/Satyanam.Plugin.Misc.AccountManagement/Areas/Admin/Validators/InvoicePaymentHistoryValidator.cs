using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoicePaymentHistories;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class InvoicePaymentHistoryValidator : BaseNopValidator<InvoicePaymentHistoryModel>
{
    #region Ctor

    public InvoicePaymentHistoryValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.AmountInPaymentCurrency).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.AmountInPaymentCurrency.Required"));

        RuleFor(x => x.AmountInPaymentCurrency).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.AmountInPaymentCurrency.Required"));

        RuleFor(x => x.AmountInINR).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.AmountInINR.Required"));

        RuleFor(x => x.AmountInINR).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.AmountInINR.Required"));

        RuleFor(x => x.PaymentMethodId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.PaymentMethodId.Required"));

        RuleFor(x => x.PaymentMethodId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoicePaymentHistory.Fields.PaymentMethodId.Required"));
    }

    #endregion
}
