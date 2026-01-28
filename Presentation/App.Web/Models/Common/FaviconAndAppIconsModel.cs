using App.Web.Framework.Models;

namespace App.Web.Models.Common
{
    public partial record FaviconAndAppIconsModel : BaseNopModel
    {
        public string HeadCode { get; set; }
    }
}