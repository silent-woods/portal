using App.Core.Domain.Leaves;
using App.Data;
using App.Services.Leaves;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Leavetypes;
using App.Web.Framework.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Framework.Models.Extensions;
using App.Data.Extensions;
using App.Web.Areas.Admin.Models.LeaveManagement;
using App.Services.Employees;
using App.Web.Models.Extensions.LeaveManagement;
using App.Core.Domain.PerformanceMeasurements;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Services;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using System.Globalization;


namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the leaveType model factory implementation
    /// </summary>
    public partial class LeaveTransactionLogModelFactory : ILeaveTransactionLogModelFactory
    {
        #region Fields

        private readonly ILeaveTypeService _leaveTypeService;
        private readonly ILeaveTransactionLogService _leaveTransactionLogService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmployeeService _employeeService;
      
        #endregion

        #region Ctor

        public LeaveTransactionLogModelFactory(ILeaveTypeService leaveTypeService,
            IDateTimeHelper dateTimeHelper, ILeaveTransactionLogService leaveTransactionLogService, IEmployeeService employeeService)
        {
            _leaveTypeService = leaveTypeService;
            _dateTimeHelper = dateTimeHelper;
            _leaveTransactionLogService = leaveTransactionLogService;
            _employeeService = employeeService;
        }

        #endregion
        #region Utilities

        public virtual async Task PrepareLeaveTypeListAsync(LeaveTransactionLogModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableLeaveTypes.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var leaveTypeName = "";
            var leaves = await _leaveTypeService.GetAllLeaveTypeAsync(leaveTypeName);
            foreach (var p in leaves)
            {
                model.AvailableLeaveTypes.Add(new SelectListItem
                {
                    Text = p.Type,
                    Value = p.Id.ToString()
                });
            }
        }

        public virtual async Task PrepareLeaveTypeListAsync(LeaveTransactionLogSearchModel model)
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

        public virtual async Task PrepareEmployeeListAsync(LeaveTransactionLogModel model)
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

        public virtual async Task PrepareYearrListAsync(LeaveTransactionLogModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Years.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });
            int startYear = DateTime.Now.Year - 110; // Start from 110 years ago
            int currentYear = DateTime.Now.Year +1;

            // Iterate over the range of years and add them to the model
            for (int year = currentYear; year >= startYear; year--)
            {
                model.Years.Add(new SelectListItem
                {
                    Text = year.ToString(),
                    Value = year.ToString()
                });
            }
        }

        #endregion
        #region Methods

        public virtual async Task<LeaveTransactionLogSearchModel> PrepareLeaveTransactionLogSearchModelAsync(LeaveTransactionLogSearchModel searchModel)
        {

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
            searchModel.AvailableStatus = statusList;
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<LeaveTransactionLogListModel> PrepareLeaveTransactionListModelAsync(LeaveTransactionLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get leaveTransactionLog
            var leaveTransactionLog = await _leaveTransactionLogService.GetAllLeaveTransactionLogAsync(searchModel.EmployeeName, searchModel.SearchLeaveId, searchModel.From, searchModel.To, searchModel.SearchLeaveTypeId,searchModel.StatusId,searchModel.SearchComment,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new LeaveTransactionLogListModel().PrepareToGridAsync(searchModel, leaveTransactionLog, () =>
            {
                return leaveTransactionLog.SelectAwait(async leaveTransactionLog =>
                {
                    //fill in model values from the entity
                    var leaveTransactionLogModel = new LeaveTransactionLogModel();
                    leaveTransactionLogModel.Id = leaveTransactionLog.Id;
                    leaveTransactionLogModel.ApprovedId = leaveTransactionLog.ApprovedId;
                   
                        leaveTransactionLogModel.LeaveId = leaveTransactionLog.LeaveId;
                    leaveTransactionLogModel.BalanceMonthYear = leaveTransactionLog.BalanceMonthYear;
                    if(leaveTransactionLog.BalanceMonthYear != null)
                    leaveTransactionLogModel.BalanceMonthYearString = leaveTransactionLog.BalanceMonthYear.ToString("MMMM yyyy");

                    leaveTransactionLogModel.StatusId = leaveTransactionLog.StatusId;
                  
                    leaveTransactionLogModel.BalanceChange = leaveTransactionLog.BalanceChange;
                    leaveTransactionLogModel.LeaveBalance = leaveTransactionLog.LeaveBalance;
                    leaveTransactionLogModel.EmployeeId= leaveTransactionLog.EmployeeId;
                    leaveTransactionLogModel.Comment = leaveTransactionLog.Comment;
                    if(leaveTransactionLog.ManualBalanceChange != 0)
                        leaveTransactionLogModel.IsEdited = true;

                    leaveTransactionLogModel.CreatedOnUTC = await _dateTimeHelper.ConvertToUserTimeAsync(leaveTransactionLog.CreatedOnUTC, DateTimeKind.Utc);

                    var employee = await _employeeService.GetEmployeeByIdAsync(leaveTransactionLog.EmployeeId);

                    if (employee != null)
                        leaveTransactionLogModel.EmployeeName = employee.FirstName + " " + employee.LastName;

                    var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(leaveTransactionLog.ApprovedId);

                    if (leaveType != null)
                        leaveTransactionLogModel.LeaveTypeName = leaveType.Type;

                    if (leaveTransactionLogModel.BalanceChange > 0)
                        leaveTransactionLogModel.BalanceChangeString = "+" + leaveTransactionLogModel.BalanceChange;
                    else
                        leaveTransactionLogModel.BalanceChangeString = leaveTransactionLogModel.BalanceChange.ToString();

                    var selectedStatusOption = leaveTransactionLogModel.StatusId;

                    if (leaveTransactionLogModel.StatusId != 0)
                        leaveTransactionLogModel.StatusName = ((StatusEnum)selectedStatusOption).ToString();
                    
                        

                    return leaveTransactionLogModel;
                });
            });
          
            //prepare grid model
            return model;
        }
        public virtual async Task<LeaveTransactionLogModel> PrepareLeaveTransactionLogModelAsync(LeaveTransactionLogModel model, LeaveTransactionLog leaveTransactionLog, bool excludeProperties = false)
        {
            if (leaveTransactionLog != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = leaveTransactionLog.ToModel<LeaveTransactionLogModel>();
                }
            }

           await  PrepareEmployeeListAsync(model);
           await  PrepareLeaveTypeListAsync(model);
            var month = await MonthEnum.Select.ToSelectListAsync();
            model.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.MonthId.ToString() == store.Value
            }).ToList();
            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            await PrepareYearrListAsync(model);
            model.Year = currTime.Year;
            model.MonthId = currTime.Month;
            var employee=  await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
            if (employee != null)
                model.EmployeeName = employee.FirstName + " " + employee.LastName;

            var leaveType = await _leaveTypeService.GetLeaveTypeByIdAsync(model.ApprovedId);

            if (leaveType != null)
                model.LeaveTypeName = leaveType.Type;


           

            return model;
        }

        public virtual async Task<LeaveTransactionLogModel> PrepareAddMonthlyLeave(LeaveTransactionLogModel model)
        {
            var month = await MonthEnum.Select.ToSelectListAsync();
            model.Months = month.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.MonthId.ToString() == store.Value
            }).ToList();
            var currTime = await _dateTimeHelper.GetIndianTimeAsync();
            await PrepareYearrListAsync(model);
            model.Year = currTime.Year;
            return model;
        }

        
        #endregion
    }
}
