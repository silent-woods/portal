using App.Web.Framework.Models;
using App.Web.Models.Common;

namespace App.Web.Models.Customer
{
    public partial record CustomerAddressEditModel : BaseNopModel
    {
        public CustomerAddressEditModel()
        {
            Address = new AddressModel();
        }
        
        public AddressModel Address { get; set; }
    }
}