using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Models.Messages
{
    /// <summary>
    /// Represents a newsletter subscription model
    /// </summary>
    public partial record NewsletterSubscriptionModel : BaseNopEntityModel
    {
        #region Properties

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Active")]
        public bool Active { get; set; }

        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn")]
        public string CreatedOn { get; set; }

        #endregion
    }
}