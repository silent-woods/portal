using App.Core;
using App.Core.Domain.Common;
using App.Core.Domain.Employees;
using App.Data.Extensions;
using App.Services.Employees;
using App.Services.Helpers;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Factories;
using App.Web.Framework.Models.Extensions;
using App.Web.Models.Employee;
using App.Web.Models.Employee;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the experience model factory implementation
    /// </summary>
    public partial class EmployeeExperienceModelFactory : IEmployeeExperienceModelFactory
    {
        #region Fields

        private readonly IExperienceService _experienceService;
        private readonly IEmployeeService _employeeService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EmployeeExperienceModelFactory(IExperienceService experienceService,
            IEmployeeService employeeService,
            IDateTimeHelper dateTimeHelper,
            IWorkContext workContext
            )
        {
            _experienceService = experienceService;
            _employeeService = employeeService;
            _dateTimeHelper = dateTimeHelper;
            _workContext = workContext;
            
        }

        #endregion

        #region Methods        
        public virtual async Task<EmployeeExperienceModel> PrepareEmployeeExperienceModelAsync(EmployeeExperienceModel model)
        {
            // Get the current customer
            var customer = await _workContext.GetCurrentCustomerAsync();

            // Get the current employee
            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

            // Get the experiences for the current employee
            var experiences = await _experienceService.GetAllExperienceAsync(employeeId: employee.Id, "");

            // Create a new list to store the experiences
            model.Experiences = new List<EmployeeExperienceModel>();

            foreach (var experience in experiences)
            {
                if (experience != null)
                {
                    // Create a new EmployeeExperienceModel for each experience
                    var experienceModel = new EmployeeExperienceModel
                    {
                        EmployeeName = employee.FirstName + " " + employee.LastName,
                        PreviousCompanyName = experience.PreviousCompanyName,
                        Designation = experience.Designation,
                        Froms = experience.From.ToString("d-MMM-yyyy"),
                        Tos = experience.To.ToString("d-MMM-yyyy")
                    };

                    // Add the experience model to the list
                    model.Experiences.Add(experienceModel);
                }
            }

            return model;
        }
        #endregion
    }
}
