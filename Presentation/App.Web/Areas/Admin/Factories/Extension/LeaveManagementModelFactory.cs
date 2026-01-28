using App.Core.Caching;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.ProjectTasks;
using App.Core.Domain.Extension.TimeSheets;
using App.Core.Domain.Leaves;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Leaves;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the leaveManagement model factory implementation
    /// </summary>
    public partial class LeaveManagementModelFactory : ILeaveManagementModelFactory
    {
        #region Fields

        private readonly ILeaveManagementService _leaveManagementService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILeaveTypeService _leaveTypeService;
        private readonly IEmployeeService _employeeService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;

        #endregion

        #region Ctor

        public LeaveManagementModelFactory(ILeaveManagementService leaveManagementService,
            IDateTimeHelper dateTimeHelper,
            ILeaveTypeService leaveTypeService,
            IEmployeeService employeeService,
            IBaseAdminModelFactory baseAdminModelFactory,
            IStaticCacheManager staticCacheManager,
            ILeaveTransactionLogService leaveTransactionLogService
            )
        {
            _leaveManagementService = leaveManagementService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTypeService = leaveTypeService;
            _employeeService = employeeService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _staticCacheManager = staticCacheManager;
            _leaveTransactionLogService = leaveTransactionLogService;

        }

        #endregion

        #region Utilities

        public virtual async Task PrepareLeaveTypeListAsync(LeaveManagementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.Leave.Add(new SelectListItem
            {
                Text = "Select",
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

        public virtual async Task PrepareLeaveTypeListAsync(LeaveManagementSearchModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableLeaveType.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var leaveTypeName = "";
            var leaves = await _leaveTypeService.GetAllLeaveTypeAsync(leaveTypeName);
            foreach (var p in leaves)
            {
                model.AvailableLeaveType.Add(new SelectListItem
                {
                    Text = p.Type,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareEmployeeListAsync(LeaveManagementModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Employee.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.Employee.Add(new SelectListItem
                {
                    Text = p.FirstName+" "+p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }

        #endregion
        #region Methods

        public virtual async Task<LeaveManagementSearchModel> PrepareLeaveManagementSearchModelAsync(LeaveManagementSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            await PrepareLeaveTypeListAsync(searchModel);
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

            var periods = await SearchPeriodEnum.CustomRange.ToSelectListAsync();

            searchModel.PeriodList = periods.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = searchModel.SearchPeriodId.ToString() == store.Value
            }).ToList();
            searchModel.AvailableStatus = statusList;

            searchModel.StatusId = (int)StatusEnum.Pending;

            return searchModel;
        }

        public virtual async Task<LeaveManagementListModel> PrepareLeaveManagementListModelAsync(LeaveManagementSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));
            var startDateValue = !searchModel.From.HasValue ? null
            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.From.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync());
            var endDateValue = !searchModel.To.HasValue ? null
                : (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.To.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync()).AddDays(1);
            //get leaveType

            
            var leaveManagement = await _leaveManagementService.GetAllLeaveManagementAsync(employeeName: searchModel.EmployeeName, searchModel.From,
                searchModel.To,
                showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,leaveType:searchModel.SearchLeaveTypeId, status:searchModel.StatusId );

            //if(!leaveManagement.Any(l=>l.StatusId == 2))
            //{
            //    searchModel.StatusId = (int)StatusEnum.All
            //}
            //prepare grid model
            var model = await new LeaveManagementListModel().PrepareToGridAsync(searchModel, leaveManagement, () =>
            {
                return leaveManagement.SelectAwait(async leaveManagements =>
                {
                    if (leaveManagements.EmployeeId == 0)
                    {
                        return null;
                    }

                    var selectedAvailableDaysOption = leaveManagements.StatusId;
                    var leaveManagementModel = leaveManagements.ToModel<LeaveManagementModel>();
                    leaveManagementModel.DateFrom = leaveManagements.From.ToString("MM/dd/yyyy");
                    leaveManagementModel.DateTo = leaveManagements.To.ToString("MM/dd/yyyy");
                    Leave leaves = new Leave();
                    leaves = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveManagements.LeaveTypeId);
                    if (leaves == null)
                        return null;
                    leaveManagementModel.LeaveTypeId = leaves.Id;
                    leaveManagementModel.LeaveType = leaves.Type;
                    Employee emp = new Employee();
                    emp = await _employeeService.GetEmployeeByIdAsync(leaveManagementModel.EmployeeId);
                    if (emp == null)
                        return null;
                    
                        leaveManagementModel.EmployeeName = emp.FirstName + " " + emp.LastName;
                    
                    leaveManagementModel.CreatedOnUTC = await _dateTimeHelper.ConvertToUserTimeAsync(leaveManagements.CreatedOnUTC, DateTimeKind.Utc);
                    leaveManagementModel.Status = ((StatusEnum)selectedAvailableDaysOption).ToString();

                    var leaveLog = await _leaveTransactionLogService.GetLeaveBalanceByLog(emp.Id, leaveManagementModel.LeaveTypeId);
                    

                    if(leaveManagementModel.Status == null)
                        return null;
            

                    return leaveManagementModel;
                }).Where(x => x != null);
            });

            await PrepareLeaveTypeListAsync(searchModel);
            //prepare grid model
            return model;
        }
        public virtual async Task<LeaveManagementModel> PrepareLeaveManagementModelAsync(LeaveManagementModel model, LeaveManagement leaveManagement, bool excludeProperties = false)
        {

            var questiontype = await StatusEnum.Pending.ToSelectListAsync();
            var selectOption = new SelectListItem { Value = "", Text = "Select" };

            // Convert SelectList to List<SelectListItem>
            var questionTypeList = questiontype.ToList();

            // Insert the select option at the beginning of the list
            questionTypeList.Insert(0, selectOption);

            // Create a new SelectList from the modified list
            var questionTypeSelectList = new SelectList(questionTypeList, "Value", "Text");


            if (leaveManagement != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = leaveManagement.ToModel<LeaveManagementModel>();

                    var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                    if (employee != null)
                    {
                        model.EmployeeName = employee.FirstName + " " + employee.LastName;
                    }
                }

                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                   
                    model.EmployeeName = emp.FirstName+" "+emp.LastName;
                }

                if (leaveManagement.SendMailIds != null && leaveManagement.SendMailIds != "")
                    model.SelectedEmployeeIdForEmail = leaveManagement.SendMailIds
                            .Split(',')                         // Split by comma
                            .Select(int.Parse)                  // Convert each item to int
                            .ToList();
            }
            
            model.ApprovedStatus = questionTypeSelectList.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StatusId.ToString() == store.Value
            }).ToList();

            var Emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (Emp != null)
            {
                model.EmployeeName = Emp.FirstName + " " + Emp.LastName;
            }


            await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableEmployees, false);
            foreach (var employeeItem in model.AvailableEmployees)
            {
                employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
                    && model.SelectedEmployeeId.Contains(employeeId);
         
   }
          

            await PrepareLeaveTypeListAsync(model);
      
            return model;
        }
        #endregion
    }
}
