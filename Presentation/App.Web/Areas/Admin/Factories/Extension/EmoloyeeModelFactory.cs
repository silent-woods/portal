using System;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Framework.Models.Extensions;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Data.Extensions;
using App.Services.Employees;
using App.Web.Areas.Admin.Models.Employees;
using App.Core.Domain.Employees;
using App.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using App.Services.Departments;
using App.Services.Designations;
using App.Core.Domain.Designations;
using App.Core.Domain.Departments;

namespace App.Web.Areas.Admin.Factories.Extension
{
    /// <summary>
    /// Represents the employee model factory implementation
    /// </summary>
    public partial class EmployeeModelFactory : IEmployeeModelFactory
    {
        #region Fields

        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public EmployeeModelFactory(
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper,
            IDepartmentService departmentService,
            IDesignationService designationService)
        {
            _employeeService = employeeService;
            _dateTimeHelper = dateTimeHelper;
            _departmentService = departmentService;
            _designationService = designationService;
        }

        #endregion
        #region Utilities

        public virtual async Task PrepareDepartmentListAsync(EmployeeModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Departments.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var departmentName = "";
            var department = await _departmentService.GetAllDepartmentByNameAsync(departmentName);
            foreach (var p in department)
            {
                model.Departments.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareDesignationListAsync(EmployeeModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Designation.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var designationName = "";
            var designation = await _designationService.GetAllDesignationAsync(designationName);
            foreach (var p in designation)
            {
                model.Designation.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareManagerListAsync(EmployeeModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            var employeeName = "";
            var employee = await _employeeService.GetAllEmployeesAsync();
            foreach (var p in employee)
            {  if(model.Id == p.Id)
                {
                    continue;
                }
                model.AvailableManager.Add(new SelectListItem
                {
                    Text = p.FirstName+" "+p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }

        #endregion
        #region Methods

        /// <summary>
        /// Prepare employee search model
        /// </summary>
        /// <param name="searchModel">Employee search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee search model
        /// </returns>
        public virtual async Task<EmployeeSearchModel> PrepareEmployeeSearchModelAsync(EmployeeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged employee list model
        /// </summary>
        /// <param name="searchModel">Employee search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee list model
        /// </returns>
        public virtual async Task<EmployeeListModel> PrepareEmployeeListModelAsync(EmployeeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get employees
            var employees = await _employeeService.GetAllEmployeesAsync(showHidden: true,
                employee: searchModel.SearchEmployeeName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize,showInActive:true);

            //prepare grid model
            var model = await new EmployeeListModel().PrepareToGridAsync(searchModel, employees, () =>
            {
                //fill in model values from the entity
                return employees.SelectAwait(async employee =>
                {
                    
                    var selectedAvailableBloodGroupOption = employee.BloodGroupId;
                    var selectedAvailableMaritalsStatusOption = employee.MaritalStatusId;
                    var selectedAvailableEmployeeStatusOption = employee.EmployeeStatusId;
                   
                    var employeeModel = employee.ToModel<EmployeeModel>();
                    
                    employeeModel.BloodGroup = ((BloodGroupEnum)selectedAvailableBloodGroupOption).ToString();
                    employeeModel.MaritalStatus = ((MaritalStatusEnum)selectedAvailableMaritalsStatusOption).ToString();
                    employeeModel.EmployeeStatus = ((EmployeeStatusEnum)selectedAvailableEmployeeStatusOption).ToString();

                    Designation designation = await _designationService.GetDesignationByIdAsync(employeeModel.DesignationId);
                    if (designation != null)
                          employeeModel.DesignationName = designation.Name;
                    

                    Department department = await _departmentService.GetDepartmentByIdAsync(employeeModel.DepartmentId);
                   if(department != null)
                    employeeModel.DepartmentName=department.Name;


                    employeeModel.OfficialEmail = employee.OfficialEmail;
                    
                    //employeeModel.HolidayDate = employee.Date.ToString("MM-dd-yyyy");

                    //var employeeModel = new EmployeeModel();
                    //employeeModel.Id = employee.Id;
                    //employeeModel.Name = employee.Name;
                    //employeeModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(department.CreatedOnUtc, DateTimeKind.Utc);
                    //employeeModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(department.UpdatedOnUtc, DateTimeKind.Utc);

                    return employeeModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare employee model
        /// </summary>
        /// <param name="model">Employee model</param>
        /// <param name="employee">Employee</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the employee model
        /// </returns>
        public virtual async Task<EmployeeModel> PrepareEmployeeModelAsync(EmployeeModel model, Employee employee, bool excludeProperties = false)
        {
            var bloodGroup = await BloodGroupEnum.Select.ToSelectListAsync();
            var maritalStatus = await MaritalStatusEnum.Select.ToSelectListAsync();
            var empStatus = await EmployeeStatusEnum.Select.ToSelectListAsync();
            if (employee != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = employee.ToModel<EmployeeModel>();
                    var manager = await _employeeService.GetEmployeeByIdAsync(model.ManagerId);
                    if (manager != null)
                    {
                        model.ManagerName = manager.FirstName + " " + manager.LastName;
                    }
                }
                var selectedManager = await _employeeService.GetEmployeeByIdAsync(model.ManagerId);

                if (selectedManager != null)
                {
                    model.SelectedManagerId.Add(selectedManager.Id);
                }
            }
                model.BloodGroups = bloodGroup.Select(store => new SelectListItem
                {
                    Value = store.Value,
                    Text = store.Text,
                    Selected = model.BloodGroupId.ToString() == store.Value
                }).ToList();
                model.MaritalsStatus = maritalStatus.Select(store => new SelectListItem
                {
                    Value = store.Value,
                    Text = store.Text,
                    Selected = model.MaritalStatusId.ToString() == store.Value
                }).ToList();
                model.EmployeesStatus = empStatus.Select(store => new SelectListItem
                {
                    Value = store.Value,
                    Text = store.Text,
                    Selected = model.EmployeeStatusId.ToString() == store.Value
                }).ToList();
            await PrepareDepartmentListAsync(model);
            await PrepareDesignationListAsync(model);
            await PrepareManagerListAsync(model);
            return model;
        }

        #endregion
    }
}
