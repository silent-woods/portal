using App.Web.Framework.Models;

namespace App.Web.Models.Blogs
{
    public partial record BlogPostTagModel : BaseNopModel
    {
        public string Name { get; set; }

        public int BlogPostCount { get; set; }
    }
}