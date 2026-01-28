using System.Threading.Tasks;
using FluentAssertions;
using App.Core.Domain.Customers;
using App.Web.Factories;
using NUnit.Framework;

namespace App.Tests.App.Web.Tests.Public.Factories
{
    [TestFixture]
    public class NewsletterModelFactoryTests : BaseNopTest
    {
        private INewsletterModelFactory _newsletterModelFactory;

        [OneTimeSetUp]
        public void SetUp()
        {
            _newsletterModelFactory = GetService<INewsletterModelFactory>();
        }

        [Test]
        public async Task CanPrepareNewsletterBoxModel()
        {
            var model = await _newsletterModelFactory.PrepareNewsletterBoxModelAsync();

            model.AllowToUnsubscribe.Should().Be(GetService<CustomerSettings>().NewsletterBlockAllowToUnsubscribe);
        }

        [Test]
        public async Task CanPrepareSubscriptionActivationModel()
        {
            var activated = (await _newsletterModelFactory.PrepareSubscriptionActivationModelAsync(true)).Result;

            var deactivated = (await _newsletterModelFactory.PrepareSubscriptionActivationModelAsync(false)).Result;

            activated.Should().NotBe(deactivated);
        }
    }
}
