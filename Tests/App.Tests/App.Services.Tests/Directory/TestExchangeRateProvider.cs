using System.Collections.Generic;
using System.Threading.Tasks;
using App.Core.Domain.Directory;
using App.Services.Directory;
using App.Services.Plugins;

namespace App.Tests.App.Services.Tests.Directory
{
    public class TestExchangeRateProvider : BasePlugin, IExchangeRateProvider
    {
        /// <summary>
        /// Gets currency live rates
        /// </summary>
        /// <param name="exchangeRateCurrencyCode">Exchange rate currency code</param>
        /// <returns>Exchange rates</returns>
        public Task<IList<ExchangeRate>> GetCurrencyLiveRatesAsync(string exchangeRateCurrencyCode)
        {
            return Task.FromResult<IList<ExchangeRate>>(new List<ExchangeRate>());
        }
    }
}
