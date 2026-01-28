using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents an email account model
    /// </summary>
    public partial record EmailAccountModel : BaseNopEntityModel
    {
        #region Properties

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.DisplayName")]
        public string DisplayName { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Host")]
        public string Host { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Port")]
        public int Port { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Username")]
        public string Username { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.EnableSsl")]
        public bool EnableSsl { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.UseDefaultCredentials")]
        public bool UseDefaultCredentials { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.IsDefaultEmailAccount")]
        public bool IsDefaultEmailAccount { get; set; }

        [NopResourceDisplayName("Admin.Configuration.EmailAccounts.Fields.SendTestEmailTo")]
        public string SendTestEmailTo { get; set; }

        #endregion
    }
}