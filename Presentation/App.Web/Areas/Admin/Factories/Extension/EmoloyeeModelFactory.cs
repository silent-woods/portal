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
using Microsoft.CodeAnalysis.Operations;
using App.Services.Localization;

namespace App.Web.Areas.Admin.Factories.Extension
{
    public partial class EmployeeModelFactory : IEmployeeModelFactory
    {
        #region Fields

        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public EmployeeModelFactory(
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper,
            IDepartmentService departmentService,
            IDesignationService designationService,
            ILocalizationService localizationService)
        {
            _employeeService = employeeService;
            _dateTimeHelper = dateTimeHelper;
            _departmentService = departmentService;
            _designationService = designationService;
            _localizationService = localizationService;
        }

        #endregion
        #region Utilities

        public virtual async Task PrepareDepartmentListAsync(EmployeeModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Departments.Add(new SelectListItem
            {
                Text = await _localizationService.GetResourceAsync("admin.extension.employee.fields.department.select"),
                Value = null
            });
            var department = await _departmentService.GetAllDepartmentByNameAsync();
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
                Text = await _localizationService.GetResourceAsync("admin.extension.employee.fields.designation.select"),
                Value = null
            });
            var designation = await _designationService.GetAllDesignationAsync();
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
        public virtual async Task<EmployeeSearchModel> PrepareEmployeeSearchModelAsync(EmployeeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<EmployeeListModel> PrepareEmployeeListModelAsync(EmployeeSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var employees = await _employeeService.GetAllEmployeesAsync(showHidden: true,
                employee: searchModel.SearchEmployeeName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, showInActive: true, showVendors: true);

            var model = await new EmployeeListModel().PrepareToGridAsync(searchModel, employees, () =>
            {
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
                    return employeeModel;
                });
            });

            return model;
        }
        public virtual async Task<EmployeeModel> PrepareEmployeeModelAsync(EmployeeModel model, Employee employee, bool excludeProperties = false)
        {
            var bloodGroup = await BloodGroupEnum.Select.ToSelectListAsync();
            var maritalStatus = await MaritalStatusEnum.Select.ToSelectListAsync();
            var empStatus = await EmployeeStatusEnum.Select.ToSelectListAsync();
            if (employee != null)
            {
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
