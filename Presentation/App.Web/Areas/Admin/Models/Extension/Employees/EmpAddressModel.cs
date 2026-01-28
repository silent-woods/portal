using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;

namespace App.Web.Areas.Admin.Models.Employees
{
    public partial record EmpAddressModel : BaseNopEntityModel
    {
        public EmpAddressModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            EmpAddressSearchModel = new EmpAddressSearchModel();
        }
        [Required(ErrorMessage = "Please enter First Name")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.FirstName")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Please enter Last Name")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.LastName")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Please enter Email")]
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter Company Name.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.Company")]
        public string Company { get; set; }

        [Required(ErrorMessage = "Please select Country.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Country.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.Country")]
        public int CountryId { get; set; }

        [NopResourceDisplayName("Admin.Employee.Address.Fields.Country")]
        public string CountryName { get; set; }

        [Required(ErrorMessage = "Please select State.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select State.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }


        //[Required(ErrorMessage = "Please enter State Name.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [Required(ErrorMessage = "Please enter Address.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.Address1")]
        public string Address1 { get; set; }


        [Required(ErrorMessage = "Please enter City.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.City")]
        public string City { get; set; }


    
        [NopResourceDisplayName("Admin.Employee.Address.Fields.County")]
        public string County { get; set; }

        [NopResourceDisplayName("Admin.Employee.Address.Fields.Address2")]
        public string Address2 { get; set; }

        [Required(ErrorMessage = "Please enter Zip Postal Code.")]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [Required(ErrorMessage = "Please enter Phone Number.")]
        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Admin.Employee.Address.Fields.FaxNumber")]
        public string FaxNumber { get; set; }

        //address in HTML format (usually used in grids)
        [NopResourceDisplayName("Admin.Employee.Address")]
        public string AddressHtml { get; set; }
        public int EmployeeId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        
       public EmpAddressSearchModel EmpAddressSearchModel { get; set; }

    }
}