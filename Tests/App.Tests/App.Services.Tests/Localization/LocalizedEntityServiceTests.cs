using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using App.Core.Domain.Catalog;
using App.Services.Catalog;
using App.Services.Localization;
using NUnit.Framework;

namespace App.Tests.App.Services.Tests.Localization
{
    [TestFixture]
    public class LocalizedEntityServiceTests : BaseNopTest
    {
        private ILocalizedEntityService _localizedEntityService;

        [OneTimeSetUp]
        public void SetUp()
        {
            _localizedEntityService = GetService<ILocalizedEntityService>();
        }

    }
}