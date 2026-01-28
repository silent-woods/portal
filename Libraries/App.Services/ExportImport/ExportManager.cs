using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using App.Core;
using App.Core.Domain.Catalog;
using App.Core.Domain.Customers;
using App.Core.Domain.Localization;
using App.Core.Domain.Seo;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.ExportImport.Help;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Seo;
using App.Services.Stores;

namespace App.Services.ExportImport
{
    /// <summary>
    /// Export manager
    /// </summary>
    public partial class ExportManager : IExportManager
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly CustomerSettings _customerSettings;
        private readonly DateTimeSettings _dateTimeSettings;
        private readonly ICountryService _countryService;
        private readonly ICustomerAttributeFormatter _customerAttributeFormatter;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IPictureService _pictureService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly IUrlRecordService _urlRecordService;
   
        #endregion

        #region Ctor

        public ExportManager(CatalogSettings catalogSettings,
            ICustomerActivityService customerActivityService,
            CustomerSettings customerSettings,
            DateTimeSettings dateTimeSettings,
            ICountryService countryService,
            ICustomerAttributeFormatter customerAttributeFormatter,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILocalizedEntityService localizedEntityService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPictureService pictureService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            IUrlRecordService urlRecordService)
        {
            _catalogSettings = catalogSettings;
            _customerActivityService = customerActivityService;
            _customerSettings = customerSettings;
            _dateTimeSettings = dateTimeSettings;
            _countryService = countryService;
            _customerAttributeFormatter = customerAttributeFormatter;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _localizedEntityService = localizedEntityService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _pictureService = pictureService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _urlRecordService = urlRecordService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Returns the path to the image file by ID
        /// </summary>
        /// <param name="pictureId">Picture ID</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the path to the image file
        /// </returns>
        protected virtual async Task<string> GetPicturesAsync(int pictureId)
        {
            var picture = await _pictureService.GetPictureByIdAsync(pictureId);

            return await _pictureService.GetThumbLocalPathAsync(picture);
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<TProperty> GetLocalizedAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> keySelector,
            Language language) where TEntity : BaseEntity, ILocalizedEntity
        {
            if (entity == null)
                return default(TProperty);

            return await _localizationService.GetLocalizedAsync(entity, keySelector, language.Id, false);
        }
          
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task<object> GetCustomCustomerAttributesAsync(Customer customer)
        {
            return await _customerAttributeFormatter.FormatAttributesAsync(customer.CustomCustomerAttributesXML, ";");
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task WriteLocalizedPropertyXmlAsync<TEntity, TPropType>(TEntity entity, Expression<Func<TEntity, TPropType>> keySelector,
            XmlWriter xmlWriter, IList<Language> languages, bool ignore = false, string overriddenNodeName = null)
            where TEntity : BaseEntity, ILocalizedEntity
        {
            if (ignore)
                return;

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (keySelector.Body is not MemberExpression member)
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");

            if (member.Member is not PropertyInfo propInfo)
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");

            var localeKeyGroup = entity.GetType().Name;
            var localeKey = propInfo.Name;

            var nodeName = localeKey;
            if (!string.IsNullOrWhiteSpace(overriddenNodeName))
                nodeName = overriddenNodeName;

            await xmlWriter.WriteStartElementAsync(nodeName);
            await xmlWriter.WriteStringAsync("Standard", propInfo.GetValue(entity));

            if (languages.Count >= 2)
            {
                await xmlWriter.WriteStartElementAsync("Locales");

                var properties = await _localizedEntityService.GetEntityLocalizedPropertiesAsync(entity.Id, localeKeyGroup, localeKey);
                foreach (var language in languages)
                    if (properties.FirstOrDefault(lp => lp.LanguageId == language.Id) is LocalizedProperty localizedProperty)
                        await xmlWriter.WriteStringAsync(language.UniqueSeoCode, localizedProperty.LocaleValue);

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task WriteLocalizedSeNameXmlAsync<TEntity>(TEntity entity, XmlWriter xmlWriter, IList<Language> languages,
            bool ignore = false, string overriddenNodeName = null)
            where TEntity : BaseEntity, ISlugSupported
        {
            if (ignore)
                return;

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var nodeName = "SEName";
            if (!string.IsNullOrWhiteSpace(overriddenNodeName))
                nodeName = overriddenNodeName;

            await xmlWriter.WriteStartElementAsync(nodeName);
            await xmlWriter.WriteStringAsync("Standard", await _urlRecordService.GetSeNameAsync(entity, 0));

            if (languages.Count >= 2)
            {
                await xmlWriter.WriteStartElementAsync("Locales");

                foreach (var language in languages)
                    if (await _urlRecordService.GetSeNameAsync(entity, language.Id, returnDefaultValue: false) is string seName && !string.IsNullOrWhiteSpace(seName))
                        await xmlWriter.WriteStringAsync(language.UniqueSeoCode, seName);

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Export customer list to XLSX
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task<byte[]> ExportCustomersToXlsxAsync(IList<Customer> customers)
        {
            
            async Task<object> getCountry(Customer customer)
            {
                var countryId = customer.CountryId;

                if (!_catalogSettings.ExportImportRelatedEntitiesByName)
                    return countryId;

                var country = await _countryService.GetCountryByIdAsync(countryId);

                return country?.Name ?? string.Empty;
            }

            async Task<object> getStateProvince(Customer customer)
            {
                var stateProvinceId = customer.StateProvinceId;

                if (!_catalogSettings.ExportImportRelatedEntitiesByName)
                    return stateProvinceId;

                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(stateProvinceId);

                return stateProvince?.Name ?? string.Empty;
            }

            //property manager 
            var manager = new PropertyManager<Customer, Language>(new[]
            {
                new PropertyByName<Customer, Language>("CustomerId", (p, l) => p.Id),
                new PropertyByName<Customer, Language>("CustomerGuid", (p, l) => p.CustomerGuid),
                new PropertyByName<Customer, Language>("Email", (p, l) => p.Email),
                new PropertyByName<Customer, Language>("Username", (p, l) => p.Username),
                new PropertyByName<Customer, Language>("IsTaxExempt", (p, l) => p.IsTaxExempt),
                new PropertyByName<Customer, Language>("AffiliateId", (p, l) => p.AffiliateId),
                new PropertyByName<Customer, Language>("Active", (p, l) => p.Active),
                new PropertyByName<Customer, Language>("CustomerRoles",  async (p, l) =>  string.Join(", ",
                    (await _customerService.GetCustomerRolesAsync(p)).Select(role => _catalogSettings.ExportImportRelatedEntitiesByName ? role.Name : role.Id.ToString()))),
                new PropertyByName<Customer, Language>("IsGuest", async (p, l) => await _customerService.IsGuestAsync(p)),
                new PropertyByName<Customer, Language>("IsRegistered", async (p, l) => await _customerService.IsRegisteredAsync(p)),
                new PropertyByName<Customer, Language>("IsAdministrator", async (p, l) => await _customerService.IsAdminAsync(p)),
                new PropertyByName<Customer, Language>("IsForumModerator", async (p, l) => await _customerService.IsForumModeratorAsync(p)),
                new PropertyByName<Customer, Language>("IsVendor", async (p, l) => await _customerService.IsVendorAsync(p)),
                new PropertyByName<Customer, Language>("CreatedOnUtc", (p, l) => p.CreatedOnUtc),
                //attributes
                new PropertyByName<Customer, Language>("FirstName", (p, l) => p.FirstName, !_customerSettings.FirstNameEnabled),
                new PropertyByName<Customer, Language>("LastName", (p, l) => p.LastName, !_customerSettings.LastNameEnabled),
                new PropertyByName<Customer, Language>("Gender", (p, l) => p.Gender, !_customerSettings.GenderEnabled),
                new PropertyByName<Customer, Language>("Company", (p, l) => p.Company, !_customerSettings.CompanyEnabled),
                new PropertyByName<Customer, Language>("StreetAddress", (p, l) => p.StreetAddress, !_customerSettings.StreetAddressEnabled),
                new PropertyByName<Customer, Language>("StreetAddress2", (p, l) => p.StreetAddress2, !_customerSettings.StreetAddress2Enabled),
                new PropertyByName<Customer, Language>("ZipPostalCode", (p, l) => p.ZipPostalCode, !_customerSettings.ZipPostalCodeEnabled),
                new PropertyByName<Customer, Language>("City", (p, l) => p.City, !_customerSettings.CityEnabled),
                new PropertyByName<Customer, Language>("County", (p, l) => p.County, !_customerSettings.CountyEnabled),
                new PropertyByName<Customer, Language>("Country",  async (p, l) => await getCountry(p), !_customerSettings.CountryEnabled),
                new PropertyByName<Customer, Language>("StateProvince",  async (p, l) => await getStateProvince(p), !_customerSettings.StateProvinceEnabled),
                new PropertyByName<Customer, Language>("Phone", (p, l) => p.Phone, !_customerSettings.PhoneEnabled),
                new PropertyByName<Customer, Language>("Fax", (p, l) => p.Fax, !_customerSettings.FaxEnabled),
                new PropertyByName<Customer, Language>("VatNumber", (p, l) => p.VatNumber),
                new PropertyByName<Customer, Language>("VatNumberStatusId", (p, l) => p.VatNumberStatusId),
                new PropertyByName<Customer, Language>("TimeZone", (p, l) => p.TimeZoneId, !_dateTimeSettings.AllowCustomersToSetTimeZone),
                new PropertyByName<Customer, Language>("AvatarPictureId", async (p, l) => await _genericAttributeService.GetAttributeAsync<int>(p, NopCustomerDefaults.AvatarPictureIdAttribute), !_customerSettings.AllowCustomersToUploadAvatars),
                new PropertyByName<Customer, Language>("ForumPostCount", async (p, l) => await _genericAttributeService.GetAttributeAsync<int>(p, NopCustomerDefaults.ForumPostCountAttribute)),
                new PropertyByName<Customer, Language>("Signature", async (p, l) => await _genericAttributeService.GetAttributeAsync<string>(p, NopCustomerDefaults.SignatureAttribute)),
                new PropertyByName<Customer, Language>("CustomCustomerAttributes", async (p, l) => await GetCustomCustomerAttributesAsync(p))
            }, _catalogSettings);

            //activity log
            await _customerActivityService.InsertActivityAsync("ExportCustomers",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.ExportCustomers"), customers.Count));

            return await manager.ExportToXlsxAsync(customers);
        }

        /// <summary>
        /// Export customer list to XML
        /// </summary>
        /// <param name="customers">Customers</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result in XML format
        /// </returns>
        public virtual async Task<string> ExportCustomersToXmlAsync(IList<Customer> customers)
        {
            var settings = new XmlWriterSettings
            {
                Async = true,
                ConformanceLevel = ConformanceLevel.Auto
            };

            await using var stringWriter = new StringWriter();
            await using var xmlWriter = XmlWriter.Create(stringWriter, settings);

            await xmlWriter.WriteStartDocumentAsync();
            await xmlWriter.WriteStartElementAsync("Customers");
            await xmlWriter.WriteAttributeStringAsync("Version", NopVersion.CURRENT_VERSION);

            foreach (var customer in customers)
            {
                await xmlWriter.WriteStartElementAsync("Customer");
                await xmlWriter.WriteElementStringAsync("CustomerId", null, customer.Id.ToString());
                await xmlWriter.WriteElementStringAsync("CustomerGuid", null, customer.CustomerGuid.ToString());
                await xmlWriter.WriteElementStringAsync("Email", null, customer.Email);
                await xmlWriter.WriteElementStringAsync("Username", null, customer.Username);

                await xmlWriter.WriteElementStringAsync("IsTaxExempt", null, customer.IsTaxExempt.ToString());
                await xmlWriter.WriteElementStringAsync("AffiliateId", null, customer.AffiliateId.ToString());
                await xmlWriter.WriteElementStringAsync("Active", null, customer.Active.ToString());

                await xmlWriter.WriteElementStringAsync("IsGuest", null, (await _customerService.IsGuestAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("IsRegistered", null, (await _customerService.IsRegisteredAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("IsAdministrator", null, (await _customerService.IsAdminAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("IsForumModerator", null, (await _customerService.IsForumModeratorAsync(customer)).ToString());
                await xmlWriter.WriteElementStringAsync("CreatedOnUtc", null, customer.CreatedOnUtc.ToString(CultureInfo.InvariantCulture));

                await xmlWriter.WriteElementStringAsync("FirstName", null, customer.FirstName);
                await xmlWriter.WriteElementStringAsync("LastName", null, customer.LastName);
                await xmlWriter.WriteElementStringAsync("Gender", null, customer.Gender);
                await xmlWriter.WriteElementStringAsync("Company", null, customer.Company);

                await xmlWriter.WriteElementStringAsync("CountryId", null, customer.CountryId.ToString());
                await xmlWriter.WriteElementStringAsync("StreetAddress", null, customer.StreetAddress);
                await xmlWriter.WriteElementStringAsync("StreetAddress2", null, customer.StreetAddress2);
                await xmlWriter.WriteElementStringAsync("ZipPostalCode", null, customer.ZipPostalCode);
                await xmlWriter.WriteElementStringAsync("City", null, customer.City);
                await xmlWriter.WriteElementStringAsync("County", null, customer.County);
                await xmlWriter.WriteElementStringAsync("StateProvinceId", null, customer.StateProvinceId.ToString());
                await xmlWriter.WriteElementStringAsync("Phone", null, customer.Phone);
                await xmlWriter.WriteElementStringAsync("Fax", null, customer.Fax);
                await xmlWriter.WriteElementStringAsync("VatNumber", null, customer.VatNumber);
                await xmlWriter.WriteElementStringAsync("VatNumberStatusId", null, customer.VatNumberStatusId.ToString());
                await xmlWriter.WriteElementStringAsync("TimeZoneId", null, customer.TimeZoneId);

                foreach (var store in await _storeService.GetAllStoresAsync())
                {
                    var newsletter = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                    var subscribedToNewsletters = newsletter != null && newsletter.Active;
                    await xmlWriter.WriteElementStringAsync($"Newsletter-in-store-{store.Id}", null, subscribedToNewsletters.ToString());
                }

                await xmlWriter.WriteElementStringAsync("AvatarPictureId", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.AvatarPictureIdAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("ForumPostCount", null, (await _genericAttributeService.GetAttributeAsync<int>(customer, NopCustomerDefaults.ForumPostCountAttribute)).ToString());
                await xmlWriter.WriteElementStringAsync("Signature", null, await _genericAttributeService.GetAttributeAsync<string>(customer, NopCustomerDefaults.SignatureAttribute));

                if (!string.IsNullOrEmpty(customer.CustomCustomerAttributesXML))
                {
                    var selectedCustomerAttributes = new StringReader(customer.CustomCustomerAttributesXML);
                    var selectedCustomerAttributesXmlReader = XmlReader.Create(selectedCustomerAttributes);
                    await xmlWriter.WriteNodeAsync(selectedCustomerAttributesXmlReader, false);
                }

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
            await xmlWriter.WriteEndDocumentAsync();
            await xmlWriter.FlushAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("ExportCustomers",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.ExportCustomers"), customers.Count));

            return stringWriter.ToString();
        }

        #endregion
    }
}
