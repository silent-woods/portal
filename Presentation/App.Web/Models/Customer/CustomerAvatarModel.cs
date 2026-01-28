using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record CustomerAvatarModel : BaseNopModel
    {
        public string AvatarUrl { get; set; }
    }
}