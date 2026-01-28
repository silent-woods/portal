using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Directory
{
    /// <summary>
    /// Represents a currency exchange rate model
    /// </summary>
    public partial record CurrencyExchangeRateModel : BaseNopModel
    {
        #region Properties

        public string CurrencyCode { get; set; }

        public decimal Rate { get; set; }

        #endregion
    }
}