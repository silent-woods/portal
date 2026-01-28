using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Web.Framework.Mvc.ModelBinding;
using App.Web.Framework.Models;
using System.Globalization;
using App.Core;

namespace App.Web.Models.Employee
{
    public partial record EmployeeInfoModel : BaseNopModel
    {
        public EmployeeInfoModel()
        {
            
        }

        [NopResourceDisplayName("Account.Employee.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Account.Employee.Fields.LastName")]
        public string LastName { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.PersonalEmail")]
        public string PersonalEmail { get; set; }            
        [NopResourceDisplayName("Account.Employee.Fields.Gender")]
        public string Gender { get; set; }            
        [NopResourceDisplayName("Account.Employee.Fields.MobileNo")]
        public string MobileNo { get; set; }            
        [NopResourceDisplayName("Account.Employee.Fields.PictureId")]
        public string PictureId { get; set; }

        // Work information

        [NopResourceDisplayName("Account.Employee.Fields.OfficialEmail")]
        public string OfficialEmail { get; set; }            
        [NopResourceDisplayName("Account.Employee.Fields.Location")]
        public string Location { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.Department")]
        public string Department { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.Designation")]
        public string Designation { get; set; }   
        [NopResourceDisplayName("Account.Employee.Fields.DateofJoining")]
        public string DateofJoining { get; set; }            
        [NopResourceDisplayName("Account.Employee.Fields.DateOfBirth")]

        public string DateOfBirth { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.CTC")]

        
        public string CTC { get; set; }

        // Personal Information
        [NopResourceDisplayName("Account.Employee.Fields.MaritalStatus")]
        public string MaritalStatus { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.FatherName")]
        public string FatherName { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.MotherName")]
        public string MotherName { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.Hobbies")]
        public string Hobbies { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.BloodGroup")]
        public string BloodGroup { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.EmergencyContactPerson")]
        public string EmergencyContactPerson { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.Relationship")]
        public string Relationship { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.ContactNumber")]
        public string ContactNumber { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.EmployeeStatus")]
        public string EmployeeStatus { get; set; }
        //[NopResourceDisplayName("Account.Employee.Fields.CTC")]
        //public string CTC { get; set; }
        [NopResourceDisplayName("Account.Employee.Fields.IsSeniorStatus")]
        public bool IsSeniorStatus { get; set; }

    }
}