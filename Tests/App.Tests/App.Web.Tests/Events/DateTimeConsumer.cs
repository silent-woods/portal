using System;
using System.Threading.Tasks;
using App.Services.Events;

namespace App.Tests.App.Web.Tests.Events
{
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
