using System;
using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record ExternalAuthenticationMethodModel : BaseNopModel
    {
        public Type ViewComponent { get; set; }
    }
}