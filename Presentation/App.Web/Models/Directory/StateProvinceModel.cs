using App.Web.Framework.Models;

namespace App.Web.Models.Directory
{
    public partial record StateProvinceModel : BaseNopModel
    {
        public int id { get; set; }
        public string name { get; set; }
    }
}