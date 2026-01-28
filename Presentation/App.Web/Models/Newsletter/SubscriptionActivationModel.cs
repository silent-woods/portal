using App.Web.Framework.Models;

namespace App.Web.Models.Newsletter
{
    public partial record SubscriptionActivationModel : BaseNopModel
    {
        public string Result { get; set; }
    }
}