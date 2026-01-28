using App.Web.Areas.Admin.Models.Employees;
using App.Web.Framework.Models;
using App.Web.Framework.Mvc.ModelBinding;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;



namespace App.Web.Areas.Admin.Models.Extension.Announcements
{
    public partial record AnnouncementModel : BaseNopEntityModel
    {

    
       public AnnouncementModel()
        {
            AvailableEmployees = new List<SelectListItem>();
            AvailableDesignation = new List<SelectListItem>();
            AvailableAudienceTypes = new List<SelectListItem>();
            AvailableProjects =  new List<SelectListItem>();
            Projects = new List<SelectListItem>();
            SendEmployeeIdList = new List<int>();
            ReferenceIdList = new List<int>();
            LikedEmployees= new List<EmployeeModel>();
            RemainingEmployees=   new List<EmployeeModel>();

        }

        [NopResourceDisplayName("Admin.Announcement.Fields.Title")]
        [Required(ErrorMessage = "Please select Title")]

        public string Title { get; set; }


        [NopResourceDisplayName("Admin.Announcement.Fields.Message")]

        public string Message { get; set; }

        
        public DateTime CreatedOnUtc { get; set; }


        [NopResourceDisplayName("Admin.Announcement.Fields.ScheduledOnUtc")]


        public DateTime? ScheduledOnUtc { get; set; }

        [NopResourceDisplayName("Admin.Announcement.Fields.IsScheduledOnUtc")]

        public bool IsScheduledOnUtc { get; set; }




        public string LikedEmployeeIds { get; set; }

        [NopResourceDisplayName("Admin.Announcement.Fields.AttachmentFile")]

        public IFormFile AttachmentFile { get; set; }

        public string AttachmentUrl { get; set; }

        [NopResourceDisplayName("Admin.Announcement.Fields.ReferenceName")]

        public string ReferenceName { get; set; }

        [NopResourceDisplayName("Admin.Announcement.Fields.AudienceTypeId")]

        public int AudienceTypeId { get; set; }

        public string AudienceTypeName { get; set; }

        public string ReferenceIds { get; set; }

        public IList<int> ReferenceIdList { get; set; }

        [NopResourceDisplayName("Admin.Announcement.Fields.SendEmployeeIdList")]

        public IList<int> SendEmployeeIdList { get; set; }

        public string SendEmployeeIds { get; set; }

        public bool IsSent { get; set; }

        [NopResourceDisplayName("Admin.Announcement.Fields.SendTestEmailTo")]

        public string SendTestEmailTo { get; set; }

      

        public string ProjectName { get; set; }
        public IList<EmployeeModel> LikedEmployees { get; set; } 
        public IList<EmployeeModel> RemainingEmployees { get; set; } 

        public IList<SelectListItem> AvailableProjects { get; set; }
        public IList<SelectListItem> AvailableDesignation { get; set; }

        public IList<SelectListItem> Projects { get; set; }

        public IList<SelectListItem> AvailableEmployees { get; set; }
        public IList<SelectListItem> AvailableAudienceTypes { get; set; }


    }
}
