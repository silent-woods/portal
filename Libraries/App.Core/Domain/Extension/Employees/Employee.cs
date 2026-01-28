using System;

namespace App.Core.Domain.Employees
{
    /// <summary>
    /// Represents a Employee
    /// </summary>
    public partial class Employee : BaseEntity
    {
       
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PersonalEmail { get; set; }
        public string Gender { get; set; }
        public string MobileNo { get; set; }

        public DateTime CreateOnUtc { get; set; }
        public DateTime UpdateOnUtc { get; set; }
        public int PictureId { get; set; }

        public int Customer_Id { get; set; }

        // Work information

        public string OfficialEmail { get; set; }
        public string Location { get; set; }
        public int DepartmentId { get; set; }
        public int DesignationId { get; set; }
        public DateTime DateofJoining { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string CTC { get; set; }

        // Personal Information
        public int MaritalStatusId { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Hobbies { get; set; }
        public int BloodGroupId { get; set; }
        public string EmergencyContactPerson { get; set; }
        public string Relationship { get; set; }
        public string ContactNumber { get; set; }
        public int EmployeeStatusId { get; set; }
        public int PresentAddressId { get; set; }
        public int PermanentAddressID { get; set; }
        public bool IsSeniorStatus { get; set; }

        public int ManagerId { get; set; }

        public int ActivityTrackingStatusId { get; set; }

    }
}