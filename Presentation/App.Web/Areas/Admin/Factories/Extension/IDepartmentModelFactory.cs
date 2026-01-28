using System.Threading.Tasks;
using App.Core.Domain.Departments;
using App.Web.Areas.Admin.Models.Departments;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the department model factory
    /// </summary>
    public partial interface IDepartmentModelFactory
    {
        /// <summary>
        /// Prepare department search model
        /// </summary>
        /// <param name="searchModel">Department search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department search model
        /// </returns>
        Task<DepartmentSearchModel> PrepareDepartmentSearchModelAsync(DepartmentSearchModel searchModel);

        /// <summary>
        /// Prepare paged department list model
        /// </summary>
        /// <param name="searchModel">Department search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the department list model
        /// </returns>
        Task<DepartmentListModel> PrepareDepartmentListModelAsync(DepartmentSearchModel searchModel);

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
        Task<DepartmentModel> PrepareDepartmentModelAsync(DepartmentModel model, Department department, bool excludeProperties = false);

    }
}