using System;
using System.ComponentModel.DataAnnotations;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace App.Web.Areas.Admin.Models.Employees

{
    /// <summary>
    /// Represents a employee model
    /// </summary>
    public partial record EmployeeModel : BaseNopEntityModel
    {

        #region Ctor

        public EmployeeModel()
        {
            EmployeeSearchModel = new EmployeeSearchModel();
            EducationModel = new List<EducationModel>();
            EducationSearchModel = new EducationSearchModel();
            ExperienceModel = new List<EducationModel>();
            ExperienceSearchModel = new ExperienceSearchModel();
            AssetsModels = new List<AssetsModel>();
            AssetsSearchModel = new AssetsSearchModel();
            BloodGroups = new List<SelectListItem>();
            MaritalsStatus = new List<SelectListItem>();
            EmployeesStatus = new List<SelectListItem>();
            Departments = new List<SelectListItem>();
            Designation= new List<SelectListItem>();
            EmpAddressModels = new List<EmpAddressModel>();
            EmpAddressSearchModels = new EmpAddressSearchModel();
            SelectedManagerId = new List<int>();
            AvailableManager = new List<SelectListItem>();
        }

        #endregion

        #region Properties
        public List<AssetsModel> AssetsModels { get; set; }
        public AssetsSearchModel AssetsSearchModel { get; set; }
        public List<EducationModel> ExperienceModel { get; set; }
        public EducationSearchModel EducationSearchModel { get; set; }
        public List<EducationModel> EducationModel { get; set; }
        public ExperienceSearchModel ExperienceSearchModel { get; set; }
        public List<EmpAddressModel> EmpAddressModels { get; set; }
        public EmpAddressSearchModel EmpAddressSearchModels { get; set; }

        [NopResourceDisplayName("Admin.Employees.Fields.FirstName")]
        [Required(ErrorMessage = "Please enter a first name.")]
        public string FirstName { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.LastName")]
        [Required(ErrorMessage = "Please enter a last name.")]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter a Personal Email")]
        [NopResourceDisplayName("Admin.Employees.Fields.PersonalEmail")]
        public string PersonalEmail { get; set; }

        [NopResourceDisplayName("Admin.Customers.Customers.Fields.Password")]
        
        public string Password { get; set; }


        [NopResourceDisplayName("Admin.Employees.Fields.Gender")]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please enter a Mobile No")]
        [NopResourceDisplayName("Admin.Employees.Fields.MobileNo")]
        public string MobileNo { get; set; }

  
        [NopResourceDisplayName("Admin.Employees.Fields.PictureId")]
        [UIHint("Picture")]
        public int PictureId { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.PictureUrl")]
        public string PictureUrl { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.CreateOnUtc")]
        public DateTime CreateOnUtc { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.UpdateOnUtc")]
        public DateTime UpdateOnUtc { get; set; }

        // Work information
        [DataType(DataType.EmailAddress)]
        [Required(ErrorMessage = "Please enter a Official Email")]
        [NopResourceDisplayName("Admin.Employees.Fields.OfficialEmail")]
        public string OfficialEmail { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.Location")]
        public string Location { get; set; }

        [Required(ErrorMessage = "Please select Department.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Department.")]
        [NopResourceDisplayName("Admin.Employees.Fields.DepartmentId")]
        public int DepartmentId { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.DepartmentName")]
        public string DepartmentName { get; set; }
        public IList<SelectListItem> Departments { get; set; }

        [Required(ErrorMessage = "Please select Designation.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Designation.")]
        [NopResourceDisplayName("Admin.Employees.Fields.DesignationId")]
        public int DesignationId { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.DesignationName")]
        public string DesignationName { get; set; }
        public IList<SelectListItem> Designation { get; set; }

        [Required(ErrorMessage = "Please select Date Of Joining.")]
        [NopResourceDisplayName("Admin.Employees.Fields.DateofJoining")]
        [UIHint("DateNullable")]
        public DateTime? DateofJoining { get; set; }

        [Required(ErrorMessage = "Please select Date Of Birth.")]
        [NopResourceDisplayName("Admin.Employees.Fields.DateOfBirth")]
        [UIHint("DateNullable")]
        public DateTime? DateOfBirth { get; set; }

       

        [NopResourceDisplayName("Admin.Employees.Fields.CTC")]
        public string CTC { get; set; }

        // Personal Information
        //[Required(ErrorMessage = "Please select Marital Status.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select Marital Status.")]
        [NopResourceDisplayName("Admin.Employees.Fields.MaritalStatusId")]
        public int MaritalStatusId { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.MaritalStatus")]
        public string MaritalStatus { get; set; }
        public IList<SelectListItem> MaritalsStatus { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.FatherName")]
        public string FatherName { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.MotherName")]
        public string MotherName { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.Hobbies")]
        public string Hobbies { get; set; }

        //[Required(ErrorMessage = "Please select BloodGroup.")]
        //[Range(1, int.MaxValue, ErrorMessage = "Please select BloodGroup.")]
        [NopResourceDisplayName("Admin.Employees.Fields.BloodGroupId")]
        public int BloodGroupId { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.BloodGroup")]
        public string BloodGroup { get; set; }
        public IList<SelectListItem> BloodGroups { get; set; }

        [NopResourceDisplayName("Admin.Employees.Fields.EmergencyContactPerson")]
        public string EmergencyContactPerson { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.Relationship")]
        public string Relationship { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.ContactNumber")]
        public string ContactNumber { get; set; }

        [Required(ErrorMessage = "Please select Employee Status.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select Employee Status.")]
        [NopResourceDisplayName("Admin.Employees.Fields.EmployeeStatusId")]
        public int EmployeeStatusId { get; set; }
        [NopResourceDisplayName("Admin.Employees.Fields.EmployeeStatus")]
        public string EmployeeStatus { get; set; }
        public IList<SelectListItem> EmployeesStatus { get; set; }
        public int PresentAddressId { get; set; }
        public int PermanentAddressID { get; set; }

        public EmployeeSearchModel EmployeeSearchModel { get; set; }

        public int ManagerId { get; set; }

        public string ManagerName { get; set; }
        public IList<SelectListItem> AvailableManager { get; set; }


        [NopResourceDisplayName("Admin.Common.Fields.SelectedManagerId")]

        public IList<int> SelectedManagerId { get; set; }

        public int Customer_Id { get; set; }
        [NopResourceDisplayName("Admin.Common.Fields.OnBordingEmail")]

        public string OnBordingEmail { get; set; }


        #endregion
    }
}