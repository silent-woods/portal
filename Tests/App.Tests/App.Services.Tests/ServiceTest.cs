using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Moq;
using App.Core;
using App.Core.Infrastructure;
using App.Data.Configuration;
using App.Services.Plugins;
using App.Tests.App.Services.Tests.Directory;
using NUnit.Framework;

namespace App.Tests.App.Services.Tests
{
    [TestFixture]
    public abstract class ServiceTest : BaseNopTest
    {
        protected ServiceTest()
        {
            //init plugins
            InitPlugins();
        }

        private static void InitPlugins()
        {
            var webHostEnvironment = new Mock<IWebHostEnvironment>();
            webHostEnvironment.Setup(x => x.ContentRootPath).Returns(System.Reflection.Assembly.GetExecutingAssembly().Location);
            webHostEnvironment.Setup(x => x.WebRootPath).Returns(System.IO.Directory.GetCurrentDirectory());
            CommonHelper.DefaultFileProvider = new NopFileProvider(webHostEnvironment.Object);
            
            Environment.SetEnvironmentVariable("ConnectionStrings", Singleton<DataConfig>.Instance.ConnectionString);

            Singleton<IPluginsInfo>.Instance = new PluginsInfo(CommonHelper.DefaultFileProvider)
            {
                PluginDescriptors = new List<(PluginDescriptor, bool)>
                {
                    (new PluginDescriptor
                    {
                        PluginType = typeof(TestExchangeRateProvider),
                        SystemName = "CurrencyExchange.TestProvider",
                        FriendlyName = "Test exchange rate provider",
                        Installed = true,
                        ReferencedAssembly = typeof(TestExchangeRateProvider).Assembly
                    }, true)
                }
            };
        }
    }
}
