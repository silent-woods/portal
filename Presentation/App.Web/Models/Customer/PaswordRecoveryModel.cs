using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record PasswordRecoveryModel : BaseNopModel
    {
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.PasswordRecovery.Email")]
        public string Email { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}