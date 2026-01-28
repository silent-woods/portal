using App.Data.Extensions;
using App.Services;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Departments;
using App.Web.Areas.Admin.Models.Extension.TaskCategories;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the TaskCategory model factory implementation
    /// </summary>
    public partial class TaskCategoryModelFactory : ITaskCategoryModelFactory
    {
        #region Fields

        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public TaskCategoryModelFactory(
            ITaskCategoryService taskCategoryService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _taskCategoryService = taskCategoryService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare TaskCategory search model
        /// </summary>
        public virtual async Task<TaskCategorySearchModel> PrepareTaskCategorySearchModelAsync(TaskCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // set grid page size, sort, filters etc.
            searchModel.SetGridPageSize();

            return await Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare TaskCategory list model
        /// </summary>
        public virtual async Task<TaskCategoryListModel> PrepareTaskCategoryListModelAsync(TaskCategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // get entities from service
            var categories = await _taskCategoryService.GetAllTaskCategoriesAsync(
                categoryName: searchModel.SearchCategoryName,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            // prepare list model
            var model = await new TaskCategoryListModel().PrepareToGridAsync(searchModel, categories, () =>
            {
                //fill in model values from the entity
                return categories.SelectAwait(async category =>
                {
                    var taskCategoryModel = category.ToModel<TaskCategoryModel>();
                    taskCategoryModel.CreatedOn = category.CreatedOn;
                    return taskCategoryModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare TaskCategory model for create/edit
        /// </summary>
        public virtual async Task<TaskCategoryModel> PrepareTaskCategoryModelAsync(TaskCategoryModel model, TaskCategory entity, bool excludeProperties = false)
        {
            if (entity != null)
            {
                    if (model == null)
                    model = entity.ToModel<TaskCategoryModel>();
            }

           
            return await Task.FromResult(model);
        }

        #endregion
    }
}
