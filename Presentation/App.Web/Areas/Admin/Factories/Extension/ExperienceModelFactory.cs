using App.Core.Domain.Employees;
using App.Data.Extensions;
using App.Services.Employees;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the experience model factory implementation
    /// </summary>
    public partial class ExperienceModelFactory : IExperienceModelFactory
    {
        #region Fields

        private readonly IExperienceService _experienceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public ExperienceModelFactory(IExperienceService experienceService,
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper,
            IBaseAdminModelFactory baseAdminModelFactory
            )
        {
            _experienceService = experienceService;
            _employeeService = employeeService;
            _dateTimeHelper = dateTimeHelper;
            _baseAdminModelFactory = baseAdminModelFactory;
    }

        #endregion
        #region Utitlitis
        public virtual async Task PrepareEmployeeListAsync(ExperienceModel model)
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

        public virtual async Task<ExperienceSearchModel> PrepareExperienceSearchModelAsync(ExperienceSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }
        public virtual async Task<ExperienceListModel> PrepareExperienceListModelAsync(ExperienceSearchModel searchModel, Employee employee)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get experience
            var experience = await _experienceService.GetAllExperienceAsync(employeeId:searchModel.employeeId,experienceName: searchModel.Experience, showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new ExperienceListModel().PrepareToGridAsync(searchModel, experience, () =>
            {
                return experience.SelectAwait(async Experience =>
                {
                    //fill in model values from the entity
                    var experienceModel = Experience.ToModel<ExperienceModel>();

                 
                        experienceModel.Froms = Experience.From.ToString("MM/dd/yyyy");
                    

                    experienceModel.Tos = Experience.To.ToString("MM/dd/yyyy");
                    
                    Employee emp = new Employee();
                   
                    emp = await _employeeService.GetEmployeeByIdAsync(experienceModel.EmployeeID);
                    if (emp != null)
                        experienceModel.EmployeeName = emp.FirstName + " " + emp.LastName;

                    return experienceModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<ExperienceModel> PrepareExperienceModelAsync(ExperienceModel model, Experience experience, bool excludeProperties = false)
        {
            if (experience != null)
            {
                if (model == null)
                {
                    //fill in model values from the entity
                    model = experience.ToModel<ExperienceModel>();

                   
                        var employee = await _employeeService.GetEmployeeByIdAsync(model.EmployeeID);
                        if (employee != null)
                        {
                            model.EmployeeName = employee.FirstName + " " + employee.LastName;
                        }
                    
                }
                var emp = await _employeeService.GetEmployeeByIdAsync(model.EmployeeID);

             

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
