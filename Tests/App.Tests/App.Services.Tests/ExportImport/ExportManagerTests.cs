using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using FluentAssertions;
using App.Core;
using App.Core.Domain.Catalog;
using App.Core.Domain.Customers;
using App.Core.Domain.Localization;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.ExportImport;
using App.Services.ExportImport.Help;
using App.Services.Localization;
using NUnit.Framework;

namespace App.Tests.App.Services.Tests.ExportImport
{
    [TestFixture]
    public class ExportManagerTests : ServiceTest
    {
        #region Fields

        private CatalogSettings _catalogSettings;
        private IAddressService _addressService;
        private ICountryService _countryService;
        private ICustomerService _customerService;
        private IExportManager _exportManager;
        private ILanguageService _languageService;
        private IMeasureService _measureService;
  
        #endregion

        #region Setup

        [OneTimeSetUp]
        public async Task SetUp()
        {
            _catalogSettings = GetService<CatalogSettings>();
            _addressService = GetService<IAddressService>();
            _countryService = GetService<ICountryService>();
            _customerService = GetService<ICustomerService>();
            _exportManager = GetService<IExportManager>();
            _languageService = GetService<ILanguageService>();
            _measureService = GetService<IMeasureService>();
       
            await GetService<IGenericAttributeService>()
                .SaveAttributeAsync(await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail), "category-advanced-mode",
                    true);
            await GetService<IGenericAttributeService>()
                .SaveAttributeAsync(await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail), "manufacturer-advanced-mode",
                    true);
            await GetService<IGenericAttributeService>()
                .SaveAttributeAsync(await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail), "product-advanced-mode",
                    true);
        }

        [OneTimeTearDown]
        public async Task TearDown()
        {
            await GetService<IGenericAttributeService>()
                .SaveAttributeAsync(await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail), "category-advanced-mode",
                    false);
            await GetService<IGenericAttributeService>()
                .SaveAttributeAsync(await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail), "manufacturer-advanced-mode",
                    false);
            await GetService<IGenericAttributeService>()
                .SaveAttributeAsync(await _customerService.GetCustomerByEmailAsync(NopTestsDefaults.AdminEmail), "product-advanced-mode",
                    false);
        }

        #endregion

        #region Utilities

        protected static T PropertiesShouldEqual<T, L, Tp>(T actual, PropertyManager<Tp, L> manager, IDictionary<string, string> replacePairs, params string[] filter) where L : Language
        {
            var objectProperties = typeof(T).GetProperties();
            foreach (var property in manager.GetDefaultProperties)
            {
                if (filter.Contains(property.PropertyName))
                    continue;

                var objectProperty = replacePairs.ContainsKey(property.PropertyName)
                    ? objectProperties.FirstOrDefault(p => p.Name == replacePairs[property.PropertyName])
                    : objectProperties.FirstOrDefault(p => p.Name == property.PropertyName);

                if (objectProperty == null)
                    continue;

                var objectPropertyValue = objectProperty.GetValue(actual);

                if (objectProperty.PropertyType == typeof(Guid))
                    objectPropertyValue = objectPropertyValue.ToString();

                if (objectProperty.PropertyType == typeof(string))
                    objectPropertyValue = (property.PropertyValue?.ToString() == string.Empty && objectPropertyValue == null) ? string.Empty : objectPropertyValue;

                if (objectProperty.PropertyType.IsEnum)
                    objectPropertyValue = (int)objectPropertyValue;

                //https://github.com/ClosedXML/ClosedXML/blob/develop/ClosedXML/Extensions/ObjectExtensions.cs#L61
                if (objectProperty.PropertyType == typeof(DateTime))
                    objectPropertyValue = DateTime.FromOADate(double.Parse(((DateTime)objectPropertyValue).ToOADate().ToString("G15", CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));

                if (objectProperty.PropertyType == typeof(DateTime?))
                    objectPropertyValue = objectPropertyValue != null ? DateTime.FromOADate(double.Parse(((DateTime?)objectPropertyValue)?.ToOADate().ToString("G15", CultureInfo.InvariantCulture))) : null;

                //https://github.com/ClosedXML/ClosedXML/issues/544
                property.PropertyValue.Should().Be(objectPropertyValue ?? "", $"The property \"{typeof(T).Name}.{property.PropertyName}\" of these objects is not equal");
            }

            return actual;
        }

        protected async Task<PropertyManager<T, Language>> GetPropertyManagerAsync<T>(XLWorkbook workbook)
        {
            var languages = await _languageService.GetAllLanguagesAsync();

            //the columns
            var metadata = ImportManager.GetWorkbookMetadata<T>(workbook, languages);
            var defaultProperties = metadata.DefaultProperties;
            var localizedProperties = metadata.LocalizedProperties;

            return new PropertyManager<T, Language>(defaultProperties, _catalogSettings, localizedProperties);
        }

        protected XLWorkbook GetWorkbook(byte[] excelData)
        {
            var stream = new MemoryStream(excelData);
            return new XLWorkbook(stream);
        }

        protected T AreAllObjectPropertiesPresent<T, L>(T obj, PropertyManager<T, L> manager, params string[] filters) where L : Language
        {
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                if (filters.Contains(propertyInfo.Name))
                    continue;

                if (manager.GetDefaultProperties.Any(p => p.PropertyName == propertyInfo.Name))
                    continue;

                Assert.Fail("The property \"{0}.{1}\" no present on excel file", typeof(T).Name, propertyInfo.Name);
            }

            return obj;
        }

        #endregion

        #region Test export to excel
        [Test]
        public async Task CanExportCustomersToXlsx()
        {
            var replacePairs = new Dictionary<string, string>
            {
                { "VatNumberStatus", "VatNumberStatusId" }
            };

            var customers = await _customerService.GetAllCustomersAsync();

            var excelData = await _exportManager.ExportCustomersToXlsxAsync(customers);
            var workbook = GetWorkbook(excelData);
            var manager = await GetPropertyManagerAsync<Customer>(workbook);

            // get the first worksheet in the workbook
            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            manager.ReadDefaultFromXlsx(worksheet, 2);

            var customer = customers.First();

            var ignore = new List<string> { "Id", "ExternalAuthenticationRecords", "CustomerRoles", "ShoppingCartItems",
                "ReturnRequests", "BillingAddress", "ShippingAddress", "Addresses", "AdminComment",
                "EmailToRevalidate", "HasShoppingCartItems", "RequireReLogin", "FailedLoginAttempts",
                "CannotLoginUntilDateUtc", "Deleted", "IsSystemAccount", "SystemName", "LastIpAddress",
                "LastLoginDateUtc", "LastActivityDateUtc", "RegisteredInStoreId", "BillingAddressId", "ShippingAddressId",
                "CustomerCustomerRoleMappings", "CustomerAddressMappings", "EntityCacheKey", "VendorId",
                "DateOfBirth", "StreetAddress", "StreetAddress2", "ZipPostalCode", "City", "County", "CountryId",
                "StateProvinceId", "Phone", "Fax", "VatNumberStatusId", "TimeZoneId", "CustomCustomerAttributesXML",
                "CurrencyId", "LanguageId", "TaxDisplayTypeId", "TaxDisplayType", "TaxDisplayType", "VatNumberStatusId" };

            AreAllObjectPropertiesPresent(customer, manager, ignore.ToArray());
            PropertiesShouldEqual(customer, manager, replacePairs);
        }

        #endregion
    }
}
