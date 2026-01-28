using FluentValidation;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Customer;

namespace App.Web.Validators.Customer
{
    public partial class GiftCardValidator : BaseNopValidator<CheckGiftCardBalanceModel>
    {
        public GiftCardValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.GiftCardCode).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("CheckGiftCardBalance.GiftCardCouponCode.Empty"));            
        }
    }
}
