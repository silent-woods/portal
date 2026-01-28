using App.Core.Domain.Activities;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Leaves;
using App.Core.Domain.Projects;
using App.Core.Domain.ProjectTasks;
using App.Data.Extensions;
using App.Services;
using App.Services.Activities;
using App.Services.Designations;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Areas.Admin.Models.Extension.Activities;
using App.Web.Areas.Admin.Models.Extension.Announcements;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.Extension.TaskComments;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Areas.Admin.Models.Projects;
using App.Web.Framework.Models.Extensions;
using App.Web.Models.Boards;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the leaveManagement model factory implementation
    /// </summary>
    public partial class AnnouncementModelFactory : IAnnouncementModelFactory
    {
        #region Fields

        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectsService _projectService;
        private readonly IActivityService _activityService;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly IAnnouncementService _announcementService;
        private readonly IDesignationService _designationService;
        #endregion

        #region Ctor

        public AnnouncementModelFactory(ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IBaseAdminModelFactory baseAdminModelFactory
,
            IProjectsService projectService,

            IProjectTaskService projectTaskService,
            IActivityService activityService,
            ITimeSheetsService timeSheetsService
,
            IAnnouncementService announcementService,
            IDesignationService designationService)
        {
            _leaveManagementService = leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _projectTaskService = projectTaskService;
            _projectService = projectService;
            _activityService = activityService;
            _timeSheetsService = timeSheetsService;
            _announcementService = announcementService;
            _designationService = designationService;
        }

        #endregion

        #region Utilities

        private string FormatEnumValue(string enumValue)
        {
            // Check if the enum value contains underscores
            if (enumValue.Contains("_"))
            {
                var valueWithoutUnderscores = enumValue.Replace("_", string.Empty);

                // Remove any spaces that are followed by a capital letter
                var result = System.Text.RegularExpressions.Regex.Replace(valueWithoutUnderscores, "(?<=\\w) (?=[A-Z])", string.Empty);

                return result;
            }
            else
            {
                // Insert spaces before capital letters (ignoring the first character)
                return System.Text.RegularExpressions.Regex.Replace(enumValue, "(?<!^)(?=[A-Z])", " ");
            }
        }


        public virtual async Task PrepareProjectListAsync(AnnouncementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

          
            var projectTaskName = "";
            var projects = await _projectService.GetAllProjectsAsync(projectTaskName);

            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.ProjectName = p.ProjectTitle;
                    model.AvailableProjects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }

        public virtual async Task PrepareProjectListByEmployeeAsync(ProjectTaskModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

     
      
            var projects = await _projectService.GetProjectListByEmployee(model.EmployeeId);

            foreach (var p in projects)
            {
                if (p.StatusId != 4)
                {
                    model.ProjectName = p.ProjectTitle;
                    model.Projects.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        public virtual async Task PrepareEmployeeListAsync(AnnouncementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

  
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareAudienceTypeListAsync(AnnouncementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));


            model.AvailableAudienceTypes = Enum.GetValues(typeof(AudienceType))
     .Cast<AudienceType>()
     .Select(a => new SelectListItem
     {
         Value = ((int)a).ToString(),
         Text = a.ToString()
     })
     .ToList();

           
        }

        public virtual async Task PrepareDesignationListAsync(AnnouncementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));


            var designations = await _designationService.GetAllDesignationAsync("");
            foreach (var p in designations)
            {
                model.AvailableDesignation.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareEmployeeListAsync(ActivitySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.AvailableEmployee.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                searchModel.AvailableEmployee.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareProjectListAsync(ActivitySearchModel searchmodel)
        {
            if (searchmodel == null)
                throw new ArgumentNullException(nameof(searchmodel));

            searchmodel.AvailableProject.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var projectTaskName = "";
            var leaves = await _projectService.GetAllProjectsAsync(projectTaskName);
            foreach (var p in leaves)
            {
                if (p.StatusId != 4)
                {
                    searchmodel.AvailableProject.Add(new SelectListItem
                    {
                        Text = p.ProjectTitle,
                        Value = p.Id.ToString()
                    });
                }
            }
        }
        

        #endregion
        #region Methods

        public virtual async Task<AnnouncementSearchModel> PrepareAnnouncementSearchModelAsync(AnnouncementSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
          

            return searchModel;
        }


        public virtual async Task<AnnouncementListModel> PrepareAnnouncementListModelAsync(AnnouncementSearchModel searchModel)
        {
            var announcements = await _announcementService.GetAllAnnouncementsAsync(
                title: searchModel.SearchTitle,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize,
                showHidden: true);

            var model = await new AnnouncementListModel().PrepareToGridAsync(searchModel, announcements, () =>
            {
                return announcements.SelectAwait(async announcementEntity =>
                {
                    var announcementModel = announcementEntity.ToModel<AnnouncementModel>();

                    // Convert dates
                    announcementModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(
                        announcementEntity.CreatedOnUtc, DateTimeKind.Utc);

                    announcementModel.ScheduledOnUtc = announcementEntity.ScheduledOnUtc.HasValue
                        ? await _dateTimeHelper.ConvertToUserTimeAsync(announcementEntity.ScheduledOnUtc.Value, DateTimeKind.Utc)
                        : null;



                    var istTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");


                    if (announcementModel.CreatedOnUtc != null)
                        announcementModel.CreatedOnUtc = TimeZoneInfo.ConvertTimeFromUtc(announcementModel.CreatedOnUtc, istTimeZone);

                    if (announcementModel.ScheduledOnUtc != null)
                        announcementModel.ScheduledOnUtc = TimeZoneInfo.ConvertTimeFromUtc(announcementModel.ScheduledOnUtc.Value, istTimeZone);



                    announcementModel.IsSent = announcementEntity.IsSent;
                    announcementModel.AudienceTypeName = ((AudienceType)announcementEntity.AudienceTypeId).ToString();


                    // Audience reference name
                    if (announcementEntity.AudienceTypeId == (int)AudienceType.Project && !string.IsNullOrEmpty(announcementEntity.ReferenceIds))
                    {
                        var projectIds = announcementEntity.ReferenceIds.Split(',').Select(int.Parse).ToArray();
                        var projects = await _projectService.GetProjectsByIdsAsync(projectIds);
                        announcementModel.ReferenceName = string.Join(", ", projects.Select(p => p.ProjectTitle));
                    }
                    else if (announcementEntity.AudienceTypeId == (int)AudienceType.Designation && !string.IsNullOrEmpty(announcementEntity.ReferenceIds))
                    {
                        var designationIds = announcementEntity.ReferenceIds.Split(',').Select(int.Parse).ToArray();
                        var designations = await _designationService.GetDesignationsByIdsAsync(designationIds);
                        announcementModel.ReferenceName = string.Join(", ", designations.Select(d => d.Name));
                    }
                    else if (announcementEntity.AudienceTypeId == (int)AudienceType.Employee && !string.IsNullOrEmpty(announcementEntity.ReferenceIds))
                    {
                        var employeeIds = announcementEntity.ReferenceIds.Split(',').Select(int.Parse).ToArray();
                        var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIds);
                        announcementModel.ReferenceName = string.Join(", ", employees.Select(e => e.FirstName + " " + e.LastName));
                    }

                    return announcementModel;
                }).Where(x => x != null);
            });

            return model;
        }


        public virtual async Task<AnnouncementModel> PrepareAnnouncementModelAsync(
       AnnouncementModel model,
       Announcement announcement,
       bool excludeProperties = false)
        {
            if (announcement != null)
            {
                // Fill in model values from the entity
                if (model == null)
                {
                    model = announcement.ToModel<AnnouncementModel>();

                    // Convert dates
                    model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(
                        announcement.CreatedOnUtc, DateTimeKind.Utc);

                    model.ScheduledOnUtc = announcement.ScheduledOnUtc.HasValue
                        ? await _dateTimeHelper.ConvertToUserTimeAsync(announcement.ScheduledOnUtc.Value, DateTimeKind.Utc)
                        : null;

                    model.IsSent = announcement.IsSent;

                    // Audience reference name
                    if (announcement.AudienceTypeId == (int)AudienceType.Project && !string.IsNullOrEmpty(announcement.ReferenceIds))
                    {
                        var projectIds = announcement.ReferenceIds.Split(',').Select(int.Parse).ToArray();
                        var projects = await _projectService.GetProjectsByIdsAsync(projectIds);
                        model.ReferenceName = string.Join(", ", projects.Select(p => p.ProjectTitle));
                    }
                    else if (announcement.AudienceTypeId == (int)AudienceType.Designation && !string.IsNullOrEmpty(announcement.ReferenceIds))
                    {
                        var designationIds = announcement.ReferenceIds.Split(',').Select(int.Parse).ToArray();
                        var designations = await _designationService.GetDesignationsByIdsAsync(designationIds);
                        model.ReferenceName = string.Join(", ", designations.Select(d => d.Name));
                    }
                    else if (announcement.AudienceTypeId == (int)AudienceType.Employee && !string.IsNullOrEmpty(announcement.ReferenceIds))
                    {
                        var employeeIds = announcement.ReferenceIds.Split(',').Select(int.Parse).ToArray();
                        var employees = await _employeeService.GetEmployeesByIdsAsync(employeeIds);
                        model.ReferenceName = string.Join(", ", employees.Select(e => e.FirstName + " " + e.LastName));
                    }

                    model.SendEmployeeIdList = !string.IsNullOrEmpty(announcement.SendEmployeeIds)
          ? announcement.SendEmployeeIds.Split(',').Select(int.Parse).ToList()
          : new List<int>();

                    model.ReferenceIdList = !string.IsNullOrEmpty(announcement.ReferenceIds)
                        ? announcement.ReferenceIds.Split(',').Select(int.Parse).ToList()
                        : new List<int>();


                    var likedIds = (announcement.LikedEmployeeIds ?? "")
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(int.Parse)
                                        .ToList();

                    // Get audience employees (based on Announcement.SendEmployeeIds)
                    var audienceIds = (announcement.SendEmployeeIds ?? "")
                                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(int.Parse)
                                        .ToList();

                    var audienceEmployees = await _employeeService.GetEmployeesByIdsAsync(audienceIds.ToArray());
                    var likedEmployees = audienceEmployees.Where(e => likedIds.Contains(e.Id)).ToList();
                    var remainingEmployees = audienceEmployees.Where(e => !likedIds.Contains(e.Id)).ToList();

                    model.LikedEmployees = likedEmployees
                        .Select(e => new EmployeeModel { Id = e.Id, FirstName = e.FirstName ,LastName= e.LastName })
                        .ToList();

                    model.RemainingEmployees = remainingEmployees
                        .Select(e => new EmployeeModel { Id = e.Id, FirstName = e.FirstName, LastName = e.LastName })
                        .ToList();

                }
            }

            // Dropdown helpers (multi-selects for Project, Designation, Employee)
            await PrepareProjectListAsync(model);
            await PrepareDesignationListAsync(model);
            await PrepareEmployeeListAsync(model);
            await PrepareAudienceTypeListAsync(model);


            return model;
        }



        #endregion
    }
}


