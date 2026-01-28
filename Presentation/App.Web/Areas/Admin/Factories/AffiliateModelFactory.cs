using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Affiliates;
using App.Data.Extensions;
using App.Services.Affiliates;
using App.Services.Common;
using App.Services.Customers;
using App.Services.Directory;
using App.Services.Helpers;
using App.Services.Localization;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Affiliates;
using App.Web.Areas.Admin.Models.Common;
using App.Web.Framework.Models.Extensions;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the affiliate model factory implementation
    /// </summary>
    public partial class AffiliateModelFactory : IAffiliateModelFactory
    {
        #region Fields

        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IAffiliateService _affiliateService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IStateProvinceService _stateProvinceService;

        #endregion

        #region Ctor

        public AffiliateModelFactory(IAddressModelFactory addressModelFactory,
            IAddressService addressService,
            IAffiliateService affiliateService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            ILocalizationService localizationService,
            IStateProvinceService stateProvinceService)
        {
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _affiliateService = affiliateService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _stateProvinceService = stateProvinceService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Prepare affiliated customer search model
        /// </summary>
        /// <param name="searchModel">Affiliated customer search model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>Affiliated customer search model</returns>
        protected virtual AffiliatedCustomerSearchModel PrepareAffiliatedCustomerSearchModel(AffiliatedCustomerSearchModel searchModel, Affiliate affiliate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            searchModel.AffliateId = affiliate.Id;

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare affiliate search model
        /// </summary>
        /// <param name="searchModel">Affiliate search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliate search model
        /// </returns>
        public virtual Task<AffiliateSearchModel> PrepareAffiliateSearchModelAsync(AffiliateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged affiliate list model
        /// </summary>
        /// <param name="searchModel">Affiliate search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliate list model
        /// </returns>
        public virtual async Task<AffiliateListModel> PrepareAffiliateListModelAsync(AffiliateSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get affiliates
            var affiliates = await _affiliateService.GetAllAffiliatesAsync(searchModel.SearchFriendlyUrlName,
                searchModel.SearchFirstName,
                searchModel.SearchLastName,
                searchModel.Page - 1, searchModel.PageSize, true);

            //prepare list model
            var model = await new AffiliateListModel().PrepareToGridAsync(searchModel, affiliates, () =>
            {
                //fill in model values from the entity
                return affiliates.SelectAwait(async affiliate =>
                {
                    var address = await _addressService.GetAddressByIdAsync(affiliate.AddressId);

                    var affiliateModel = affiliate.ToModel<AffiliateModel>();
                    affiliateModel.Address = address.ToModel<AddressModel>();
                    affiliateModel.Address.CountryName = (await _countryService.GetCountryByAddressAsync(address))?.Name;
                    affiliateModel.Address.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name;

                    return affiliateModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare affiliate model
        /// </summary>
        /// <param name="model">Affiliate model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliate model
        /// </returns>
        public virtual async Task<AffiliateModel> PrepareAffiliateModelAsync(AffiliateModel model, Affiliate affiliate, bool excludeProperties = false)
        {
            //fill in model values from the entity
            if (affiliate != null)
            {
                model ??= affiliate.ToModel<AffiliateModel>();
                model.Url = await _affiliateService.GenerateUrlAsync(affiliate);

                PrepareAffiliatedCustomerSearchModel(model.AffiliatedCustomerSearchModel, affiliate);

                //whether to fill in some of properties
                if (!excludeProperties)
                {
                    model.AdminComment = affiliate.AdminComment;
                    model.FriendlyUrlName = affiliate.FriendlyUrlName;
                    model.Active = affiliate.Active;
                }
            }

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(affiliate?.AddressId ?? 0);
            if (!excludeProperties && address != null)
                model.Address = address.ToModel(model.Address);
            await _addressModelFactory.PrepareAddressModelAsync(model.Address, address);
            model.Address.FirstNameRequired = true;
            model.Address.LastNameRequired = true;
            model.Address.EmailRequired = true;
            model.Address.CountryRequired = true;
            model.Address.CountyRequired = true;
            model.Address.CityRequired = true;
            model.Address.StreetAddressRequired = true;
            model.Address.ZipPostalCodeRequired = true;
            model.Address.PhoneRequired = true;

            return model;
        }

        /// <summary>
        /// Prepare paged affiliated customer list model
        /// </summary>
        /// <param name="searchModel">Affiliated customer search model</param>
        /// <param name="affiliate">Affiliate</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the affiliated customer list model
        /// </returns>
        public virtual async Task<AffiliatedCustomerListModel> PrepareAffiliatedCustomerListModelAsync(AffiliatedCustomerSearchModel searchModel,
            Affiliate affiliate)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (affiliate == null)
                throw new ArgumentNullException(nameof(affiliate));

            //get customers
            var customers = await _customerService.GetAllCustomersAsync(affiliateId: affiliate.Id,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new AffiliatedCustomerListModel().PrepareToGrid(searchModel, customers, () =>
            {
                //fill in model values from the entity
                return customers.Select(customer =>
                {
                    var affiliatedCustomerModel = customer.ToModel<AffiliatedCustomerModel>();
                    affiliatedCustomerModel.Name = customer.Email;

                    return affiliatedCustomerModel;
                });
            });

            return model;
        }

        #endregion
    }
}