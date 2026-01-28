using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Models.Customer
{
    public partial record CheckGiftCardBalanceModel : BaseNopModel
    {
        public string Result { get; set; }

        public string Message { get; set; }
        
        [NopResourceDisplayName("ShoppingCart.GiftCardCouponCode.Tooltip")]
        public string GiftCardCode { get; set; }
    }
}
