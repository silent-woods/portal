using App.Core.Configuration;

namespace App.Core.Domain.Extension.Alerts
{
    public partial class EmailSettings : ISettings
    {
        #region Properties

        public decimal FirstMailVariation { get; set; }

        public decimal SecondMailVariation { get; set; }

        public decimal ThirdMailVariation { get; set; }

        #endregion
    }
}
