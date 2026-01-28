using App.Core.Domain.Employees;
using App.Data.Extensions;
using App.Services.Employees;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class EducationModelFactory : IEducationModelFactory
    {
        #region Fields

        private readonly IEducationService _educationService;
        private readonly IEmployeeService _employeeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        #endregion

        #region Ctor

        public EducationModelFactory(IEducationService educationService,
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper,
            IBaseAdminModelFactory baseAdminModelFactory
            )
        {
            _educationService = educationService;
            _employeeService = employeeService;
            _dateTimeHelper = dateTimeHelper;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion
        #region Utitlitis
        public virtual async Task PrepareEmployeeListAsync(EducationModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Employess.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var employeeName = "";
            var employees = await _employeeService.GetAllEmployeeNameAsync(employeeName);
            foreach (var p in employees)
            {
                model.Employess.Add(new SelectListItem
                {
                    Text = p.FirstName + " " + p.LastName,
                    Value = p.Id.ToString()
                });
            }
        }
        #endregion
        #region Methods

        public virtual async Task<EducationSearchModel> PrepareEducationSearchModelAsync(EducationSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }
        public virtual async Task<EducationListModel> PrepareEducationListModelAsync(EducationSearchModel searchModel, Employee employee)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get educations
            var educations = await _educationService.GetAllEducationsAsync(employeeId:searchModel.employeeId,showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new EducationListModel().PrepareToGridAsync(searchModel, educations, () =>
            {
                return educations.SelectAwait(async education =>
                {
                    //fill in model values from the entity
                    var educationModel = education.ToModel<EducationModel>();
                   
                    Employee emp = new Employee();
                    
                    emp = await _employeeService.GetEmployeeByIdAsync(educationModel.EmployeeID);
                    if (emp != null)
                        educationModel.EmployeeName = emp.FirstName + " " + emp.LastName;
                    
                    return educationModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<EducationModel> PrepareEducationModelAsync(EducationModel model, Education education, bool excludeProperties = false)
        {
            if (education != null)
            {
                if (model ==null)
                {
                    //fill in model values from the entity
                    model = education.ToModel<EducationModel>();
                    
                    
                        var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeID);
                        if (employee != null)
                        {
                            model.EmployeeName = employee.FirstName + " " + employee.LastName;
                        }
                    


                }

                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeID);

                if (model.SelectedEmployeeId == null)
                {
                    model.SelectedEmployeeId = new List<int>();
                }

                if (emp != null)
                {
                    model.SelectedEmployeeId.Add(emp.Id);
                }

            }
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
