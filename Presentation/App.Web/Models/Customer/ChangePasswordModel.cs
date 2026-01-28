using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record ChangePasswordModel : BaseNopModel
    {
        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.ChangePassword.Fields.OldPassword")]
        public string OldPassword { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.ChangePassword.Fields.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.ChangePassword.Fields.ConfirmNewPassword")]
        public string ConfirmNewPassword { get; set; }
    }
}