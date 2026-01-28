using App.Core.Domain.EmployeeAttendances;
using App.Core.Domain.Employees;
using App.Data.Extensions;
using App.Services;
using App.Services.Employees;
using App.Services.Media;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.EmployeeAttendances;
using App.Web.Areas.Admin.Models.Employees;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Assets model factory implementation
    /// </summary>
    public partial class AssetsModelFactory : IAssetsModelFactory
    {
        #region Fields

        private readonly IAssetsService _assetsService;
        private readonly IEmployeeService _employeeService;
        private readonly IDownloadService _downloadService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public AssetsModelFactory(IAssetsService assetsService,
            IEmployeeService employeeService,IDownloadService downloadService,IBaseAdminModelFactory baseAdminModelFactory
            )
        {
            _downloadService = downloadService;
            _assetsService = assetsService;
            _employeeService = employeeService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion
        #region Utitlitis
        public virtual async Task PrepareEmployeeListAsync(AssetsModel model)
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

        public virtual async Task<AssetsSearchModel> PrepareAssetsSearchModelAsync(AssetsSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }
        public virtual async Task<AssetsListModel> PrepareAssetsListModelAsync(AssetsSearchModel searchModel, Employee employee)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get assets
            var assets = await _assetsService.GetAllAssetsAsync(employeeId:searchModel.employeeId,assestsName: searchModel.Assets, showHidden: true,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new AssetsListModel().PrepareToGridAsync(searchModel, assets, () =>
            {
                return assets.SelectAwait(async Assets =>
                {
                    //fill in model values from the entity
                    var assetsModel = Assets.ToModel<AssetsModel>();
                    var selectedAvailableTypeOption = Assets.TypeId;
                    assetsModel.Type = ((TypeEnum)selectedAvailableTypeOption).ToString();
                    Employee emp = new Employee();
                    emp = await _employeeService.GetEmployeeByIdAsync(assetsModel.EmployeeID);
                    assetsModel.EmployeeName = emp.FirstName + " " + emp.LastName;
                    assetsModel.DocumentId = Assets.DocumentId;

                    if (assetsModel.DocumentId > 0)
                    {
                        var download = await _downloadService.GetDownloadByIdAsync(assetsModel.DocumentId);
                        if (download != null)
                        {
                            assetsModel.DownloadGuid = download.DownloadGuid;
                           
                        }
                    }

                    return assetsModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<AssetsModel> PrepareAssetsModelAsync(AssetsModel model, Assets assets, bool excludeProperties = false)
        {
            var questiontype = await TypeEnum.Select.ToSelectListAsync();
            if (assets != null)
            {
                if (model == null)
                {
                    //fill in model values from the entity
                    model = assets.ToModel<AssetsModel>();

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
            model.Types = questiontype.Select(store => new SelectListItem
            {
                Value = store.Value,
                Text = store.Text,
                Selected = model.TypeId.ToString() == store.Value
            }).ToList();

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
