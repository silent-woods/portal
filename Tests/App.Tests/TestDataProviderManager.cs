using App.Core;
using App.Core.Infrastructure;
using App.Data;
using App.Data.Configuration;
using App.Data.DataProviders;

namespace App.Tests
{
    /// <summary>
    /// Represents the data provider manager
    /// </summary>
    public partial class TestDataProviderManager : IDataProviderManager
    {
        #region Properties

        /// <summary>
        /// Gets data provider
        /// </summary>
        public INopDataProvider DataProvider
        {
            get
            {
                return Singleton<DataConfig>.Instance.DataProvider switch
                {
                    DataProviderType.SqlServer => new MsSqlNopDataProvider(),
                    DataProviderType.MySql => new MySqlNopDataProvider(),
                    DataProviderType.PostgreSQL => new PostgreSqlDataProvider(),
                    DataProviderType.Unknown => new SqLiteNopDataProvider(),
                    _ => throw new NopException($"Unknown [{Singleton<DataConfig>.Instance.DataProvider}] DataProvider")
                };
            }
        }

        #endregion
    }
}
