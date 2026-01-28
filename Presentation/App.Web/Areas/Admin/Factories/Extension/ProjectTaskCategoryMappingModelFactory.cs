using App.Core.Domain.Projects;
using App.Data.Extensions;
using App.Services.Helpers;
using App.Services.Projects;

using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.ProjectTaskCategoryMappings;
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
    /// Represents the ProjectTaskCategoryMapping model factory implementation
    /// </summary>
    public partial class ProjectTaskCategoryMappingModelFactory : IProjectTaskCategoryMappingModelFactory
    {
        #region Fields

        private readonly IProjectTaskCategoryMappingService _projectTaskCategoryMappingService;
        private readonly IProjectsService _projectsService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public ProjectTaskCategoryMappingModelFactory(
            IProjectTaskCategoryMappingService projectTaskCategoryMappingService,
            IProjectsService projectsService,
            ITaskCategoryService taskCategoryService,
            IDateTimeHelper dateTimeHelper)
        {
            _projectTaskCategoryMappingService = projectTaskCategoryMappingService;
            _projectsService = projectsService;
            _taskCategoryService = taskCategoryService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        protected virtual async Task PrepareProjectListAsync(ProjectTaskCategoryMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableProjects.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });

            var projects = await _projectsService.GetAllProjectsAsync(string.Empty);
            foreach (var p in projects)
            {
                model.AvailableProjects.Add(new SelectListItem
                {
                    Text = p.ProjectTitle,
                    Value = p.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareTaskCategoryListAsync(ProjectTaskCategoryMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableTaskCategories.Add(new SelectListItem
            {
                Text = "Select",
                Value = "0"
            });

            var taskCategories = await _taskCategoryService.GetAllTaskCategoriesAsync(string.Empty,isActive:true);
            foreach (var t in taskCategories)
            {
                model.AvailableTaskCategories.Add(new SelectListItem
                {
                    Text = t.CategoryName,
                    Value = t.Id.ToString()
                });
            }
        }

        #endregion

        #region Methods

        public virtual async Task<ProjectTaskCategoryMappingSearchModel> PrepareProjectTaskCategoryMappingSearchModelAsync(ProjectTaskCategoryMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<ProjectTaskCategoryMappingListModel> PrepareProjectTaskCategoryMappingListModelAsync(ProjectTaskCategoryMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var mappings = await _projectTaskCategoryMappingService.GetAllMappingsAsync(
                projectId: searchModel.ProjectId,
               
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new ProjectTaskCategoryMappingListModel().PrepareToGridAsync(searchModel, mappings, () =>
            {
                return mappings.SelectAwait(async m =>
                {
                    var mappingModel = m.ToModel<ProjectTaskCategoryMappingModel>();

                    var project = await _projectsService.GetProjectsByIdAsync(m.ProjectId);
                    if (project != null)
                        mappingModel.ProjectName = project.ProjectTitle;

                    var category = await _taskCategoryService.GetTaskCategoryByIdAsync(m.TaskCategoryId);
                    if (category != null)
                        mappingModel.TaskCategoryName = category.CategoryName;

                    return mappingModel;
                });
            });

            return model;
        }

        public virtual async Task<ProjectTaskCategoryMappingModel> PrepareProjectTaskCategoryMappingModelAsync(ProjectTaskCategoryMappingModel model,
            ProjectTaskCategoryMapping projectTaskCategoryMapping,
            bool excludeProperties = false)
        {
            if (projectTaskCategoryMapping != null)
            {
                if (model == null)
                    model = projectTaskCategoryMapping.ToModel<ProjectTaskCategoryMappingModel>();

                var project = await _projectsService.GetProjectsByIdAsync(model.ProjectId);
                if (project != null)
                    model.ProjectName = project.ProjectTitle;

                var category = await _taskCategoryService.GetTaskCategoryByIdAsync(model.TaskCategoryId);
                if (category != null)
                    model.TaskCategoryName = category.CategoryName;
            }

            await PrepareProjectListAsync(model);
            await PrepareTaskCategoryListAsync(model);

            return model;
        }

        #endregion
    }
}
