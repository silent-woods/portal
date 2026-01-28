using App.Core.Domain.Customers;
using App.Web.Framework.Models;

namespace App.Web.Models.Common
{
    public partial record HeaderLinksModel : BaseNopModel
    {
        public bool IsAuthenticated { get; set; }
        public string CustomerName { get; set; }
        
        public bool AllowPrivateMessages { get; set; }
        public string UnreadPrivateMessages { get; set; }
        public string AlertMessage { get; set; }
        public UserRegistrationType RegistrationType { get; set; }
    }
}