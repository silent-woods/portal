using App.Core;
using App.Core.Domain.Customers;
using App.Core.Domain.Employees;
using App.Core.Domain.Extension.Employees;
using App.Services.Customers;
using App.Services.Departments;
using App.Services.Designations;
using App.Services.Directory;
using App.Services.Employees;
using App.Services.Helpers;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Media;
using App.Services.Messages;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Factories.Extension;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Areas.Admin.Models.Extension.ActivityTracking;
using App.Web.Framework.Controllers;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Core.Domain.Security;

namespace App.Web.Areas.Admin.Controllers.Extension
{
    public partial class EmployeeController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IEmployeeModelFactory _employeeModelFactory;
        private readonly IEmployeeService _employeeService;
        private readonly IPictureService _pictureService;
        private readonly IEducationModelFactory _educationModelFactory;
        private readonly IEducationService _educationService;
        private readonly IExperienceModelFactory _experienceModelFactory;
        private readonly IExperienceService _experienceService;
        private readonly IAssetsModelFactory _assetsModelFactory;
        private readonly IAssetsService _assetsService;
        private readonly IEmpAddressModelFactory _empAddressModelFactory;
        private readonly IEmpAddressService _empAddressService;
        private readonly IDesignationService _designationService;
        private readonly ICountryService _countryService;
        private readonly ICustomerService _customerService;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWorkContext _workContext;
        private readonly CustomerSettings _customerSettings;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly EmployeeSettings _employeeSettings;
        private readonly IDepartmentService _departmentService;
        private readonly ITokenizer _tokenizer;
        private readonly IReportsModelFactory _reportsModelFactory;


        #endregion Fields

        #region Ctor

        public EmployeeController(
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IEmployeeModelFactory employeeModelFactory,
            IEmployeeService employeeService,
            IPictureService pictureService,
            IEducationModelFactory educationModelFactory,
            IEducationService educationService,
            IExperienceModelFactory experienceModelFactory,
            IExperienceService experienceService,
            IAssetsModelFactory assetsModelFactory,
            IAssetsService assetsService,
            IEmpAddressModelFactory empAddressModelFactory,
            IEmpAddressService empAddressService,
            IDesignationService designationService,
            ICountryService countryService,
            ICustomerService customerService, IWorkflowMessageService workflowMessageService, IWorkContext workContext, CustomerSettings customerSettings,
            ICustomerRegistrationService customerRegistrationService,
            IDateTimeHelper dateTimeHelper,
            EmployeeSettings employeeSettings,
            IDepartmentService departmentService,
            ITokenizer tokenizer
, IReportsModelFactory reportsModelFactory)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _employeeModelFactory = employeeModelFactory;
            _employeeService = employeeService;
            _pictureService = pictureService;
            _educationModelFactory = educationModelFactory;
            _educationService = educationService;
            _experienceModelFactory = experienceModelFactory;
            _experienceService = experienceService;
            _assetsModelFactory = assetsModelFactory;
            _assetsService = assetsService;
            _empAddressModelFactory = empAddressModelFactory;
            _empAddressService = empAddressService;
            _designationService = designationService;
            _countryService = countryService;
            _customerService = customerService;
            _workflowMessageService = workflowMessageService;
            _workContext = workContext;
            _customerSettings = customerSettings;
            _customerRegistrationService = customerRegistrationService;

            _dateTimeHelper = dateTimeHelper;
            _employeeSettings = employeeSettings;
            _departmentService = departmentService;
            _tokenizer = tokenizer;
            _reportsModelFactory = reportsModelFactory;
        }

        #endregion

        #region  Employee List

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _employeeModelFactory.PrepareEmployeeSearchModelAsync(new EmployeeSearchModel());

            return View("/Areas/Admin/Views/Extension/Employee/List.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(EmployeeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.View))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _employeeModelFactory.PrepareEmployeeListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region   Employee Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _employeeModelFactory.PrepareEmployeeModelAsync(new EmployeeModel(), null);

            return View("/Areas/Admin/Views/Extension/Employee/Create.cshtml", model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(EmployeeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.Add))
                return AccessDeniedView();

            int selectedManagerId = model.SelectedManagerId.FirstOrDefault();
            model.ManagerId = selectedManagerId;

          

            if (ModelState.IsValid)
            {
                var employee = model.ToEntity<Employee>();
                employee.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();
                employee.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                // Check if the email exists as a customer
                var customer = !string.IsNullOrWhiteSpace(model.OfficialEmail) ? await _customerService.GetCustomerByEmailAsync(model.OfficialEmail) : null;

                // Check if the email exists as an employee
                var existingEmployee = !string.IsNullOrWhiteSpace(model.OfficialEmail) ? await _employeeService.GetEmployeeByEmailAsync(model.OfficialEmail) : null;

                if (customer != null && existingEmployee != null)
                {
                    // The email exists as both a customer and an employee
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Account.EmailUsernameErrors.EmailAlreadyExists"));
                    model = await _employeeModelFactory.PrepareEmployeeModelAsync(model, null);
                    return View("/Areas/Admin/Views/Extension/Employee/Create.cshtml", model);
                }
                else if (customer == null && existingEmployee == null)
                {
                    var allCustomerRoles = await _customerService.GetAllCustomerRolesAsync(true);
                    var newCustomerRoles = new List<CustomerRole>();
                    foreach (var customerRole in allCustomerRoles)
                        if (customerRole.Id == 3)
                            newCustomerRoles.Add(customerRole);
                    // The email does not exist as either a customer or an employee
                    // Create a new customer
                    var newCustomer = new Customer
                    {
                        Email = model.OfficialEmail,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Gender = model.Gender,
                        Phone=model.MobileNo,
                        City=model.Location,
                        Active = true
                        
                    };

                   

                    await _customerService.InsertCustomerAsync(newCustomer);
                    // Assign the registered customer role to the new customer
                    var registeredCustomerRole = await _customerService.GetCustomerRoleByIdAsync(3);
                    if (registeredCustomerRole != null)
                    {
                        await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping 
                        { CustomerId = newCustomer.Id, 
                            CustomerRoleId = registeredCustomerRole.Id 
                        });
                    }

                    // Set the customer ID for the new employee
                    employee.Customer_Id = newCustomer.Id;

                    // Insert the new employee
                    await _employeeService.InsertEmployeeAsync(employee);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Employee.Added"));

                    //send welcome mail when employee created.

             //       await _workflowMessageService.SendWelcomeMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id,
             //employee.OfficialEmail.Trim(), employee.FirstName + " " + employee.LastName, employee.Id);

                    await _employeeService.UpdateEmployeeAsync(employee);



                }
                else if (customer != null && existingEmployee == null)
                {
                    // The email exists as a customer but not as an employee
                    // Set the customer ID for the new employee
                    employee.Customer_Id = customer.Id;

                    // Insert the new employee
                    await _employeeService.InsertEmployeeAsync(employee);
                    _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Employee.Added"));
                    await _employeeService.UpdateEmployeeAsync(employee);
                }
                else if (customer == null && existingEmployee != null)
                {
                   // that is not possible
                }
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    var changePassRequest = new ChangePasswordRequest(model.OfficialEmail, false, _customerSettings.DefaultPasswordFormat, model.Password);
                    var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
                    if (!changePassResult.Success)
                    {
                        foreach (var changePassError in changePassResult.Errors)
                            _notificationService.ErrorNotification(changePassError);
                    }
                }
                // Redirect to the appropriate view
                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = employee.Id });
            }

            // Prepare model and redisplay form if something failed
            model = await _employeeModelFactory.PrepareEmployeeModelAsync(model, null, true);
            return View("/Areas/Admin/Views/Extension/Employee/Create.cshtml", model);
        }


        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a employee with the specified id
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _employeeModelFactory.PrepareEmployeeModelAsync(null, employee);

            return View("/Areas/Admin/Views/Extension/Employee/Edit.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(EmployeeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a employee with the specified id
            var employee = await _employeeService.GetEmployeeByIdAsync(model.Id);
            if (employee == null)
                return RedirectToAction("List");

            int selectedManagerId = model.SelectedManagerId.FirstOrDefault();
            model.ManagerId = selectedManagerId;

            if (ModelState.IsValid)
            {
                employee = model.ToEntity<Employee>();
                model.PictureUrl = await _pictureService.GetPictureUrlAsync(model.PictureId);
                employee.CreateOnUtc = await _dateTimeHelper.GetUTCAsync();
                employee.UpdateOnUtc = await _dateTimeHelper.GetUTCAsync();

                // Check if the email exists as a customer
                var customer = !string.IsNullOrWhiteSpace(model.OfficialEmail) ? await _customerService.GetCustomerByEmailAsync(model.OfficialEmail) : null;

                // Check if the email exists as an employee
                var existingEmployee = !string.IsNullOrWhiteSpace(model.OfficialEmail) ? await _employeeService.GetEmployeeByEmailAsync(model.OfficialEmail) : null;

                if (customer != null && existingEmployee != null)
                {
                    var existingCustomer = await _customerService.GetCustomerByIdAsync(employee.Customer_Id);

                    if (existingCustomer != null)
                    {

                        existingCustomer.Email = model.OfficialEmail;
                        existingCustomer.FirstName = model.FirstName;
                        existingCustomer.LastName = model.LastName;
                        existingCustomer.Gender = model.Gender;
                        existingCustomer.Phone = model.MobileNo;
                        existingCustomer.City = model.Location;

                        await _customerService.UpdateCustomerAsync(existingCustomer);

                        // Update the employee's email
                        employee.OfficialEmail = model.OfficialEmail;
                        // employee.Customer_Id = existingCustomer.Id;

                        await _employeeService.UpdateEmployeeAsync(employee);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Employee.Updated"));
                    }
                    else
                    {

                        _notificationService.ErrorNotification("Customer not found for the given Customer_Id.");
                    }

                }

                else if (customer == null && existingEmployee == null)
                {
                    var existingCustomer = await _customerService.GetCustomerByIdAsync(employee.Customer_Id);

                    if (existingCustomer != null)
                    {

                        existingCustomer.Email = model.OfficialEmail;
                        existingCustomer.FirstName = model.FirstName;
                        existingCustomer.LastName = model.LastName;
                        existingCustomer.Gender = model.Gender;
                        existingCustomer.Phone = model.MobileNo;
                        existingCustomer.City = model.Location;

                        await _customerService.UpdateCustomerAsync(existingCustomer);

                        // Update the employee's email
                        employee.OfficialEmail = model.OfficialEmail;
                       // employee.Customer_Id = existingCustomer.Id;

                        await _employeeService.UpdateEmployeeAsync(employee);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Employee.Updated"));
                    }
                    else
                    {
                        
                        _notificationService.ErrorNotification("Customer not found for the given Customer_Id.");
                    }

                }
               
                // Redirect to the appropriate view
                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = employee.Id });
            }


            //prepare model
            model = await _employeeModelFactory.PrepareEmployeeModelAsync(model, employee, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Employee/Edit.cshtml", model);
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("changepassword")]
        public virtual async Task<IActionResult> ChangePassword(EmployeeModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(model.Customer_Id);
            if (customer == null)
                return RedirectToAction("List");

            //ensure that the current customer cannot change passwords of "Administrators" if he's not an admin himself
            if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.OnlyAdminCanChangePassword"));
                return RedirectToAction("Edit", new { id = customer.Id });
            }

            if (!ModelState.IsValid)
                return RedirectToAction("Edit", new { id = customer.Id });

            var changePassRequest = new ChangePasswordRequest(model.OfficialEmail,
                false, _customerSettings.DefaultPasswordFormat, model.Password);
            var changePassResult = await _customerRegistrationService.ChangePasswordAsync(changePassRequest);
            if (changePassResult.Success)
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.PasswordChanged"));
            else
                foreach (var error in changePassResult.Errors)
                    _notificationService.ErrorNotification(error);

            return RedirectToAction("Edit", new { id = customer.Id });
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a topic with the specified id
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
                return RedirectToAction("List");
            await _employeeService.DeleteEmployeeAsync(employee);

            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Employee.Deleted"));

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _employeeService.GetEmployeeByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _employeeService.DeleteEmployeeAsync(item);
            }

            return Json(new { Result = true });
        }

        public virtual async Task<IActionResult> SendOnBordingEmail(int employeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployee))
                return AccessDeniedView();

            //try to get a employee with the specified id
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                ViewBag.RefreshPage = false;

                return View("/Areas/Admin/Views/Extension/Employee/OnBordingEmail.cshtml");

            }

            ViewBag.RefreshPage = false;

            //fillup Tokens

            var department = await _departmentService.GetDepartmentByIdAsync(employee.DepartmentId);
            string departmentName = "";
            if (department != null)
                departmentName = department.Name;

            var designation = await _designationService.GetDesignationByIdAsync(employee.DesignationId);
            string designationName = "";
            if (designation != null)
                designationName = designation.Name;
;
            var commonTokens = new List<Token>
    {
        new Token("Employee.Name", employee.FirstName+" "+employee.LastName),
        new Token("Employee.OfficialEmail", employee.OfficialEmail),
        new Token("Employee.Department",departmentName),
        new Token("Employee.Designation",designationName),
        
    };
            var bodyReplaced = _tokenizer.Replace(_employeeSettings.OnBoardingEmail, commonTokens, true);

            //prepare model
            var model = await _employeeModelFactory.PrepareEmployeeModelAsync(null, employee, true);

            model.OnBordingEmail = bodyReplaced;

            return View("/Areas/Admin/Views/Extension/Employee/OnBordingEmail.cshtml",model);
        }


        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> SendOnBordingEmail(EmployeeModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageDesignation))
                return AccessDeniedView();

          
                await _workflowMessageService.SendEmployeeOnBordingMessageAsync((await _workContext.GetWorkingLanguageAsync()).Id, model.Id, model.OnBordingEmail);
                string successMessage = await _localizationService.GetResourceAsync("Admin.Catalog.designation.Added");
                TempData["SuccessMessage"] = successMessage;

                ViewBag.RefreshPage = true;
            
            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Employee/OnBordingEmail.cshtml", model);
        }

        #endregion

        #region Education List/ Create /Edit / Delete

        [HttpPost]
        public virtual async Task<IActionResult> EducationList(EducationSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.View))
                return AccessDeniedView();
            var employee = await _employeeService.GetEmployeeByIdAsync(searchModel.employeeId);
            //prepare model
            var model = await _educationModelFactory.PrepareEducationListModelAsync(searchModel,employee);
            return Json(model);
        }
        public virtual async Task<IActionResult> EducationCreate(int employeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _educationModelFactory.PrepareEducationModelAsync(new EducationModel(), null);
            model.EmployeeID = employeeId;
            if (employeeId > 0)
            {               
                    model.SelectedEmployeeId.Add(employeeId);               
            }

            return View("/Areas/Admin/Views/Extension/Employee/EducationCreate.cshtml", model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EducationCreate(EducationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeID = selectedEmployeeId;

            var education = model.ToEntity<Education>();

            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                //prepare model
                model = await _educationModelFactory.PrepareEducationModelAsync(model, education, true);

                //if we got this far, something failed, redisplay form
                return View("/Areas/Admin/Views/Extension/Employee/EducationCreate.cshtml", model);
            }


            if (ModelState.IsValid)
            {
                await _educationService.InsertEducationAsync(education);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeEducation.Added"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = education.EmployeeID });

                return RedirectToAction("EducationEdit", new { id = education.Id , employeeId = education.EmployeeID});
            }

            //prepare model
            model = await _educationModelFactory.PrepareEducationModelAsync(model, education, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Employee/EducationCreate.cshtml", model);
        }
        public virtual async Task<IActionResult> EducationEdit(int employeeId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.Edit))
                return AccessDeniedView();

            var education = await _educationService.GetEducationByIdAsync(id);
            if (education == null)
                return RedirectToAction("EducationList");

            //prepare model
            var model = await _educationModelFactory.PrepareEducationModelAsync(null, education);
            model.EmployeeID = employeeId;
            return View("/Areas/Admin/Views/Extension/Employee/EducationEdit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> EducationEdit(EducationModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var education = await _educationService.GetEducationByIdAsync(model.Id);
            if (education == null)
                return RedirectToAction("EducationList");

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeID = selectedEmployeeId;

            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                //prepare model
                model = await _educationModelFactory.PrepareEducationModelAsync(model, education, true);

                //if we got this far, something failed, redisplay form
                return View("/Areas/Admin/Views/Extension/Employee/EducationEdit.cshtml", model);
            }


            if (ModelState.IsValid)
            {
                education = model.ToEntity(education);

                await _educationService.UpdateEducationAsync(education);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeEducation.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = education.EmployeeID });
              

                return RedirectToAction("EducationEdit", new { id = education.Id , employeeId = education.EmployeeID});
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Employee/EducationEdit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> EducationDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var education = await _educationService.GetEducationByIdAsync(id);
            if (education == null)
                return RedirectToAction("EducationList");

            await _educationService.DeleteEducationAsync(education);


            return new NullJsonResult();
        }
        [HttpPost]
        public virtual async Task<IActionResult> EducationDeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEducation, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _educationService.GetEducationByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _educationService.DeleteEducationAsync(item);
            }
            return Json(new { Result = true });
        }

        #endregion

        #region Experience List / Create / Edit / Delete
        
        [HttpPost]
        public virtual async Task<IActionResult> ExperienceList(ExperienceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.View))
                return AccessDeniedView();
            var employee = await _employeeService.GetEmployeeByIdAsync(searchModel.employeeId);
            //prepare model
            var model = await _experienceModelFactory.PrepareExperienceListModelAsync(searchModel,employee);
            return Json(model);
        }
        public virtual async Task<IActionResult> ExperienceCreate(int employeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.Add))
                return AccessDeniedView();

          
            //prepare model
            var model = await _experienceModelFactory.PrepareExperienceModelAsync(new ExperienceModel(), null);
            model.EmployeeID = employeeId;
            if (employeeId > 0)
            {
                model.SelectedEmployeeId.Add(employeeId);
            }


            return View("/Areas/Admin/Views/Extension/Employee/ExperienceCreate.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ExperienceCreate(ExperienceModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeID = selectedEmployeeId;

            var experience = model.ToEntity<Experience>();
            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                //prepare model
                model = await _experienceModelFactory.PrepareExperienceModelAsync(model, experience, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Employee/ExperienceCreate.cshtml", model);
            }
            if (experience.From > experience.To)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeExperience.Error.ToGreaterThenFrom"));
                model = await _experienceModelFactory.PrepareExperienceModelAsync(model, experience, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Employee/ExperienceCreate.cshtml", model);
            }

            if (ModelState.IsValid)
            {  
                await _experienceService.InsertExperienceAsync(experience);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeExperience.Added"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = experience.EmployeeID });

                return RedirectToAction("ExperienceEdit", new { id = experience.Id, employeeId = experience.EmployeeID });
            }

            //prepare model
            model = await _experienceModelFactory.PrepareExperienceModelAsync(model, experience, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Employee/ExperienceCreate.cshtml", model);
        }
        public virtual async Task<IActionResult> ExperienceEdit(int employeeId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.Edit))
                return AccessDeniedView();


            var experience = await _experienceService.GetExperienceByIdAsync(id);
            if (experience == null)
                return RedirectToAction("ExperienceList");

            //prepare model
            var model = await _experienceModelFactory.PrepareExperienceModelAsync(null, experience);
            model.EmployeeID = employeeId;
            return View("/Areas/Admin/Views/Extension/Employee/ExperienceEdit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> ExperienceEdit(ExperienceModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a project with the specified id
            var experience = await _experienceService.GetExperienceByIdAsync(model.Id);
            if (experience == null)
                return RedirectToAction("ExperienceList");

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeID = selectedEmployeeId;

            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _experienceModelFactory.PrepareExperienceModelAsync(model, experience, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Employee/ExperienceCreate.cshtml", model);
            }


            if (ModelState.IsValid)
            {
                experience = model.ToEntity(experience);
                if (experience.From > experience.To)
                {
                    _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeExperience.Error.ToGreaterThenFrom"));
                    model = await _experienceModelFactory.PrepareExperienceModelAsync(model, experience, true);

                    //if we got this far, something failed, redisplay form

                    return View("/Areas/Admin/Views/Extension/Employee/ExperienceCreate.cshtml", model);
                }

                await _experienceService.UpdateExperienceAsync(experience);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeExperience.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = experience.EmployeeID });

                return RedirectToAction("ExperienceEdit", new { id = experience.Id , employeeId=experience.EmployeeID});
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Employee/ExperienceEdit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> ExperienceDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a leaveType with the specified id
            var experience = await _experienceService.GetExperienceByIdAsync(id);
            if (experience == null)
                return RedirectToAction("ExperienceList");

            await _experienceService.DeleteExperienceAsync(experience);


            return new NullJsonResult();
        }
        [HttpPost]
        public virtual async Task<IActionResult> ExperienceDeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageExperience, PermissionAction.Delete))
                return AccessDeniedView();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var data = await _experienceService.GetExperienceByIdsAsync(selectedIds.ToArray());

            foreach (var item in data)
            {
                await _experienceService.DeleteExperienceAsync(item);
            }
            return Json(new { Result = true });
        }
        #endregion

        #region Assets  List / Create / Edit / Delete
        
        [HttpPost]
        public virtual async Task<IActionResult> AssetsList(AssetsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAssets, PermissionAction.View))
                return AccessDeniedView();
            var employee = await _employeeService.GetEmployeeByIdAsync(searchModel.employeeId);
            //prepare model
            var model = await _assetsModelFactory.PrepareAssetsListModelAsync(searchModel,employee);
            return Json(model);
        }
        public virtual async Task<IActionResult> AssetsCreate(int employeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAssets, PermissionAction.Add))
                return AccessDeniedView();


            //prepare model
            var model = await _assetsModelFactory.PrepareAssetsModelAsync(new AssetsModel(), null);
            model.EmployeeID = employeeId;
            if (employeeId > 0)
            {
                model.SelectedEmployeeId.Add(employeeId);
            }

            return View("/Areas/Admin/Views/Extension/Employee/AssetsCreate.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> AssetsCreate(AssetsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAssets, PermissionAction.Add))
                return AccessDeniedView();

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeID = selectedEmployeeId;

            var assets = model.ToEntity<Assets>();

            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _assetsModelFactory.PrepareAssetsModelAsync(model, assets, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Employee/AssetsCreate.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                await _assetsService.InsertAssetsAsync(assets);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAssets.Added"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = assets.EmployeeID });

                return RedirectToAction("AssetsEdit", new { id = assets.Id , employeeId=assets.EmployeeID});
            }

            //prepare model
            model = await _assetsModelFactory.PrepareAssetsModelAsync(model, assets, true);

            //if we got this far, something failed, redisplay form

            return View("/Areas/Admin/Views/Extension/Employee/AssetsCreate.cshtml", model);
        }
        public virtual async Task<IActionResult> AssetsEdit(int employeeId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAssets, PermissionAction.Edit))
                return AccessDeniedView();

            var assets = await _assetsService.GetAssetsByIdAsync(id);
            if (assets == null)
                return RedirectToAction("AssetsList");

            //prepare model
            var model = await _assetsModelFactory.PrepareAssetsModelAsync(null, assets);
            model.EmployeeID = employeeId;
            return View("/Areas/Admin/Views/Extension/Employee/AssetsEdit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> AssetsEdit(AssetsModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAssets, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a assets with the specified id
            var assets = await _assetsService.GetAssetsByIdAsync(model.Id);
            if (assets == null)
                return RedirectToAction("AssetsList");

            int selectedEmployeeId = model.SelectedEmployeeId.FirstOrDefault();
            model.EmployeeID = selectedEmployeeId;

         

            if (selectedEmployeeId == 0)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Catalog.Common.SelectEmployeeValidation"));
                model = await _assetsModelFactory.PrepareAssetsModelAsync(model, assets, true);

                //if we got this far, something failed, redisplay form

                return View("/Areas/Admin/Views/Extension/Employee/AssetsEdit.cshtml", model);
            }

            if (ModelState.IsValid)
            {
                assets = model.ToEntity(assets);

                await _assetsService.UpdateAssetsAsync(assets);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAssets.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = assets.EmployeeID });

                return RedirectToAction("AssetsEdit", new { id = assets.Id, employeeId = assets.EmployeeID });
            }
            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Employee/AssetsEdit.cshtml", model);
        }
        [HttpPost]
        public virtual async Task<IActionResult> AssetsDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAssets, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a assets with the specified id
            var assets = await _assetsService.GetAssetsByIdAsync(id);
            if (assets == null)
                return RedirectToAction("AssetsList");

            await _assetsService.DeleteAssetsAsync(assets);

            

            return new NullJsonResult();
        }
        #endregion

        #region Address / List / Create / Edit / Delete

        [HttpPost]
        public virtual async Task<IActionResult> AddressList(EmpAddressSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAddress, PermissionAction.View))
                return AccessDeniedView();
            //prepare model
            var employee = await _employeeService.GetEmployeeByIdAsync(searchModel.employeeId);
            var model = await _empAddressModelFactory.PrepareAddressListModelAsync(searchModel,employee);
            return Json(model);
        }
        public virtual async Task<IActionResult> AddressCreate(int employeeId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAddress, PermissionAction.Add))
                return AccessDeniedView();

            //prepare model
            var model = await _empAddressModelFactory.PrepareAddressModelAsync(new EmpAddressModel(), null);
            model.EmployeeId = employeeId;

            return View("/Areas/Admin/Views/Extension/Employee/AddressCreate.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> AddressCreate(EmpAddressModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAddress, PermissionAction.Add))
                return AccessDeniedView();

            var address = model.ToEntity<EmpAddress>();

            if (ModelState.IsValid)
            {
                address.CreatedOnUtc = await _dateTimeHelper.GetUTCAsync();
                await _empAddressService.InsertAddressAsync(address);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAddress.Added"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = address.EmployeeID });

                return RedirectToAction("AddressEdit", new { id = address.Id , employeeId = address.EmployeeID});
            }

            //prepare model
            model = await _empAddressModelFactory.PrepareAddressModelAsync(model, address, true);

            //if we got this far, something failed, redisplay form
            return View("/Areas/Admin/Views/Extension/Employee/AddressCreate.cshtml", model); ;
        }
        public virtual async Task<IActionResult> AddressEdit(int employeeId, int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAddress, PermissionAction.Edit))
                return AccessDeniedView();

            var address = await _empAddressService.GetAddressByIdAsync(id);
            if (address == null)
                return RedirectToAction("AddressList");

            //prepare model
            var model = await _empAddressModelFactory.PrepareAddressModelAsync(null, address);
            model.EmployeeId = employeeId;
            return View("/Areas/Admin/Views/Extension/Employee/AddressEdit.cshtml", model);
        }
        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> AddressEdit(EmpAddressModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAddress, PermissionAction.Edit))
                return AccessDeniedView();

            //try to get a address with the specified id
            var address = await _empAddressService.GetAddressByIdAsync(model.Id);
            if (address == null)
                return RedirectToAction("AddressList");

            if (ModelState.IsValid)
            {
                address = model.ToEntity(address);

                await _empAddressService.UpdateAddressAsync(address);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Catalog.EmployeeAddress.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Edit", "Employee", new { id = address.EmployeeID });

                return RedirectToAction("AddressEdit", new { id = address.Id, employeeId = address.EmployeeID});
            }
            //if we got this far, something failed, redisplay form
            return View("AddressList");
        }
        [HttpPost]
        public virtual async Task<IActionResult> AddressDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeAddress, PermissionAction.Delete))
                return AccessDeniedView();

            //try to get a address with the specified id
            var address = await _empAddressService.GetAddressByIdAsync(id);
            if (address == null)
                return RedirectToAction("AddressList");

            await _empAddressService.DeleteAddressAsync(address);

       

            return new NullJsonResult();
            //return Json(new { success = true });

        }
        #endregion

        #region Activity Tracking

        public virtual async Task<IActionResult> ActivityTracking()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeCurrentActivity, PermissionAction.View))
                return AccessDeniedView();

            //prepare model
            var model = await _reportsModelFactory.PrepareActivityTrackingSearchModelAsync(new ActivityTrackingSearchModel());

            return View("/Areas/Admin/Views/Extension/PerformanceReports/ActivityTracking.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ActivityTracking(ActivityTrackingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageEmployeeCurrentActivity, PermissionAction.View))
                return await AccessDeniedDataTablesJson();


            if (searchModel.SearchDate == null)
            {             
                return Json(null);
            }
            //prepare model
            var model = await _reportsModelFactory.PrepareActivityTrackingListModelAsync(searchModel);
            
            return Json(model);
        }

        #endregion
    }

}

