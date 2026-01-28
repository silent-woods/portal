using App.Core.Domain.Employees;
using App.Services.Employees;
using App.Services.Helpers;
using App.Web.Models.Employee;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using App.Core;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class EmployeeEducationModelFactory : IEmployeeEducationModelFactory
    {
        #region Fields

        private readonly IEducationService _educationService;
        private readonly IEmployeeService _employeeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EmployeeEducationModelFactory(IEducationService educationService,
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext
            )
        {
            _educationService = educationService;
            _employeeService = employeeService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods
        public virtual async Task<EmployeeEducationModel> PrepareEmployeeEducationModelAsync(EmployeeEducationModel model, Education education)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

            var educations = await _educationService.GetAllEducationsAsync(employee.Id);
            //return model;


            // Prepare the list of education details
            model.Educations = new List<EmployeeEducationModel>();

            foreach (var data in educations)
            {
                if (data != null)
                {
                    // Create a new education model
                    var educationModel = new EmployeeEducationModel
                    {
                        EmployeeName = employee.FirstName+" "+employee.LastName,
                        Course = data.Course,
                        InstitutionName = data.InstitutionName,
                        MarksScored = Math.Round(data.MarksScored, 2),
                        YearOfCompletion = data.YearOfCompletion
                    };

                    model.Educations.Add(educationModel);
                }
            }

            return model;
        }
        #endregion
    }
}
