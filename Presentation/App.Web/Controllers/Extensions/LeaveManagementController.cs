using App.Core;
using App.Core.Domain.Customers;
using App.Core.Domain.Designations;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Leaves;
using App.Core.Domain.Leaves;
using App.Core.Domain.Security;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Services.Localization;
using App.Services.Messages;
using App.Services.ProjectEmployeeMappings;
using App.Services.Security;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Factories.Extensions;
using App.Web.Models.Boards;
using App.Web.Models.Extensions.LeaveManagement;
using Azure.Storage.Blobs.Models;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.ExtendedProperties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.VisualBasic;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace App.Web.Controllers.Extensions
{
    public partial class LeaveManagementController : BasePublicController
    {
        //public IActionResult Index()
        //{
        //    return View();
        //}

        #region Fields
        private readonly IPermissionService _permissionService;
        private readonly ILeaveManagementModelFactory _leaveManagementModelFactory;
        private readonly ILeaveManagementService _leaveManagementService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IEmployeeService _employeeService;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly LeaveSettings _leaveSettings;
        private readonly IProjectEmployeeMappingService _projectEmployeeMappingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        #endregion

        #region Ctor
        public LeaveManagementController(IPermissionService permissionService,
            ILeaveManagementModelFactory leaveManagementModelFactory,
            ILeaveManagementService leaveManagementService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            ICustomerService customerService,
            IEmployeeService employeeService,
            ILeaveTypeService leaveTypeService,
            IWorkflowMessageService workflowMessageService,
            ILeaveTransactionLogService leaveTransactionLogService,
            LeaveSettings leaveSettings,
            IProjectEmployeeMappingService projectEmployeeMappingService,
            IDateTimeHelper dateTimeHelper
            )
        {
            _permissionService = permissionService;
            _leaveManagementModelFactory = leaveManagementModelFactory;
            _leaveManagementService = leaveManagementService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _customerService = customerService;
            _employeeService = employeeService;
            _leaveTypeService = leaveTypeService;
            _workflowMessageService = workflowMessageService;
            _leaveTransactionLogService = leaveTransactionLogService;
            _leaveSettings = leaveSettings;
            _projectEmployeeMappingService = projectEmployeeMappingService;
            _dateTimeHelper = dateTimeHelper;
        }
        #endregion

        #region Method
        public virtual async Task<IActionResult> LeaveCreate()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();


            //var model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(new LeaveManagementModel(), null);
            var model = new LeaveManagementModel();


            model.From = DateTime.Today;
            model.To = DateTime.Today;

            var currentCustomer = await _customerService.GetCustomerByIdAsync(customer.Id);
            if (currentCustomer != null)
            {
                model.EmployeeName = currentCustomer.FirstName + " " + currentCustomer.LastName;
            }

            int currEmployeeId = 0;
            
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();
            if (customer != null)
            {
                var currEmployee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
                if (currEmployee != null)
                    currEmployeeId = currEmployee.Id;
            }

            var leaveTypes =await  _leaveTypeService.GetAllLeaveTypeAsync("");

            model.Leave.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });

            // Add the actual leave types
            foreach (var lt in leaveTypes)
            {
                model.Leave.Add(new SelectListItem
                {
                    Text = lt.Type,
                    Value = lt.Id.ToString()
                });
            }

            var employee = await _employeeService.GetAllEmployeeNameAsync("");
            foreach (var p in employee)
            {
                model.AvailableEmployees.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }

           


            var preSelectedEmployeeIds = new HashSet<int>(); // Use HashSet to prevent duplicates

            // Pre-select Employees based on conditions
            if (_leaveSettings.SendEmailToAllProjectManager)
            {
                var projectManagers = await _projectEmployeeMappingService.GetProjectManagersByEmployeeIdAsync(currEmployeeId);

                // Add IDs directly to HashSet (duplicates are ignored automatically)
                foreach (var managerId in projectManagers)
                {
                    if(managerId != currEmployeeId)
                    preSelectedEmployeeIds.Add(managerId);
                }
            }

            if (_leaveSettings.SendEmailToAllProjectLeaders)
            {
                var projectLeaders = await _projectEmployeeMappingService.GetProjectLeadersByEmployeeIdAsync(currEmployeeId);
                foreach (var projectLeader in projectLeaders)
                {
                    // Add IDs directly to HashSet (duplicates are ignored automatically)
                    foreach (var leaderId in projectLeaders)
                    {
                        if (leaderId != currEmployeeId)
                            preSelectedEmployeeIds.Add(leaderId);
                    }
                }
            }

            if (_leaveSettings.SendEmailToEmployeeManager)
            {
                var employeeManager = await _employeeService.GetEmployeeByIdAsync(currEmployeeId);
                if (employeeManager != null)
                {
                    if (employeeManager.Id != currEmployeeId)
                        preSelectedEmployeeIds.Add(employeeManager.Id);
                }
            }

            model.SelectedEmployeeId = preSelectedEmployeeIds.ToList();


            return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model);
        }   

        [HttpPost]
        public virtual async Task<IActionResult> LeaveCreate(LeaveManagementModel model)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
                return Challenge();

            if (ModelState.IsValid)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

                var leaveManagement = new LeaveManagement
                {
                    LeaveTypeId = model.LeaveTypeId,
                    From = model.From.Value,
                    To = model.To.Value,
                    NoOfDays = model.NoOfDays,
                    ReasonForLeave = model.ReasonForLeave,
                    CreatedOnUTC = await _dateTimeHelper.GetUTCAsync(),
                    EmployeeId = employee.Id ,
                    StatusId= 1
                };

                if (model.From < DateTime.Today)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.leavemanagement.error.fromdatetodayorfuture"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null);
                    return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model);
                }

                if (model.To < DateTime.Today)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("admin.catalog.leavemanagement.error.ToDatetodayorfuture"));
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null);
                    return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model);
                }

                if (leaveManagement.From > leaveManagement.To)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Error.ToGreaterThenFrom"));
                
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null);

                    return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model);

                }

                var diffInDays = (decimal)((leaveManagement.To - leaveManagement.From).TotalDays + 1);

                if (model.NoOfDays > diffInDays)
                {
                    _notificationService.ErrorNotification("The number of days cannot be greater than the difference between the selected From and To dates, and must be exactly 0.5 less than the difference.");
                    model = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(model, null);
                    return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model);
                }
                await _leaveManagementService.InsertLeaveManagementAsync(leaveManagement);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Added"));
                //ModelState.Clear();

                IList<int> selectedEmployeeIdForEmail = new List<int>();
                if (leaveManagement.SendMailIds != null && leaveManagement.SendMailIds != "")
                    selectedEmployeeIdForEmail = leaveManagement.SendMailIds
                            .Split(',')                         // Split by comma
                            .Select(int.Parse)                  // Convert each item to int
                            .ToList();

                await _workflowMessageService.SendLeaveRequestMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
         employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id, leaveManagement.Id, selectedEmployeeIdForEmail);

                var model1 = await _leaveManagementModelFactory.PrepareLeaveManagementModelAsync(new LeaveManagementModel(), null);
               
                model1.From = DateTime.Today;
                model1.To = DateTime.Today;
                //return Json(new { success = true });
                return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model1);

            }

            return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveCreate.cshtml", model);
         
        }

        [HttpPost]
        public virtual async Task<IActionResult> InsertLeave(int leaveTypeId,
     DateTime? from,
     DateTime? to,
     decimal? NoOfDays,
     string ReasonForLeave,
     string SelectedEmployeeIds)
        {
            // Parameter validation at the beginning
            if (leaveTypeId <= 0 || from == null || to == null || NoOfDays == null || string.IsNullOrWhiteSpace(ReasonForLeave))
            {
                return Json(new { success = false, message = "All parameters are required." });
            }
            if (from.Value.Month != to.Value.Month || from.Value.Year != to.Value.Year)
            {
                return Json(new { success = false, message = "Leave request must start and end within the same month." });
            }
            if (NoOfDays <=0)
                return Json(new { success = false, message = "No Of Days Must Be Greater Then 0" });

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsRegisteredAsync(customer))
            {
                return Json(new { success = false, message = "User is not registered." });
            }

            // Initialize leave model
            LeaveManagementModel model = new LeaveManagementModel()
            {
                LeaveTypeId = leaveTypeId,
                From = from,
                To = to,
                NoOfDays = NoOfDays.Value,
                ReasonForLeave = ReasonForLeave,
                SendMailIds= SelectedEmployeeIds
            };

            if (ModelState.IsValid)
            {
                var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

                // New leave entity
                var leaveManagement = new LeaveManagement
                {
                    LeaveTypeId = model.LeaveTypeId,
                    From = model.From.Value,
                    To = model.To.Value,
                    NoOfDays = model.NoOfDays,
                    ReasonForLeave = model.ReasonForLeave,
                    CreatedOnUTC =await _dateTimeHelper.GetUTCAsync(),
                    EmployeeId = employee.Id,
                    SendMailIds = model.SendMailIds,
                    StatusId = 1 // Assuming 1 is 'Pending' status
                };

                // 1. Handle Invalid/Default DateTime (01/01/0001)
                if (from == DateTime.MinValue || to == DateTime.MinValue)
                {
                    return Json(new { success = false, message = "Invalid dates provided." });
                }

               
                // 4. Validate if From date is greater than To date
                if (leaveManagement.From > leaveManagement.To)
                {
                    return Json(new { success = false, message = "To date cannot be earlier than From date." });
                }

                // 5. Calculate difference in days between From and To dates
                
                var diffInDays = await _leaveManagementService.GetDifferenceByFromToAsync(from.Value, to.Value);

                // 6. Validate NoOfDays is not greater than calculated days and supports half-days

                if (model.NoOfDays != diffInDays - 0.5m && model.NoOfDays != diffInDays)
                {
                   
                    return Json(new { success = false, message = "No of Days must be equal to or 0.5 less than the date range." });

                }

                if (await _leaveManagementService.IsLeaveAlreadyTaken(employee.Id, model.From.Value, model.To.Value))
                {
                    return Json(new { success = false, message = "A leave request already exists for this date range." });
                }
                // Insert the leave record
                try
                {
                    await _leaveManagementService.InsertLeaveManagementAsync(leaveManagement);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.leaveManagement.Added"));


                    IList<int> selectedEmployeeIdForEmail = new List<int>();
                    if (leaveManagement.SendMailIds != null && leaveManagement.SendMailIds != "")
                        selectedEmployeeIdForEmail = leaveManagement.SendMailIds
                                .Split(',')                         // Split by comma
                                .Select(int.Parse)                  // Convert each item to int
                                .ToList();

                    // Send notification or email
                    await _workflowMessageService.SendLeaveRequestMessageAsync(
                        (await _workContext.GetWorkingLanguageAsync()).Id,
                        employee.OfficialEmail.Trim(),
                        employee.FirstName + " " + employee.LastName,
                        employee.Id,
                        leaveManagement.Id,
                        selectedEmployeeIdForEmail
                    );
                    //decimal balanceChange = 0;
                    //await _leaveTransactionLogService.AddLeaveTransactionLogAsync(leaveManagement.EmployeeId, leaveManagement.Id, leaveManagement.StatusId, leaveManagement.LeaveTypeId, leaveManagement.NoOfDays, balanceChange, await _localizationService.GetResourceAsync("Admin.LeaveManagement.LeaveTransactionLogComment.LeaveRequestArrived"));

                    return Json(new { success = true, message = "Leave inserted successfully." });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"An error occurred while inserting leave: {ex.Message}" });
                }
            }

            return Json(new { success = false, message = "Model validation failed." });
        }


  

        public async Task<IActionResult> LeaveBalance()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

            // Get all leave types
            var leaveTypes = await _leaveTypeService.GetLeaveTypesAsync();

            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            if (_leaveSettings.LeaveTestDate != DateTime.MinValue)
                currTime = _leaveSettings.LeaveTestDate;

            // Initialize a list to store leave balances
            var leaveBalances = new List<LeaveTypeModel>();

            // Loop through each leave type
            foreach (var leaveType in leaveTypes)
            {
                // Get approved taken leaves for the current year and employee
                var approvedTakenLeaves = await _leaveManagementService.GetApprovedTakenLeavesForCurrentYearAndEmployeeAsync(employee.Id, leaveType.Id);

                // Calculate total taken leaves
                var totalTakenLeaves = approvedTakenLeaves.Sum(l => l.NoOfDays);

                decimal balance = 0;
                decimal remainingLeaves = 0;
                var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(employee.Id, leaveType.Id);
                if (leaveLog != null)
                    remainingLeaves = await _leaveTransactionLogService.GetAddedLeaveBalanceForCurrentMonthForReport(employee.Id, leaveType.Id, currTime.Month, currTime.Year);


                //var remainingLeaves = balance;

                //// Calculate remaining leaves
                ////var remainingLeaves = leaveType.Total_Allowed - totalTakenLeaves;
                //var totalAllowed  = totalTakenLeaves + remainingLeaves;
                // Create a leave balance model
                var leaveBalanceModel = new LeaveTypeModel
                {
                    LeaveTypeName = leaveType.Type,
                    
                    TakenLeaves = totalTakenLeaves,
                    RemainingLeaves = remainingLeaves
                };

                // Add the leave balance model to the list
                leaveBalances.Add(leaveBalanceModel);
            }

            // Return the view with the leave balances
            //return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveBalance.cshtml", leaveBalances);
            // Get the last updated balance time
            string lastUpdateTime = _leaveSettings.LastUpdateBalance ?? "N/A";

            return Json(new { leaveBalances, lastUpdateTime });
        }


        [HttpGet]
        public async Task<IActionResult> SearchLeave(LeaveManagementSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.PublicStoreLeaveManagement, PermissionAction.View))
                return Challenge();

            var currentcustomer = await _workContext.GetCurrentCustomerAsync();

            searchModel.CurrentCustomer = currentcustomer.Id;

            searchModel = await _leaveManagementModelFactory.PrepareLeaveManagementSearchModelAsync(searchModel);
            var listModel = await _leaveManagementModelFactory.PrepareLeaveManagementListModelAsync(searchModel);
            searchModel.LeaveManagements = listModel.LeaveManagements;
            searchModel.SearchPeriodId =9;
            return View("/Themes/DefaultClean/Views/Extension/LeaveManagement/LeaveHistory.cshtml", searchModel);
        }

        public async Task<IActionResult> SearchLeaveList(int leaveTypeId,int statusId, DateTime from , DateTime to)
        {
            var currentcustomer = await _workContext.GetCurrentCustomerAsync();
            LeaveManagementSearchModel searchModel = new LeaveManagementSearchModel();

            searchModel.LeaveTypeId = leaveTypeId;
            // Check for default DateTime value and set to null if applicable
            searchModel.From = from == DateTime.MinValue ? (DateTime?)null : from;
            searchModel.To = to == DateTime.MinValue ? (DateTime?)null : to;
            searchModel.StatusId = statusId;

            searchModel.CurrentCustomer = currentcustomer.Id;

            searchModel = await _leaveManagementModelFactory.PrepareLeaveManagementSearchModelAsync(searchModel);
            var listModel = await _leaveManagementModelFactory.PrepareLeaveManagementListModelAsync(searchModel);
            searchModel.LeaveManagements = listModel.LeaveManagements;
            var leaveList = listModel.LeaveManagements
                .OrderByDescending(l => l.CreatedOnUTC) 
                .ToList();


            return Json(leaveList);
        }


        [HttpGet]
        public async Task<JsonResult> CalculateNoOfDays(DateTime fromDate, DateTime toDate)
        {
            var diffInDays = await _leaveManagementService.GetDifferenceByFromToAsync(fromDate, toDate);
          
            return Json(diffInDays);
        }

        #endregion

    }
}
