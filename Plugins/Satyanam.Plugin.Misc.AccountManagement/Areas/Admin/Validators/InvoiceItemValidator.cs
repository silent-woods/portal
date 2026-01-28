using App.Services.Localization;
using App.Web.Framework.Validators;
using FluentValidation;
using Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Models.InvoiceItems;

namespace Satyanam.Plugin.Misc.AccountManagement.Areas.Admin.Validators;

public partial class InvoiceItemValidator : BaseNopValidator<InvoiceItemModel>
{
    #region Ctor

    public InvoiceItemValidator(ILocalizationService localizationService)
    {
        RuleFor(x => x.Description).NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Satyanam.Plugin.Misc.AccountManagement.Admin.InvoiceItem.Fields.Description.Required"));
    }

    #endregion
}
