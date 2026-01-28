using App.Core;
using App.Core.Domain.Employees;
using App.Services.Customers;
using App.Services.Departments;
using App.Services.Designations;
using App.Services.Employees;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Models.Employee;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the employee model factory
    /// </summary>
    public partial class EmployeeModelFactory : IEmployeeModelFactory
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IDepartmentService _departmentService;
        private readonly IDesignationService _designationService;
        private readonly IEmployeeService _employeeService;

        #endregion

        #region Ctor

        public EmployeeModelFactory(ICustomerService customerService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IDepartmentService departmentService,
            IDesignationService designationService,
            IEmployeeService employeeService)
        {
            _customerService = customerService;
            _storeContext = storeContext;
            _workContext = workContext;
            _departmentService = departmentService;
            _designationService = designationService;
            _employeeService = employeeService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare the employee info model
        /// </summary>
        /// <param name="model">Customer info model</param>
        /// <param name="employee">Employee</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the customer info model
        /// </returns>
        public virtual async Task<EmployeeInfoModel> PrepareEmployeeInfoModelAsync(EmployeeInfoModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

         
            var customer = await _workContext.GetCurrentCustomerAsync();

           if(customer == null)
                throw new ArgumentNullException(nameof(customer));

             var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);

            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            var selectedAvailableBloodGroupOption = employee.BloodGroupId;
            var selectedAvailableMaritalsStatusOption = employee.MaritalStatusId;
            var selectedAvailableEmployeeStatusOption = employee.EmployeeStatusId;

            if (employee.Id > 0)
            {
                model.FirstName = employee.FirstName;
                model.LastName = employee.LastName;
                model.PersonalEmail = employee.PersonalEmail;
                model.Gender = employee.Gender;
                model.MobileNo = employee.MobileNo;
                model.OfficialEmail = employee.OfficialEmail;
                model.Location = employee.Location;
                model.Department = (await _departmentService.GetDepartmentByIdAsync(employee.DepartmentId)).Name;
                model.Designation = (await _designationService.GetDesignationByIdAsync(employee.DesignationId)).Name;
                model.DateofJoining = employee.DateofJoining.ToString("d-MMM-yyyy");
                model.DateOfBirth = employee.DateOfBirth.ToString("d-MMM-yyyy");

                model.CTC = employee.CTC;
                model.MaritalStatus = ((MaritalStatusEnum)selectedAvailableMaritalsStatusOption).ToString() == "Select" ? string.Empty : ((MaritalStatusEnum)selectedAvailableMaritalsStatusOption).ToString();


                model.FatherName = employee.FatherName;
                model.MotherName = employee.MotherName;
                model.Hobbies = employee.Hobbies;

                if (employee.Hobbies != null)
                {
                    var input = employee.Hobbies ;
                    model.Hobbies = Regex.Replace(input, "<.*?>", string.Empty);
                }
                else
                {
                    model.Hobbies = string.Empty;
                }

                model.BloodGroup = ((BloodGroupEnum)selectedAvailableBloodGroupOption).ToString() == "Select" ? string.Empty : ((BloodGroupEnum)selectedAvailableBloodGroupOption).ToString();

                model.EmergencyContactPerson = employee.EmergencyContactPerson;
                model.Relationship = employee.Relationship;
                model.ContactNumber = employee.ContactNumber;
                model.EmployeeStatus = ((EmployeeStatusEnum)selectedAvailableEmployeeStatusOption).ToString();
                //model. = employee.OfficialEmail;
                //model.OfficialEmail = employee.OfficialEmail;
                model.IsSeniorStatus = employee.IsSeniorStatus;
            }

            return model;
        }

        #endregion
    }
}