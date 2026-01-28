using App.Core;
using App.Core.Domain.Employees;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Localization;
using App.Web.Areas.Admin.Factories;
using App.Web.Models.Employee;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the Address model factory implementation
    /// </summary>
    public partial class EmployeeAddressModelFactory : IEmployeeAddressModelFactory
    {
        #region Fields

        private readonly IEmpAddressService _empAddressService;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;
        private readonly IEmployeeService _employeeService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        #endregion

        #region Ctor

        public EmployeeAddressModelFactory(IEmpAddressService empAddressService,
            ILocalizationService localizationService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService,
            IEmployeeService employeeService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext)
        {
            _empAddressService = empAddressService;
            _localizationService = localizationService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
            _employeeService = employeeService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
        }

        #endregion

        #region Methods 
        public virtual async Task<EmployeeAddressModel> PrepareAddressAsync(EmployeeAddressModel model, EmpAddress address)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var addresses = await _empAddressService.GetAllAddressAsync(employee.Id, "");

            if (address != null) // Work with the address passed from the loop
            {
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;

                if (address.CountryId != null)
                {
                    model.CountryId = (int)address.CountryId;
                    if (model.CountryId > 0)
                    {
                        model.CountryName = (await _countryService.GetCountryByIdAsync(model.CountryId)).Name;
                    }
                }

                if (address.StateProvinceId != null)
                {
                    int stateId = (int)address.StateProvinceId;
                    if (stateId > 0)
                    {
                        model.StateProvinceName = (await _stateProvinceService.GetStateProvinceByIdAsync(stateId)).Name;
                    }
                }

                model.Address1 = address.Address1;
                model.City = address.City;
                model.County = address.County;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);
            return model;
        }
        #endregion
    }
}
