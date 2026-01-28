using AutoMapper;
using App.Core.Infrastructure.Mapper;
using App.Web.Areas.Admin.Infrastructure.Mapper;
using NUnit.Framework;

namespace App.Tests.App.Web.Tests.Admin.Infrastructure
{
    [TestFixture]
    public class AutoMapperConfigurationTest
    {
        [Test]
        public void ConfigurationIsValid()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(typeof(AdminMapperConfiguration));
            });
            
            AutoMapperConfiguration.Init(config);
            AutoMapperConfiguration.MapperConfiguration.AssertConfigurationIsValid();
        }
    }
}