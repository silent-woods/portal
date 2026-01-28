using App.Web.Framework.Models;

namespace App.Web.Models.Customer
{
    public partial record GdprToolsModel : BaseNopModel
    {
        public string Result { get; set; }
    }
}