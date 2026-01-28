using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record PasswordRecoveryConfirmModel : BaseNopModel
    {
        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.PasswordRecovery.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.PasswordRecovery.ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }

        public bool DisablePasswordChanging { get; set; }
        public string Result { get; set; }

        public string ReturnUrl { get; set; }
    }
}