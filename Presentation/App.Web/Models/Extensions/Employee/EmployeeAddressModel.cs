using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Models.Employee
{
    public partial record EmployeeAddressModel : BaseNopEntityModel
    {
        public EmployeeAddressModel()
        {
            Addresses = new List<EmployeeAddressModel>();
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Account.Employee.Address.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.LastName")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Employee.Address.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.Company")]
        public string Company { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.Country")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.Address1")]
        public string Address1 { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.County")]
        public string County { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.Address2")]
        public string Address2 { get; set; }

        [NopResourceDisplayName("Account.Employee.Address.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Employee.Address.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Employee.Address.Fields.FaxNumber")]
        public string FaxNumber { get; set; }

        //address in HTML format (usually used in grids)
        [NopResourceDisplayName("Account.Employee.Address")]
        public string AddressHtml { get; set; }
        public int EmployeeId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
     
        public IList<EmployeeAddressModel> Addresses { get; set; }

    }
}