using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.EmployeeAttendances;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using System.Text;
using App.Core.Domain.Projects;
using System.Collections.Generic;
using Humanizer;
using App.Services.TimeSheets;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the EmployeeAttendance model factory implementation
    /// </summary>
    public partial class EmployeeAttendanceModelFactory : IEmployeeAttendanceModelFactory
    {
        #region Fields

        private readonly IEmployeeAttendanceService _employeeAttendanceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ITimeSheetsService _timeSheetsService;
        #endregion

        #region Ctor

        public EmployeeAttendanceModelFactory(IEmployeeAttendanceService employeeAttendanceService,
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper, IBaseAdminModelFactory baseAdminModelFactory,
            ITimeSheetsService timeSheetsService
            )
        {
            _employeeAttendanceService = employeeAttendanceService;
            _dateTimeHelper = dateTimeHelper;
            _employeeService = employeeService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _timeSheetsService = timeSheetsService;
        }

        #endregion

        #region Utilities

        public virtual async Task PrepareEmployeeListAsync(EmployeeAttendanceModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Employee.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employee = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employee)
            {
                model.Employee.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }

        #endregion
        #region Methods

        public virtual async Task<EmployeeAttendanceSearchModel> PrepareEmployeeAttendanceSearchModelAsync(EmployeeAttendanceSearchModel searchModel)
        {
            var statusList = Enum.GetValues(typeof(StatusEnum))
                .Cast<StatusEnum>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();


            searchModel.AvailableStatus = statusList;

            searchModel.StatusId = (int)StatusEnum.Select;

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<EmployeeAttendanceListModel> PrepareEmployeeAttendanceListModelAsync(EmployeeAttendanceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get employeeAttendance
            var employeeAttendance = await _employeeAttendanceService.GetAllEmployeeAttendanceAsync(employeeName: searchModel.EmployeeName,
                from:searchModel.From, to:searchModel.To, statusId:searchModel.StatusId,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new EmployeeAttendanceListModel().PrepareToGridAsync(searchModel, employeeAttendance, () =>
            {
                return employeeAttendance.SelectAwait(async EmployeeAttendances =>
                {
                    var selectedAvailableDaysOption = EmployeeAttendances.StatusId;
                    //fill in model values from the entity
                    var employeeAttendanceModel = EmployeeAttendances.ToModel<EmployeeAttendanceModel>();
                    //employeeAttendanceModel.Dates = EmployeeAttendances.Date.ToString("MM/dd/yyyy");
                    employeeAttendanceModel.Times = await _timeSheetsService.ConvertSpentTimeAsync(EmployeeAttendances.SpentHours,EmployeeAttendances.SpentMinutes);
                    employeeAttendanceModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(EmployeeAttendances.CreateOnUtc, DateTimeKind.Utc);
                    employeeAttendanceModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(EmployeeAttendances.UpdateOnUtc, DateTimeKind.Utc);
                    employeeAttendanceModel.StatusName = ((StatusEnum)selectedAvailableDaysOption).ToString();

                    Employee employee = new Employee();
                    employee = await _employeeService.GetEmployeeByIdAsync(employeeAttendanceModel.EmployeeId);
                    if(employee !=null)
                    employeeAttendanceModel.EmployeeName = employee.FirstName + " " + employee.LastName;

                    return employeeAttendanceModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<EmployeeAttendanceModel> PrepareEmployeeAttendanceModelAsync(EmployeeAttendanceModel model, EmployeeAttendance employeeAttendance, bool excludeProperties = false)
        {
            var questiontype = await StatusEnum.Select.ToSelectListAsync();
            if (employeeAttendance != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = employeeAttendance.ToModel<EmployeeAttendanceModel>();
                   
                    
                        var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);
                        if (employee != null)
                        {
                            model.EmployeeName = employee.FirstName+" "+employee.LastName; 
                        }
                    
                }

               
                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeId);

               

                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }
                
            }

            model.Status = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.StatusId.ToString() == store.Value
            }).ToList();

            await _baseAdminModelFactory.PrepareEmployeeAsync(model.AvailableEmployees, false);
            foreach (var employeeItem in model.AvailableEmployees)
            {
                employeeItem.Selected = int.TryParse(employeeItem.Value, out var employeeId)
                    && model.SelectedEmployeeId.Contains(employeeId);
            }


           

         
            return model;
        }
        #endregion
    }
}
