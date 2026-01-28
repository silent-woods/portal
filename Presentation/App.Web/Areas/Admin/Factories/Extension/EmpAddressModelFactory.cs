using App.Core.Domain.Customers;
using App.Core.Domain.Directory;
using App.Core.Domain.Employees;
using App.Data.Extensions;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Localization;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Address model factory implementation
    /// </summary>
    public partial class EmpAddressModelFactory : IEmpAddressModelFactory
    {
        #region Fields

        private readonly IEmpAddressService _empAddressService;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IEmployeeService _employeeService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        #endregion

        #region Ctor

        public EmpAddressModelFactory(IEmpAddressService empAddressService,
            ILocalizationService localizationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            IEmployeeService employeeService
            )
        {
            _empAddressService = empAddressService;
            _localizationService = localizationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _employeeService = employeeService;
        }

        #endregion
       
        #region Methods

        public virtual async Task<EmpAddressSearchModel> PrepareAddressSearchModelAsync(EmpAddressSearchModel searchModel,EmpAddress empAddress)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }
        public virtual async Task<EmpAddressListModel> PrepareAddressListModelAsync(EmpAddressSearchModel searchModel,Employee employee)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get address
            var address = await _empAddressService.GetAllAddressAsync(employeeId:searchModel.employeeId,addressName: searchModel.Address, showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new EmpAddressListModel().PrepareToGridAsync(searchModel, address, () =>
            {
                return address.SelectAwait(async Address =>
                {
                    //fill in model values from the entity
                    var addressModel = Address.ToModel<EmpAddressModel>();
                    addressModel.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
                    foreach (var c in await _countryService.GetAllCountriesAsync(addressModel.CountryId))
                    {
                        addressModel.AvailableCountries.Add(new SelectListItem
                        {
                            Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                            Value = c.Id.ToString(),
                            Selected = c.Id == addressModel.CountryId
                        });
                    }
                    return addressModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<EmpAddressModel> PrepareAddressModelAsync(EmpAddressModel model, EmpAddress address, bool excludeProperties = false)
        {
            if (address != null)
            {
                if (model == null)
                {
                    //fill in model values from the entity
                    model = address.ToModel<EmpAddressModel>();
                }

            }
            //prepare available countries
            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);
            // prepare available states  when post method is false then it call
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId);

            return model;
        }
        #endregion
    }
}
