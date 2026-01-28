using App.Core;
using App.Core.Domain.Employees;
using App.Services;
using App.Services.Employees;
using App.Web.Models.Employee;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Factories
{
    /// <summary>
    /// Represents the Assets model factory implementation
    /// </summary>
    public partial class EmployeeAssetsModelFactory : IEmployeeAssetsModelFactory
    {
        #region Fields

        private readonly IAssetsService _assetsService;
        private readonly IEmployeeService _employeeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public EmployeeAssetsModelFactory(IAssetsService assetsService,
            IEmployeeService employeeService,
            IWorkContext workContext
            )
        {
            _assetsService = assetsService;
            _employeeService = employeeService;
            _workContext = workContext;
        }

        #endregion

        #region Methods        
        public virtual async Task<EmployeeAssetsModel> PrepareEmployeeAssetsModelAsync(EmployeeAssetsModel model, Assets assets)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var employee = await _employeeService.GetEmployeeByCustomerIdAsync(customer.Id);
            var questiontype = await TypeEnum.Select.ToSelectListAsync();

            model.Types = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.TypeId.ToString() == store.Value
            }).ToList();

            //get educations
            
            var assetses = await _assetsService.GetAllAssetsAsync(employee.Id, "");
            model.Assets = new List<EmployeeAssetsModel>();
                

            foreach (var data in assetses)
            {
                if (data != null)
                {
                    var assetDetailModel = new EmployeeAssetsModel()
                    {
                        EmployeeName = employee.FirstName + " " + employee.LastName,
                        Type = ((TypeEnum)data.TypeId).ToString(),
                        Name = data.Name,
                        Description = data.Description
                    };

                    model.Assets.Add(assetDetailModel);
                }
            }

            return model;
        }
        #endregion
    }
}
