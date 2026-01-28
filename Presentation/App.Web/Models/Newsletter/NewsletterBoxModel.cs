using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Models;

namespace App.Web.Models.Newsletter
{
    public partial record NewsletterBoxModel : BaseNopModel
    {
        [DataType(DataType.EmailAddress)]
        public string NewsletterEmail { get; set; }
        public bool AllowToUnsubscribe { get; set; }
    }
}