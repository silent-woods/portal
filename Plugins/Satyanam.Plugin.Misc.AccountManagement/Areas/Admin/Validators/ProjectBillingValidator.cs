using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.ProjectBillings;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class ProjectBillingValidator : BaseNopValidator<ProjectBillingModel>
{
    #region Ctor

    public ProjectBillingValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.BillingName).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingName.Required"));

        RuleFor(x => x.ProjectId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.ProjectId.Required"));

        RuleFor(x => x.ProjectId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.ProjectId.Required"));

        RuleFor(x => x.CompanyId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.CompanyId.Required"));

        RuleFor(x => x.CompanyId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.CompanyId.Required"));

        RuleFor(x => x.PaymentTermId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentTermId.Required"));

        RuleFor(x => x.PaymentTermId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentTermId.Required"));

        RuleFor(x => x.BillingTypeId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingTypeId.Required"));

        RuleFor(x => x.BillingTypeId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.BillingTypeId.Required"));

        RuleFor(x => x.PrimaryCurrencyId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PrimaryCurrencyId.Required"));

        RuleFor(x => x.PrimaryCurrencyId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PrimaryCurrencyId.Required"));

        RuleFor(x => x.PaymentCurrencyId).NotNull()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentCurrencyId.Required"));

        RuleFor(x => x.PaymentCurrencyId).NotEqual(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.ProjectBilling.Fields.PaymentCurrencyId.Required"));
    }

    #endregion
}
