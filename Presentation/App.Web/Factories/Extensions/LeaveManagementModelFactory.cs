using App.Core;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Services;
using App.Services.Customers;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.WeeklyReports;
using App.Web.Models.Extensions.LeaveManagement;
using Azure.Storage.Blobs.Models;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace App.Web.Factories.Extensions
{
    public partial class LeaveManagementModelFactory : ILeaveManagementModelFactory
    {
        #region Fields

        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public LeaveManagementModelFactory(ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IWorkContext workContext,
            ICustomerService customerService)
        {
            _leaveManagementService = leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _workContext = workContext;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        public virtual async Task PrepareLeaveTypeListAsync(LeaveManagementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Leave.Add(new SelectListItem
            {
                Text = "All",
                Value = null
            });
            var leaveTypeName = "";
            var leaves = await _leaveTypeService.GetAllLeaveTypeAsync(leaveTypeName);
            foreach (var p in leaves)
            {
                model.Leave.Add(new SelectListItem
                {
                    Text = p.Type,
                    Value = p.Id.ToString()
                });
            }
        }


       

        #endregion

        #region Method
        public async Task<LeaveManagementSearchModel> PrepareLeaveManagementSearchModelAsync(LeaveManagementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var leaveTypes = await _leaveTypeService.GetAllLeaveTypeAsync("");
            var periods = await SearchPeriodEnum.Today.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();
            var selectList = new List<SelectListItem>
    {
        new SelectListItem
        {
            Text = "All",
            Value = "All" // Default value
        }
    };

            
            selectList.AddRange(leaveTypes.Select(lt => new SelectListItem
            {
                Text = lt.Type,
                Value = lt.Id.ToString()
            }));

            var statusList = Enum.GetValues(typeof(StatusEnum))
              .Cast<StatusEnum>()
              .Select(e => new SelectListItem
              {
                  Value = ((int)e).ToString(),
                  Text = e.ToString()
              }).ToList();


            statusList.Insert(0, new SelectListItem
            {
                Text = "All",
                Value = null
            });

            searchModel.AvailableStatusId = statusList;

            searchModel.AvailableLeaveTypes = selectList;

         
            

            return searchModel;
        }

        public async Task<LeaveManagementListModel> PrepareLeaveManagementListModelAsync(LeaveManagementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var currentEmployee = await _customerService.GetEmployeeByCustomerIdAsync(searchModel.CurrentCustomer);
            if (currentEmployee == null)
            {
               
                return new LeaveManagementListModel
                {
                    LeaveManagements = new List<LeaveManagementModel>()
                };
            }

            var leaveTypes = await _leaveTypeService.GetLeaveTypesAsync();
           
            // Retrieve leave management records for the current employee
            var leaveManagements = await _leaveManagementService.GetLeaveManagementsAsync(currentemp: currentEmployee.Id,
                leaveTypeId: searchModel.LeaveTypeId,
                statusId: searchModel.StatusId,
                fromDate: searchModel.From,
                toDate: searchModel.To); 

            var listModel = new LeaveManagementListModel
            {
                LeaveManagements = leaveManagements.Select(leaveManagement => new LeaveManagementModel
                {
                    Id = leaveManagement.Id,
                    LeaveTypeId = leaveManagement.LeaveTypeId,
                    LeaveType = leaveTypes.FirstOrDefault(lt => lt.Id == leaveManagement.LeaveTypeId)?.Type,
                    From = leaveManagement.From,
                    To = leaveManagement.To,
                    NoOfDays = leaveManagement.NoOfDays,
                    ReasonForLeave = leaveManagement.ReasonForLeave,
                    EmployeeName = currentEmployee.FirstName + " " + currentEmployee.LastName,
                    StatusId = leaveManagement.StatusId,
                    CreatedOnUTC = leaveManagement.CreatedOnUTC,
                    Status = leaveManagement.StatusId == (int)StatusEnum.Pending ? "Pending" : Enum.GetName(typeof(StatusEnum), leaveManagement.StatusId)

                }).ToList()
            };


            return listModel;
        }


        public virtual async Task<LeaveManagementModel> PrepareLeaveManagementModelAsync(LeaveManagementModel model, LeaveManagement leaveManagement)
        {
            if (leaveManagement != null)
            {
                if (model == null)
                {
                    model = leaveManagement.ToModel<LeaveManagementModel>();

                    model.LeaveTypeId = leaveManagement.LeaveTypeId;
                    model.From = leaveManagement.From;
                    model.To = leaveManagement.To;
                    model.NoOfDays = leaveManagement.NoOfDays;
                    model.ReasonForLeave = leaveManagement.ReasonForLeave;
                    model.CreatedOnUTC = await _dateTimeHelper.ConvertToUserTimeAsync(leaveManagement.CreatedOnUTC, DateTimeKind.Utc);
                }

                var currentUser = await _workContext.GetCurrentCustomerAsync();
                if (currentUser != null)
                {
                    var employee = await _customerService.GetEmployeeByCustomerIdAsync(currentUser.Id);
                    if (employee == null)
                    {
                        
                        var newEmployee = new Employee
                        {
                            FirstName = currentUser.FirstName,
                            LastName = currentUser.LastName,
                            PersonalEmail = currentUser.Email,
                            Gender = currentUser.Gender,
                            MobileNo = currentUser.Phone,

                        };
                        await _employeeService.InsertEmployeeAsync(newEmployee);
                    }

                   
                    model.EmployeeName = employee.FirstName + " " + employee.LastName;
                }
            }
            await PrepareLeaveTypeListAsync(model);

            return model;
        }

        #endregion
    }
}
