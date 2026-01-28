using App.Web.Framework.Models;

namespace App.Web.Models.Sitemap
{
    public partial record SitemapXmlModel : BaseNopModel
    {
        public string SitemapXmlPath { get; set; }
    }
}