using System.Collections.Generic;
using App.Web.Framework.Models;
using App.Web.Models.Common;

namespace App.Web.Models.Customer
{
    public partial record CustomerAddressListModel : BaseNopModel
    {
        public CustomerAddressListModel()
        {
            Addresses = new List<AddressModel>();
        }

        public IList<AddressModel> Addresses { get; set; }
    }
}