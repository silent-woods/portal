using App.Core;
using App.Core.Domain.Activities;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.ProjectTasks;
using App.Core.Domain.TimeSheets;
using App.Data;
using App.Services.Activities;
using App.Services.Customers;
using App.Services.EmployeeAttendances;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.Projects;
using App.Services.ProjectTasks;
using App.Services.Security;
using App.Services.TimeSheets;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.ProjectTasks;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Mvc.Filters;
using DocumentFormat.OpenXml.Office2021.DocumentTasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Collections.Generic;


using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class TimeSheetController : BaseAdminController
    {
        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ITimeSheetModelFactory _timeSheetModelFactory;
        private readonly ITimeSheetsService _timeSheetsService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IProjectsService _projectsService;
        private readonly IProjectTaskService _projectTaskService;
        private readonly IProjectTaskModelFactory _projectTaskModelFactory;
        private readonly IEmployeeService _employeeService;
        private readonly MonthlyReportSetting _monthlyReportSettings;
        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IActivityService _activityService;
        private readonly IWorkContext _workContext;
        private readonly ITaskCommentsService _taskCommentsService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkflowStatusService _workflowStatusService;
        
        #endregion

        #region Ctor

        public TimeSheetController(IPermissionService permissionService,
            ITimeSheetModelFactory timeSheetModelFactory,
            ITimeSheetsService timeSheetsService,
            INotificationService notificationService,
            ILocalizationService localizationService,IProjectsService projectsService,IProjectTaskService projectTaskService,
            IProjectTaskModelFactory projectTaskModelFactory,
            IEmployeeService employeeService, MonthlyReportSetting monthlyReportSettings,
            IEmployeeAttendanceService employeeAttendanceService,
            IDateTimeHelper dateTimeHelper,
            IActivityService activityService,
            IWorkContext workContext,
            ITaskCommentsService taskCommentsService,
            ICustomerService customerService,
            IWorkflowMessageService workflowMessageService,
            IWorkflowStatusService workflowStatusService
             
            )
        {
            _permissionService = permissionService;
            _timeSheetModelFactory = timeSheetModelFactory;
            _timeSheetsService = timeSheetsService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _projectsService = projectsService;
            _projectTaskService = projectTaskService;
            _projectTaskModelFactory = projectTaskModelFactory;
            _employeeService = employeeService;
            _monthlyReportSettings = monthlyReportSettings;
            _employeeAttendanceService = employeeAttendanceService;
            _dateTimeHelper = dateTimeHelper;
            _activityService = activityService;
            _workContext = workContext;
            _taskCommentsService = taskCommentsService;
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _workflowStatusService = workflowStatusService;
        }

        #endregion
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();
            //prepare model
            var model = await _timeSheetModelFactory.PrepareTimeSheetSearchModelAsync(new TimeSheetSearchModel());
            return View("/Areas/Admin/Views/Extension/TimeSheet/List.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> List(TimeSheetSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            //prepare model
            var model = await _timeSheetModelFactory.PrepareTimeSheetListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            //prepare model
            var model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(new TimeSheetModel(), null);
          
            model.SpentDate = await _dateTimeHelper.GetUTCAsync();
            return View("/Areas/Admin/Views/Extension/TimeSheet/Create.cshtml", model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            if (timeSheet == null)
                return RedirectToAction("List");
                    
            //prepare model
            var model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(null, timeSheet);
            var activity = await _activityService.GetActivityByIdAsync(model.ActivityId);
            if (activity != null)
                model.ActivityName = activity.ActivityName;


            return View("/Areas/Admin/Views/Extension/TimeSheet/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]

        public virtual async Task<IActionResult> Edit(TimeSheetModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();
            // Validate parameters
            if (model.Id <= 0 || model.ProjectId <= 0 || model.TaskId <= 0 ||  model.SpentTime == "")
            {
                model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(model, null, true);
                return View("/Areas/Admin/Views/Extension/TimeSheet/Edit.cshtml", model);
            }

                var timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(model.Id);
            if (timeSheet == null)
                return RedirectToAction("List");
            var prevTaskId = timeSheet.TaskId;
            var prevSpentHours = timeSheet.SpentHours;
            var prevSpentMinutes = timeSheet.SpentMinutes;

            var prevEmployeeId = timeSheet.EmployeeId;
            var prevSpentDate = timeSheet.SpentDate;
            
            model.EmployeeId = model.SelectedEmployeeId.FirstOrDefault();

            (model.SpentHours, model.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);

            if (ModelState.IsValid)
            {
                // Update time sheet properties
               
                timeSheet.SpentDate = model.SpentDate.GetValueOrDefault();
                timeSheet.ProjectId = model.ProjectId;
                timeSheet.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                //timeSheet.EstimatedHours = model.EstimatedHours;
                timeSheet.SpentHours = model.SpentHours;
                timeSheet.Billable = model.Billable;
                timeSheet.TaskId = model.TaskId;
                timeSheet.EmployeeId = model.EmployeeId;
                (timeSheet.SpentHours, timeSheet.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(model.SpentTime);
                
                    var prevProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(prevTaskId);
                    if (prevProjectTask != null)
                    {

                    (prevProjectTask.SpentHours, prevProjectTask.SpentMinutes) = await _timeSheetsService.SubtractSpentTimeAsync(prevProjectTask.SpentHours, prevProjectTask.SpentMinutes, prevSpentHours, prevSpentMinutes);
                        await _projectTaskService.UpdateProjectTaskAsync(prevProjectTask);
                    }


                await _timeSheetsService.UpdateOrCreateActivityAsync(
     timeSheet,
     timeSheet,
     model.ActivityName,
     prevSpentHours,
     timeSheet.SpentHours,
     prevSpentMinutes,
     timeSheet.SpentMinutes,
     model.TaskId
 );


                //var activity = await _activityService.GetActivityByTaskIdAndActivityNameAsync(model.TaskId, model.ActivityName);
                //if (activity != null)
                //    timeSheet.ActivityId = activity.Id;

                await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);

                // Update spent time for new task
                var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(model.TaskId);
                if (projectTask != null)
                {
                    projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask);

                 
                    (projectTask.SpentHours, projectTask.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes, model.SpentHours, model.SpentMinutes);
                    await _projectTaskService.UpdateProjectTaskAsync(projectTask);

                }

                await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(timeSheet.SpentDate, timeSheet.EmployeeId, timeSheet.SpentHours,timeSheet.SpentMinutes ,prevSpentDate, prevEmployeeId);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.TimeSheet.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }


            model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(model, null, true);
            return View("/Areas/Admin/Views/Extension/TimeSheet/Edit.cshtml", model);
        }


        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            if (timeSheet == null)
                return RedirectToAction("List");

            await _timeSheetsService.DeleteTimeSheetAsync(timeSheet);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.TimeSheet.Deleted"));

            return RedirectToAction("List");
        }
        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _timeSheetsService.GetTimeSheetByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _timeSheetsService.DeleteTimeSheetAsync(item);
            }
            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> GetTasksByProject(int projectId, int? selectedTaskId = null)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();
            if (projectId == 0)
                return Json(new List<object>()); // Return an empty list if no project is selected

            var tasks = await _projectTaskService.GetProjectTasksByProjectIdForTimeSheet(projectId);

            // Return tasks as JSON directly
            var taskList = tasks.Select(task => new
            {
                Value = task.Id.ToString(),
                Text = task.TaskTitle,
                Selected = task.Id == selectedTaskId
            }).ToList();

            return Json(taskList);
        }



        public virtual async Task<IActionResult> GetEstimatedTimeByTask(int taskId)
        {
            var task = await _projectTaskService.GetProjectTasksByIdAsync(taskId);

            if (task != null)
            {
                var spentTime = await _timeSheetsService.ConvertSpentTimeAsync(task.SpentHours, task.SpentMinutes);
                var EstimationTime = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);
                return Json(new { EstimatedTime = EstimationTime, TotalSpent = spentTime });
            }

            return Json(new { EstimatedTime = "", TotalSpent = 0 });
        }

        public virtual async Task<IActionResult> GetProjects()
        {
            
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            // Fetch projects from service
            var projects = await _projectsService.GetAllProjectsListAsync();

            // Map projects to SelectListItems or a similar structure without a default option
            var projectList = projects.Select(project => new
            {
                Value = project.Id.ToString(),
                Text = project.ProjectTitle
            }).ToList();

            return Json(projectList);
        }

        [HttpGet]
        public async Task<IActionResult> GetPreviousEntries(int employeeId, string spentDate)
        {
           
            if (employeeId == 0 || string.IsNullOrEmpty(spentDate))
            {
                return Json(new List<TimeSheetRowModel>()); // Return an empty list if parameters are missing
            }

            // Parse the spentDate string to a DateTime object
            if (!DateTime.TryParse(spentDate, out var date))
            {
                return BadRequest("Invalid date format");
            }
            
            // Fetch previous entries from the database
            //var entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, date);

            ViewBag.EmployeeId = employeeId;

            var projectTasks =await  _projectTaskService.GetAllProjectTasksAsync(); 
            var projects = await _projectsService.GetAllProjectsListAsync();
            var entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, date);
           
            ViewBag.EmployeeId = employeeId;

            var timeSheetRows = entries
                .Where(pt => projectTasks.Any(ptt => ptt.Id == pt.TaskId && ptt.ProjectId == pt.ProjectId))
                .Where(pt => projects.Any(p => p.Id == pt.ProjectId))
                .Select(pt => new TimeSheetRowModel
                {
                    Id = pt.Id,
                    ProjectId = pt.ProjectId,
                    TaskId = pt.TaskId,
                    //EstimatedHours = pt.EstimatedHours,
                    ActivityId = pt.ActivityId,
                    SpentHours = pt.SpentHours,
                    SpentMinutes = pt.SpentMinutes,
                    Billable = pt.Billable,
                    EmployeeId = pt.EmployeeId
                })
            .ToList();

            for (int i = 0; i < timeSheetRows.Count; i++)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(timeSheetRows[i].TaskId);
                if (task != null)
                {
                    timeSheetRows[i].TotalSpent = await _timeSheetsService.ConvertSpentTimeAsync(task.SpentHours,task.SpentMinutes);
                  
                    timeSheetRows[i].EstimatedTimeHHMM = await _timeSheetsService.ConvertToHHMMFormat(task.EstimatedTime);


                }
                var activity = await _activityService.GetActivityByIdAsync(timeSheetRows[i].ActivityId);
                if (activity != null)
                    timeSheetRows[i].ActivityName = activity.ActivityName;
                else
                    timeSheetRows[i].ActivityName = "";


                var time = await _timeSheetsService.ConvertSpentTimeAsync(timeSheetRows[i].SpentHours, timeSheetRows[i].SpentMinutes);
                if (time != null)
                    timeSheetRows[i].SpentTime = time;
            }

            return Json(timeSheetRows);
        }



        public async Task<IActionResult> GetRecentEntries(int employeeId, string spentDate)
        {
            if (employeeId == 0 || string.IsNullOrEmpty(spentDate))
            {
                return Json(new List<TimeSheetRowModel>()); // Return an empty list if parameters are missing
            }

            // Parse the spentDate string to a DateTime object
            if (!DateTime.TryParse(spentDate, out var date))
            {
                return BadRequest("Invalid date format");
            }

            // Fetch previous entries from the database
            var entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, date);

            // If no entries are found for the given date, try to find the most recent previous entry for the previous 5 days
            if (entries.Count == 0)
            {
                for (int i = 1; i <= 5; i++)
                {
                    var previousDate = date.AddDays(-i);
                    entries = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(employeeId, previousDate);
                    if (entries.Count > 0)
                    {
                        break;
                    }
                }
            }

            ViewBag.EmployeeId = employeeId;

            var projectTasks = await _projectTaskService.GetAllProjectTasksAsync(); 
            var projects = await _projectsService.GetAllProjectsListAsync();

            var timeSheetRows = entries
                .Where(pt => projectTasks.Any(ptt => ptt.Id == pt.TaskId && ptt.ProjectId == pt.ProjectId))
                .Where(pt => projects.Any(p => p.Id == pt.ProjectId)) // filter out rows where TaskId does not exist in ProjectTask table
                .Select(pt => new TimeSheetRowModel
                {
               
                    ProjectId = pt.ProjectId,
                    TaskId = pt.TaskId,
                    //EstimatedHours = pt.EstimatedHours,
                    SpentHours = pt.SpentHours,
                    SpentMinutes = pt.SpentMinutes,
                    Billable = pt.Billable,
                    EmployeeId = pt.EmployeeId,
             ActivityId  = pt.ActivityId
                    
                })
            .ToList();

            for (int i = 0; i < timeSheetRows.Count; i++)
            {
                var task = await _projectTaskService.GetProjectTasksByIdAsync(timeSheetRows[i].TaskId);
                {
                    timeSheetRows[i].TotalSpent = await _timeSheetsService.ConvertSpentTimeAsync(task.SpentHours, task.SpentMinutes);
                    timeSheetRows[i].EstimatedHours = task.EstimatedTime;


                }


                var activity = await _activityService.GetActivityByIdAsync(timeSheetRows[i].ActivityId);
                if (activity != null)
                    timeSheetRows[i].ActivityName = activity.ActivityName;


                var time = await _timeSheetsService.ConvertSpentTimeAsync(timeSheetRows[i].SpentHours, timeSheetRows[i].SpentMinutes);
                if (time != null)
                    timeSheetRows[i].SpentTime = time;

            }

            return Json(timeSheetRows);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SaveChanges(TimeSheetModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            // Handle employee selection
            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeId = selectedEmployeeId;

            // Validate employee selection
            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(model, null, true);
                return View("/Areas/Admin/Views/Extension/TimeSheet/Create.cshtml", model);
            }

            // Initialize timeSheet object
            TimeSheet timeSheet = model.Id > 0
                ? await _timeSheetsService.GetTimeSheetByIdAsync(model.Id)
                : new TimeSheet { CreateOnUtc = await _dateTimeHelper.GetUTCAsync() };

            if (timeSheet == null)
                return RedirectToAction("List");

            // Update timeSheet properties
            timeSheet.EmployeeId = model.EmployeeId;
            timeSheet.SpentDate = model.SpentDate.GetValueOrDefault();
            timeSheet.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

            if (ModelState.IsValid)
            {
                // Handle existing rows: reduce their spent time
                if (model.Id > 0)
                {
                    var existingTimeSheetRows = await _timeSheetsService.GetTimeSheetByEmpAndSpentDate(model.EmployeeId, model.SpentDate.GetValueOrDefault());

                    foreach (var existingRow in existingTimeSheetRows)
                    {
                        var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(existingRow.TaskId);
                        if (projectTask != null)
                        {
                            projectTask.SpentHours -= existingRow.SpentHours;
                            await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                        }
                    }
                }

                // Update or insert time sheet
                if (model.Id > 0)
                {
                    // Update existing time sheet
                    timeSheet = model.ToEntity(timeSheet);
                    await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);
                }
                else
                {
                    // Insert new time sheet
                    await _timeSheetsService.InsertTimeSheetAsync(timeSheet);

                }

                // Add new rows or update existing rows
                foreach (var row in model.TimeSheetRows)
                {
                    var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(row.TaskId);
                    if (projectTask != null)
                    {
                        projectTask.SpentHours += row.SpentHours;
                        await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                    }
                }

                // Notify success
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync(model.Id > 0 ? "Admin.Catalog.TimeSheet.Updated" : "Admin.Catalog.TimeSheet.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = timeSheet.Id });
            }

            // Prepare model if validation fails
            model = await _timeSheetModelFactory.PrepareTimeSheetModelAsync(model, timeSheet, true);

            // Redisplay form if validation fails
            return View("/Areas/Admin/Views/Extension/TimeSheet/Create.cshtml", model);
        }
        [HttpGet]
        public virtual async Task<IActionResult> DeleteRow(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var timeSheet = await _timeSheetsService.GetTimeSheetByIdWithoutCacheAsync(id);
            if (timeSheet == null)
                return RedirectToAction("List");

           

            await _timeSheetsService.DeleteTimeSheetAsync(timeSheet);

            // Return a success message
            return Ok("Row deleted successfully.");
        }

        //[HttpPost]
        //public virtual async Task<IActionResult> UpdateRow(int id, int projectId, int taskId, int prevTaskId, string activityName, string prevActivityName, decimal estimatedHours, decimal totalSpent, decimal spentHours, decimal prevSpentHours, bool billable)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
        //        return AccessDeniedView();

        //    // Validate parameters
        //    if (id <= 0 || projectId <= 0 || taskId <= 0 || estimatedHours < 0 || spentHours < 0)
        //    {
        //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.InvalidParameters"));
        //        return BadRequest();
        //    }

        //    // Fetch the existing time sheet
        //    TimeSheet timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
        //    if (timeSheet == null)
        //    {
        //        _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.TimeSheet.NotFound"));
        //        return NotFound();
        //    }

        //    // Update time sheet properties
        //    timeSheet.EmployeeId = timeSheet.EmployeeId; // Assuming EmployeeId remains unchanged
        //    timeSheet.SpentDate = timeSheet.SpentDate; // Assuming SpentDate remains unchanged

        //    // Fetch existing time sheet rows
        //    var existingRows = await _timeSheetsService.GetTimeSheetByIdAsync(id);

        //    // Update or add rows
        //    var existingRow = existingRows;
        //    if (existingRow != null)
        //    {
        //        // Update existing row
        //        existingRow.TaskId = taskId;
        //        existingRow.SpentHours = spentHours;
        //        existingRow.Billable = billable;
        //        existingRow.ProjectId = projectId;
        //        existingRow.EstimatedHours = estimatedHours;

        //        // Update spent time for old task
        //        var oldProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(prevTaskId);
        //        if (oldProjectTask != null)
        //        {
        //            oldProjectTask.SpentHours -= prevSpentHours; // Reduce previous time

        //            if (oldProjectTask.IsManualDOT == false)
        //            {
        //                var allowedVariations = _monthlyReportSettings.AllowedVariations;
        //                // Calculate the allowed variation in hours based on the percentage
        //                var allowedVariation = estimatedHours * (allowedVariations / 100);
        //                oldProjectTask.DeliveryOnTime = oldProjectTask.SpentHours <= (estimatedHours + allowedVariation);

        //            }

        //            await _projectTaskService.UpdateProjectTaskAsync(oldProjectTask);
        //        }

        //        // Update spent time for new task
        //        var newProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
        //        if (newProjectTask != null)
        //        {
        //            newProjectTask.SpentHours += spentHours; // Add new time

        //            if (newProjectTask.IsManualDOT == false)
        //            {
        //                var allowedVariations = _monthlyReportSettings.AllowedVariations;
        //                // Calculate the allowed variation in hours based on the percentage
        //                var allowedVariation = estimatedHours * (allowedVariations / 100);
        //                newProjectTask.DeliveryOnTime = newProjectTask.SpentHours <= (estimatedHours + allowedVariation);

        //            }

        //            await _projectTaskService.UpdateProjectTaskAsync(newProjectTask);
        //        }

        //        // Update existing row in the database
        //        await _timeSheetsService.UpdateTimeSheetAsync(existingRow);
        //    }
        //    else
        //    {
        //        // Handle case where the row does not exist
        //        var newRow = new TimeSheet
        //        {
        //            Id = id,
        //            ProjectId = projectId,
        //            TaskId = taskId,
        //            EstimatedHours = estimatedHours,
        //            SpentHours = spentHours,
        //            Billable = billable
        //        };

        //        await _timeSheetsService.InsertTimeSheetAsync(newRow);
        //    }

        //    // Update the time sheet
        //    await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);

        //    var updatedTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);

        //    //update attendance based on timesheet
        //    await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(timeSheet.SpentDate, timeSheet.EmployeeId, timeSheet.SpentHours, timeSheet.SpentDate, timeSheet.EmployeeId);

        //    return Json(new { success = true, updatedTotalSpent = updatedTask.SpentHours });
        //}




        //  [HttpPost]
        //  public virtual async Task<IActionResult> UpdateRow(
        //int id,
        //int projectId,
        //int taskId,
        //int prevTaskId,
        //string activityName,
        //string prevActivityName,
        //decimal estimatedHours,
        //decimal totalSpent,
        //decimal spentHours,
        //decimal prevSpentHours,
        //bool billable)
        //  {
        //      if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
        //          return AccessDeniedView();

        //      // Validate parameters
        //      if (id <= 0 || projectId <= 0 || taskId <= 0 || estimatedHours < 0 || spentHours < 0)
        //      {
        //          _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.InvalidParameters"));
        //          return BadRequest();
        //      }

        //      // Fetch the existing time sheet
        //      TimeSheet timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
        //      if (timeSheet == null)
        //      {
        //          _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.TimeSheet.NotFound"));
        //          return NotFound();
        //      }

        //      // Update time sheet properties
        //      timeSheet.EmployeeId = timeSheet.EmployeeId; // Assuming EmployeeId remains unchanged
        //      timeSheet.SpentDate = timeSheet.SpentDate; // Assuming SpentDate remains unchanged

        //      // Fetch existing time sheet rows
        //      var existingRows = await _timeSheetsService.GetTimeSheetByIdAsync(id);

        //      // Update or add rows
        //      var existingRow = existingRows;
        //      if (existingRow != null)
        //      {
        //          // Update existing row
        //          existingRow.TaskId = taskId;
        //          existingRow.SpentHours = spentHours;
        //          existingRow.Billable = billable;
        //          existingRow.ProjectId = projectId;
        //          existingRow.EstimatedHours = estimatedHours;

        //          // Update spent time for old task
        //          var oldProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(prevTaskId);
        //          if (oldProjectTask != null)
        //          {
        //              oldProjectTask.SpentHours -= prevSpentHours; // Reduce previous time

        //              if (oldProjectTask.IsManualDOT == false)
        //              {
        //                  var allowedVariations = _monthlyReportSettings.AllowedVariations;
        //                  // Calculate the allowed variation in hours based on the percentage
        //                  var allowedVariation = estimatedHours * (allowedVariations / 100);
        //                  oldProjectTask.DeliveryOnTime = oldProjectTask.SpentHours <= (estimatedHours + allowedVariation);
        //              }

        //              await _projectTaskService.UpdateProjectTaskAsync(oldProjectTask);
        //          }

        //          // Update spent time for new task
        //          var newProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
        //          if (newProjectTask != null)
        //          {
        //              newProjectTask.SpentHours += spentHours; // Add new time

        //              if (newProjectTask.IsManualDOT == false)
        //              {
        //                  var allowedVariations = _monthlyReportSettings.AllowedVariations;
        //                  // Calculate the allowed variation in hours based on the percentage
        //                  var allowedVariation = estimatedHours * (allowedVariations / 100);
        //                  newProjectTask.DeliveryOnTime = newProjectTask.SpentHours <= (estimatedHours + allowedVariation);
        //              }

        //              await _projectTaskService.UpdateProjectTaskAsync(newProjectTask);
        //          }

        //          // Handle activity creation if not exists
        //          if (string.IsNullOrWhiteSpace(activityName))
        //              throw new ArgumentException("Activity name cannot be null or empty.", nameof(activityName));

        //          // Check if the activity already exists
        //          var existingActivity = await _activityService.GetAllActivitiesAsync(activityName, 0, taskId);

        //          if (existingActivity.Count != 0)
        //          {
        //              var activity = existingActivity.First();
        //              var activityId = activity.Id; // Get the ID of the existing activity

        //              if (timeSheet != null)
        //              {
        //                  if (timeSheet.ActivityId != activity.Id)
        //                  {
        //                      // Subtract spent hours from the old activity
        //                      if (timeSheet.ActivityId != 0)
        //                      {
        //                          var oldActivity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
        //                          if (oldActivity != null)
        //                          {
        //                              oldActivity.SpentHours -= prevSpentHours;
        //                              oldActivity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
        //                              await _activityService.UpdateActivityAsync(oldActivity);
        //                          }
        //                      }

        //                      // Add spent hours to the new activity
        //                      activity.SpentHours += spentHours;
        //                      activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
        //                      await _activityService.UpdateActivityAsync(activity);

        //                      // Update the timesheet with the new activity
        //                      timeSheet.ActivityId = activity.Id;
        //                  }
        //                  else
        //                  {
        //                      // Calculate the difference in spent hours for the same activity
        //                      var diff = spentHours - prevSpentHours;
        //                      if (diff != 0)
        //                      {
        //                          activity.SpentHours += diff;
        //                          activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
        //                          await _activityService.UpdateActivityAsync(activity);
        //                      }
        //                  }

        //                  // Update the timesheet's spent hours
        //                  timeSheet.SpentHours = spentHours;
        //                  await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);

        //              }


        //          }
        //          else
        //          {

        //              if (timeSheet.ActivityId != 0)
        //              {
        //                  var oldActivity = await _activityService.GetActivityByIdAsync(timeSheet.ActivityId);
        //                  if (oldActivity != null)
        //                  {
        //                      oldActivity.SpentHours -= prevSpentHours;
        //                      oldActivity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
        //                      await _activityService.UpdateActivityAsync(oldActivity);
        //                  }
        //              }
        //              // Create a new activity
        //              var newActivity = new Activity
        //              {
        //                  ActivityName = activityName,
        //                  EmployeeId = timeSheet.EmployeeId,
        //                  TaskId = taskId,
        //                  SpentHours = spentHours,
        //                  CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
        //                  UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
        //              };

        //              await _activityService.InsertActivityAsync(newActivity);

        //              // Update the timesheet with the new activity ID
        //              if (timeSheet != null)
        //              {
        //                  timeSheet.ActivityId = newActivity.Id;
        //                  timeSheet.SpentHours = spentHours;
        //                  await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);
        //              }

        //              // Update existing row in the database
        //              await _timeSheetsService.UpdateTimeSheetAsync(existingRow);
        //          }
        //      }
        //      else
        //      {
        //          // Handle case where the row does not exist
        //          var newRow = new TimeSheet
        //          {
        //              Id = id,
        //              ProjectId = projectId,
        //              TaskId = taskId,
        //              EstimatedHours = estimatedHours,
        //              SpentHours = spentHours,
        //              Billable = billable
        //          };

        //          await _timeSheetsService.InsertTimeSheetAsync(newRow);
        //      }

        //      // Update the time sheet
        //      await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);

        //      var updatedTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);

        //      // Update attendance based on timesheet
        //      await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(
        //          timeSheet.SpentDate, timeSheet.EmployeeId, timeSheet.SpentHours,
        //          timeSheet.SpentDate, timeSheet.EmployeeId);

        //      return Json(new { success = true, updatedTotalSpent = updatedTask.SpentHours });
        //  }


        [HttpPost]
        public virtual async Task<IActionResult> UpdateRow(
int id,
int projectId,
int taskId,
int prevTaskId,
string activityName,
string prevActivityName,
decimal estimatedHours,
decimal totalSpent,
 string spentTime,
 string prevspentTime,
bool billable)
        {

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var (spentHours, spentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(spentTime);
            var (prevSpentHours, prevSpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(prevspentTime);
            // Validate parameters
            if (id <= 0 || projectId <= 0 || taskId <= 0 || estimatedHours < 0 || spentHours < 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.InvalidParameters"));
                return BadRequest();
            }

            // Fetch the existing time sheet
            TimeSheet timeSheet = await _timeSheetsService.GetTimeSheetByIdAsync(id);
            if (timeSheet == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.TimeSheet.NotFound"));
                return NotFound();
            }

            // Update time sheet properties
            timeSheet.EmployeeId = timeSheet.EmployeeId; 
            timeSheet.SpentDate = timeSheet.SpentDate; 

            // Fetch existing time sheet rows
            var existingRows = await _timeSheetsService.GetTimeSheetByIdAsync(id);

            // Update or add rows
            var existingRow = existingRows;
            if (existingRow != null)
            {
                // Update existing row
                existingRow.TaskId = taskId;
                existingRow.SpentHours = spentHours;
                existingRow.SpentMinutes = spentMinutes;
                existingRow.Billable = billable;
                existingRow.ProjectId = projectId;
                //existingRow.EstimatedHours = estimatedHours;

                // Update spent time for old task
                var oldProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(prevTaskId);
                if (oldProjectTask != null)
                {
                    (oldProjectTask.SpentHours, oldProjectTask.SpentMinutes) = await _timeSheetsService.SubtractSpentTimeAsync(oldProjectTask.SpentHours, oldProjectTask.SpentMinutes, prevSpentHours, prevSpentMinutes);// Reduce previous time

                   

                    await _projectTaskService.UpdateProjectTaskAsync(oldProjectTask);
                }

                // Update spent time for new task
                var newProjectTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
                if (newProjectTask != null)
                {
                    (newProjectTask.SpentHours, newProjectTask.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(newProjectTask.SpentHours, newProjectTask.SpentMinutes, spentHours, spentMinutes);// Add new time

                    
                    await _projectTaskService.UpdateProjectTaskAsync(newProjectTask);
                }

                //imp
                await _timeSheetsService.UpdateOrCreateActivityAsync(timeSheet, existingRow, activityName, prevSpentHours, spentHours, prevSpentMinutes, spentMinutes, taskId);

            }
            else
            {
                // Handle case where the row does not exist
                var newRow = new TimeSheet
                {
                    Id = id,
                    ProjectId = projectId,
                    TaskId = taskId,
                    //EstimatedHours = estimatedHours,
                    SpentHours = spentHours,
                    SpentMinutes = spentMinutes,
                    Billable = billable
                };

                await _timeSheetsService.InsertTimeSheetAsync(newRow);
            }

            // Update the time sheet
            await _timeSheetsService.UpdateTimeSheetAsync(timeSheet);
               
            var updatedTask = await _projectTaskService.GetProjectTasksByIdAsync(taskId);
            if (updatedTask != null)
            {
                updatedTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(updatedTask);
                await _projectTaskService.UpdateProjectTaskAsync(updatedTask);
            }
            // Update attendance based on timesheet

            await _employeeAttendanceService.UpdateEmployeeAttendanceBasedOnTimeSheetAsync(
                timeSheet.SpentDate, timeSheet.EmployeeId, timeSheet.SpentHours,timeSheet.SpentMinutes,
                timeSheet.SpentDate, timeSheet.EmployeeId);

            return Json(new { success = true, updatedTotalSpent = updatedTask.SpentHours });
        }

        public virtual async Task<IActionResult> CreateProjectTask(int employeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement))
                return AccessDeniedView();

            //prepare model
            var model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(new ProjectTaskModel(), null);
            ViewBag.RefreshPage = false;
            if(employeeId!=0)
            model.AssignedTo= employeeId;
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if(employee !=null)
            model.AssignedEmployee = employee.FirstName + " " + employee.LastName;

            model.IsSync = true;

            return View("/Areas/Admin/Views/Extension/TimeSheet/CreateProjectTask.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateProjectTask(ProjectTaskModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageLeaveManagement))
                return AccessDeniedView();

            //int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            //model.AssignedTo = selectedEmployeeId;
            if (!string.IsNullOrWhiteSpace(model.EstimationTimeHHMM) &&
                  System.Text.RegularExpressions.Regex.IsMatch(model.EstimationTimeHHMM, @"^([0-9]{1,2}):([0-5][0-9])$"))
            {
                model.EstimatedTime = await _timeSheetsService.ConvertHHMMToDecimal(model.EstimationTimeHHMM);
            }
            else
            {
                ViewBag.RefreshPage = false;

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.estimationtime.error.invalidhhmm"));

                model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Timesheet/CreateProjectTask.cshtml", model);
            }

          

            if (ModelState.IsValid)
            {

                var existingTask = await _projectTaskService.GetProjectTaskByTitleAndProjectAsync(model.TaskTitle, model.ProjectId);
                if (existingTask != null)
                {
                    ViewBag.RefreshPage = false;

                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.ProjectTask.error.TaskAlreadyExist"));
                                                                                                
                    model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
                    //if we got this far, something failed, redisplay form

                    return View("/Areas/Admin/Views/Extension/Timesheet/CreateProjectTask.cshtml", model);
                }

                var projectTask = model.ToEntity<ProjectTask>();

                projectTask.StatusId = await _workflowStatusService.GetDefaultStatusIdByWorkflowId(projectTask.ProcessWorkflowId);
                projectTask.CreatedOnUtc = await _dateTimeHelper.GetUTCAsync();
              

                await _projectTaskService.InsertProjectTaskAsync(projectTask);

                var customer = await _workContext.GetCurrentCustomerAsync();
                int currEmployeeId = 0;

                if (!await _customerService.IsRegisteredAsync(customer))
                    return Challenge();
                if (customer != null)
                {
                    var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                    if (currEmployee != null)
                        currEmployeeId = currEmployee.Id;
                }

                TaskComments taskComment = new TaskComments();

                taskComment.EmployeeId = currEmployeeId;
                taskComment.StatusId = projectTask.StatusId;
                taskComment.Description = model.StatusChangeComment;
                taskComment.TaskId = projectTask.Id;
                taskComment.CreatedOn = await _dateTimeHelper.GetUTCAsync();

                await _taskCommentsService.InsertTaskCommentsAsync(taskComment);
                await _workflowMessageService.SendEmployeeMentionMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, taskComment.EmployeeId, taskComment.TaskId, taskComment.Description);

                ViewBag.RefreshPage = true;

                return View("/Areas/Admin/Views/Extension/Timesheet/CreateProjectTask.cshtml", model);
            }
            //prepare model
            ViewBag.RefreshPage = false;

            model = await _projectTaskModelFactory.PrepareProjectTaskModelAsync(model, null, true);
            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Timesheet/CreateProjectTask.cshtml", model);
        }


        [HttpPost]
        public async Task<IActionResult> SaveNewChanges(List<TimeSheetRowModel> timeSheetEntries)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageTimeSheet))
                return AccessDeniedView();

            var savedEntries = new List<object>();

            foreach (var entry in timeSheetEntries)
            {
                if (!ModelState.IsValid)
                    continue;

                var activityId = 0;
                Activity activity = null;
                (entry.SpentHours, entry.SpentMinutes) = await _timeSheetsService.ConvertSpentTimeAsync(entry.SpentTime);
                if (!string.IsNullOrWhiteSpace(entry.ActivityName))
                {
                    // Check if the activity already exists
                    var existingActivities = await _activityService.GetAllActivitiesByActivityNameTaskIdAsync(entry.ActivityName, entry.TaskId);
                    if (existingActivities.Any())
                    {
                        // Update existing activity
                        activity = existingActivities.First();
                        (activity.SpentHours, activity.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(activity.SpentHours, activity.SpentMinutes, entry.SpentHours, entry.SpentMinutes);
                        activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                        await _activityService.UpdateActivityAsync(activity);
                        activityId = activity.Id;
                    }
                    else
                    {
                        // Create new activity
                        activity = new Activity
                        {
                            ActivityName = entry.ActivityName,
                            EmployeeId = entry.EmployeeId,
                            TaskId = entry.TaskId,
                            SpentHours = entry.SpentHours,
                            SpentMinutes = entry.SpentMinutes,
                            CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                            UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                        };
                        await _activityService.InsertActivityAsync(activity);
                        activityId = activity.Id;
                    }
                }
                // Create new time sheet entry
                var timeSheet = new TimeSheet
                {
                    EmployeeId = entry.EmployeeId,
                    SpentDate = entry.SpentDate,
                    ProjectId = entry.ProjectId,
                    TaskId = entry.TaskId,
                    //EstimatedHours = entry.EstimatedHours,
                    SpentHours = entry.SpentHours,
                    SpentMinutes = entry.SpentMinutes,
                    Billable = entry.Billable,
                    ActivityId = activityId, // Link activity to the timesheet
                    CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                    UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
                };

                await _timeSheetsService.InsertTimeSheetAsync(timeSheet);

                // Update the SpentHours in the ProjectTask table
                var projectTask = await _projectTaskService.GetProjectTasksByIdAsync(entry.TaskId);
                if (projectTask != null)
                {

                    (projectTask.SpentHours, projectTask.SpentMinutes) = await _timeSheetsService.AddSpentTimeAsync(projectTask.SpentHours, projectTask.SpentMinutes, entry.SpentHours, entry.SpentMinutes);


                    projectTask.DeliveryOnTime = await _timeSheetsService.IsTaskDeliveredOnTimeAsync(projectTask);

                    await _projectTaskService.UpdateProjectTaskAsync(projectTask);
                }

                // Prepare data for response
                savedEntries.Add(new
                {
                    newRowId = timeSheet.Id,
                    entry.ProjectId,
                    entry.TaskId,
                    entry.SpentHours,
                    entry.SpentMinutes,
                    entry.Billable,
                    entry.EstimatedHours,
                    entry.TotalSpent,
                    entry.EmployeeId,
                    ActivityId = activityId 
                });
            }

            return Json(new { success = true, message = "Changes saved successfully", savedEntries });
        }


        [HttpGet]
        public virtual async Task<IActionResult> SearchActivities(string searchText, int taskId)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Json(new List<object>());

            // Fetch activities asynchronously
            var activitiesPaged = await _activityService.GetAllActivitiesAsync(
                searchText, 0, taskId, 0, int.MaxValue, false, null);

            // Transform results for the dropdown
            var activities = activitiesPaged.Select(a => new { Value = a.Id, Text = a.ActivityName }).ToList();

            return Json(activities);
        }

        [HttpPost]
        public async Task<IActionResult> CreateActivityIfNotExists(string activityName, int employeeId, int taskId, int spentHours, int timesheetId)
        {
            if (string.IsNullOrWhiteSpace(activityName))
                throw new ArgumentException("Activity name cannot be null or empty.", nameof(activityName));

            // Check if the activity already exists
            var existingActivity = await _activityService.GetAllActivitiesByActivityNameTaskIdAsync(activityName,  taskId);

            // Fetch the timesheet
            var timesheet = await _timeSheetsService.GetTimeSheetByIdAsync(timesheetId);

            if (existingActivity.Count != 0)
            {
                var activity = existingActivity.First();
                var activityId = activity.Id; // Get the ID of the existing activity

                if (timesheet != null)
                {
                    // Check if the activity in the timesheet matches the found activity
                    if (timesheet.ActivityId != activityId)
                    {
                        // Timesheet points to a different activity - update the timesheet reference
                        timesheet.ActivityId = activityId;

                        //  handle logic for removing spent hours from the old activity
                        var oldActivity = await _activityService.GetActivityByIdAsync(timesheet.ActivityId);
                        if (oldActivity != null)
                        {
                            oldActivity.SpentHours -= timesheet.SpentHours; // Undo the previous allocation
                            oldActivity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                            await _activityService.UpdateActivityAsync(oldActivity);
                        }
                    }

                    // Calculate the difference in spent hours
                    var diff = spentHours - timesheet.SpentHours;

                    if (diff != 0)
                    {
                        // Update the new activity's spent hours
                        activity.SpentHours += diff;
                        activity.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();
                        await _activityService.UpdateActivityAsync(activity);

                        // Update the timesheet's spent hours
                        timesheet.SpentHours = spentHours;
                        await _timeSheetsService.UpdateTimeSheetAsync(timesheet);
                    }
                }

                return Json(new { success = true, activityId }); // Return the ID
            }

            // Create a new activity
            var newActivity = new Activity
            {
                ActivityName = activityName,
                EmployeeId = employeeId,
                TaskId = taskId,
                SpentHours = spentHours,
                CreateOnUtc = await _dateTimeHelper.GetUTCAsync(),
                UpdateOnUtc = await _dateTimeHelper.GetUTCAsync()
            };

            await _activityService.InsertActivityAsync(newActivity);

            // Update the timesheet with the new activity ID
            if (timesheet != null)
            {
                timesheet.ActivityId = newActivity.Id;
                timesheet.SpentHours = spentHours;
                await _timeSheetsService.UpdateTimeSheetAsync(timesheet);
            }

            return Json(new { success = true, activityId = newActivity.Id });
        }


    }

}


