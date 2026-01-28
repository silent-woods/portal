using System.Collections.Generic;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Models.Customer
{
    public partial record MultiFactorAuthenticationModel : BaseNopModel
    {
        public MultiFactorAuthenticationModel()
        {
            Providers = new List<MultiFactorAuthenticationProviderModel>();
        }

        [NopResourceDisplayName("Account.MultiFactorAuthentication.Fields.IsEnabled")]
        public bool IsEnabled { get; set; }

        public List<MultiFactorAuthenticationProviderModel> Providers { get; set; }

        public string Message { get; set; }
        
    }
}
