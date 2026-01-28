using App.Web.Framework.Models;

namespace Satyanam.Nop.Plugin.SatyanamCRM.Models.Contacts
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