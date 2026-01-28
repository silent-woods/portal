using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace Satyanam.Nop.Core.Models.Contacts
{
    public record ContactsDealsSearchModel : BaseSearchModel
    {
        public ContactsDealsSearchModel()
        {
            
        }

        #region Properties

        public int ContactId { get; set; }
        
        
        
        #endregion
    }
}