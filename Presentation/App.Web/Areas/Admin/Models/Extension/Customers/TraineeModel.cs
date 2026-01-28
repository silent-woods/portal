//using Nop.Admin.Validators.Trainee;
using App.Web.Areas.Admin.InterviewQeations.Models;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Web.Areas.Admin.Models.Customers
{
    public partial record TraineeModel : BaseNopEntityModel
    {
        public TraineeModel()
        {
            Addresses = new List<RecruitementModel>();
        }
        public IList<RecruitementModel> Addresses { get; set; }
        [NopResourceDisplayName("Trainee.Fields.Gender")]
        public string Gender { get; set; }

        [NopResourceDisplayName("Trainee.Fields.FirstName")]
        public string FirstName { get; set; }

        [NopResourceDisplayName("Trainee.Fields.LastName")]
        public string LastName { get; set; }

        [UIHint("DateNullable")]
        [NopResourceDisplayName("Trainee.Fields.DateOfBirth")]
        public DateTime? DateOfBirth { get; set; }

        [NopResourceDisplayName("Trainee.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Trainee.Fields.CollegeName")]
        public string CollegeName { get; set; }

        [NopResourceDisplayName("Trainee.Fields.Degree")]
        public string Degree { get; set; }

        [NopResourceDisplayName("Trainee.Fields.CompeletdYear")]
        public string CompletedYear { get; set; }

        [NopResourceDisplayName("Trainee.Fields.Percentage")]
        public decimal Percentage { get; set; }

        [NopResourceDisplayName("Trainee.Fields.ResumeUrl")]
        [UIHint("Download")]
        public int ResumeUrl { get; set; }

        [NopResourceDisplayName("Trainee.Fields.Skill")]
        public string Skill { get; set; }

        [NopResourceDisplayName("Trainee.Fields.MobileNumber")]
        public string MobileNumber { get; set; }

        [NopResourceDisplayName("Trainee.Fields.StreetAddress")]
        public string StreetAddress { get; set; }

        [NopResourceDisplayName("Trainee.Fields.StreetAddress2")]
        public string StreetAddress2 { get; set; }

        [NopResourceDisplayName("Trainee.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [NopResourceDisplayName("Trainee.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Trainee.Fields.AboutYourSelf")]
        public string AboutYourSelf { get; set; }

        [NopResourceDisplayName("Admin.Trainee.DateForInterview")]
        public DateTime DateForInterview { get; set; }

        [NopResourceDisplayName("Admin.Trainee.PersonInCharge")]
        public string PersonInCharge { get; set; }

        [NopResourceDisplayName("Admin.Trainee.Venue")]
        public string Venue { get; set; }

        [NopResourceDisplayName("Admin.Trainee.CreatedOnUTC")]
        public DateTime? CreatedOnUTC { get; set; }

        public int? DateOfBirthDay { get; set; }
        [NopResourceDisplayName("Trainee.Fields.DateOfBirth")]
        public int? DateOfBirthMonth { get; set; }
        [NopResourceDisplayName("Trainee.Fields.DateOfBirth")]
        public int? DateOfBirthYear { get; set; }
        public DateTime? ParseDateOfBirth()
        {
            if (!DateOfBirthYear.HasValue || !DateOfBirthMonth.HasValue || !DateOfBirthDay.HasValue)
                return null;

            DateTime? dateOfBirth = null;
            try
            {
                dateOfBirth = new DateTime(DateOfBirthYear.Value, DateOfBirthMonth.Value, DateOfBirthDay.Value);
            }
            catch { }
            return dateOfBirth;
        }
    }
}