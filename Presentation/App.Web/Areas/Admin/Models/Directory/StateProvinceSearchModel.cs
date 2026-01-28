using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a state and province search model
    /// </summary>
    public partial record StateProvinceSearchModel : BaseSearchModel
    {
        #region Properties

        public int CountryId { get; set; }

        #endregion
    }
}