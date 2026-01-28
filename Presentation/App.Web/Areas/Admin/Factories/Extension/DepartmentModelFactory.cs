using System;
using System.Linq;
using System.Threading.Tasks;
using App.Web.Framework.Models.Extensions;
using App.Data.Extensions;
using App.Services.Departments;
using App.Web.Areas.Admin.Models.Departments;
using App.Core.Domain.Departments;
using App.Services.Helpers;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class DepartmentModelFactory : IDepartmentModelFactory
    {
        #region Fields

        private readonly IDepartmentService _departmentService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public DepartmentModelFactory(
            IDepartmentService departmentService,
            IDateTimeHelper dateTimeHelper)
        {
            _departmentService = departmentService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare department search model
        /// </summary>
        /// <param name="searchModel">Department search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department search model
        /// </returns>
        public virtual async Task<DepartmentSearchModel> PrepareDepartmentSearchModelAsync(DepartmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare available countries
            //await _baseAdminModelFactory.PrepareCountriesAsync(searchModel.AvailableCountries);

            //prepare page parameters
            searchModel.SetGridPageSize();

            return searchModel;
        }

        /// <summary>
        /// Prepare paged department list model
        /// </summary>
        /// <param name="searchModel">Department search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department list model
        /// </returns>
        public virtual async Task<DepartmentListModel> PrepareDepartmentListModelAsync(DepartmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get departments
            var departments = await _departmentService.GetAllDepartmentsAsync(showHidden: true,
                name: searchModel.SearchName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare grid model
            var model = await new DepartmentListModel().PrepareToGridAsync(searchModel, departments, () =>
            {
                //fill in model values from the entity
                return departments.SelectAwait(async department =>
                {
                    //var departmentModel = department.ToModel<DepartmentModel>();

                    var departmentModel = new DepartmentModel();
                    departmentModel.Id = department.Id;
                    departmentModel.Name = department.Name;
                    departmentModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(department.CreatedOnUtc, DateTimeKind.Utc);
                    departmentModel.UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(department.UpdatedOnUtc, DateTimeKind.Utc);

                    return departmentModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare department model
        /// </summary>
        /// <param name="model">Department model</param>
        /// <param name="department">Department</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department model
        /// </returns>
        public virtual async Task<DepartmentModel> PrepareDepartmentModelAsync(DepartmentModel model, Department department, bool excludeProperties = false)
        {
            if (department != null)
            {
                //fill in model values from the entity
                if (model != null)
                {
                    // model = department.ToModel<DepartmentModel>();
                    model.Id = department.Id;
                    model.Name = department.Name;
                    model.CreatedOnUtc = department.CreatedOnUtc;
                    model.UpdatedOnUtc = department.UpdatedOnUtc;
                }
            }

            return model;
        }

        #endregion
    }
}
