using System.Linq;
using System.Threading.Tasks;
using App.Core;
using App.Services.Employees;
using App.Services.Security;
using App.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;


namespace Nop.Web.Areas.Admin.Controllers
{
    public partial class SearchCompleteController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SearchCompleteController(
            IPermissionService permissionService,
            IEmployeeService employeeService,
            IWorkContext workContext)
        {
            _permissionService = permissionService;
            _employeeService = employeeService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> SearchAutoComplete(string term)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return Content(string.Empty);

            const int searchTermMinimumLength = 3;
            if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
                return Content(string.Empty);


          
            var products = await _employeeService.GetAllEmployeesAsync(employee: term);

            var result = (from p in products
                            select new
                            {
                                label = p.FirstName+" "+p.LastName,
                                productid = p.Id
                            }).ToList();

            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetEmployeeName(int employeeId)
        {
            // Example: Fetch employee name from service based on EmployeeId
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            if (employee == null)
            {
                return NotFound(); // Handle case where employee is not found
            }

            var data = new { employeeName = employee.FirstName+" "+employee.LastName }; // Adjust property as per your Employee model

            return Json(data);
        }


        #endregion
    }
}