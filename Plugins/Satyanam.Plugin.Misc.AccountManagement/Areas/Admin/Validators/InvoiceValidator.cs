using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.Invoices;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class InvoiceValidator : BaseNopValidator<InvoiceModel>
{
    #region Ctor

    public InvoiceValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.InvoiceNumber).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.InvoiceNumber.Required"));

        RuleFor(x => x.Title).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.Title.Required"));

        RuleFor(x => x.ProjectBillingId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.ProjectBillingId.Required"));

        RuleFor(x => x.ProjectBillingId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.ProjectBillingId.Required"));

        RuleFor(x => x.AccountGroupId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.AccountGroupId.Required"));

        RuleFor(x => x.AccountGroupId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.AccountGroupId.Required"));

        RuleFor(x => x.BankAccountId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.BankAccountId.Required"));

        RuleFor(x => x.BankAccountId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.Invoice.Fields.BankAccountId.Required"));
    }

    #endregion
}
