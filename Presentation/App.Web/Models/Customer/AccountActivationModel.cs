using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record AccountActivationModel : BaseNopModel
    {
        public string Result { get; set; }

        public string ReturnUrl { get; set; }
    }
}