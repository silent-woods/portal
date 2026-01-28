using System;
using System.Threading.Tasks;
using FluentAssertions;
using App.Core.Events;
using App.Services.Events;
using NUnit.Framework;

namespace App.Tests.App.Web.Tests.Events
{
    [TestFixture]
    public class EventsTests : BaseNopTest
    {
        private IEventPublisher _eventPublisher;

        [OneTimeSetUp]
        public void SetUp()
        {
            _eventPublisher = GetService<IEventPublisher>();
        }

        [Test]
        public async Task CanPublishEvent()
        {
            var oldDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(7));
            DateTimeConsumer.DateTime = oldDateTime;

            var newDateTime = DateTime.Now.Subtract(TimeSpan.FromDays(5));
            await _eventPublisher.PublishAsync(newDateTime);
            newDateTime.Should().Be(DateTimeConsumer.DateTime);
        }

        public class DateTimeConsumer : IConsumer<DateTime>
        {
            public Task HandleEventAsync(DateTime eventMessage)
            {
                DateTime = eventMessage;

                return Task.CompletedTask;
            }

            // For testing
            public static DateTime DateTime { get; set; }
        }
    }
}