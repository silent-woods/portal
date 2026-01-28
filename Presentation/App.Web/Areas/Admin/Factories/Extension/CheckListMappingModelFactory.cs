using App.Data.Extensions;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.CheckListMappings;
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
    /// Represents the checklist mapping model factory implementation
    /// </summary>
    public partial class CheckListMappingModelFactory : ICheckListMappingModelFactory
    {
        #region Fields

        private readonly ICheckListMappingService _checkListMappingService;
        private readonly ITaskCategoryService _taskCategoryService;
        private readonly IWorkflowStatusService _workflowStatusService;
        private readonly ICheckListMasterService _checkListService;
        private readonly IProcessWorkflowService _processWorkflowService;

        #endregion

        #region Ctor

        public CheckListMappingModelFactory(
            ICheckListMappingService checkListMappingService,
            ITaskCategoryService taskCategoryService,
            IWorkflowStatusService workflowStatusService,
            ICheckListMasterService checkListService,
            IProcessWorkflowService processWorkflowService)
        {
            _checkListMappingService = checkListMappingService;
            _taskCategoryService = taskCategoryService;
            _workflowStatusService = workflowStatusService;
            _checkListService = checkListService;
            _processWorkflowService = processWorkflowService;
        }

        #endregion

        #region Utilities

        protected virtual async Task PrepareTaskCategoryListAsync(CheckListMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableTaskCategories.Add(new SelectListItem { Text = "Select", Value = "0" });

            var categories = await _taskCategoryService.GetAllTaskCategoriesAsync(string.Empty, isActive: true);
            foreach (var c in categories)
            {
                model.AvailableTaskCategories.Add(new SelectListItem
                {
                    Text = c.CategoryName,
                    Value = c.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareStatusListAsync(CheckListMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableStatuses.Add(new SelectListItem { Text = "Select", Value = "0" });

            var statuses = await _workflowStatusService.GetAllWorkflowStatusAsync(0,"");
            foreach (var s in statuses)
            {
                model.AvailableStatuses.Add(new SelectListItem
                {
                    Text = s.StatusName,
                    Value = s.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareCheckListAsync(CheckListMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.AvailableCheckLists.Add(new SelectListItem { Text = "Select", Value = "0" });

            var checklists = await _checkListService.GetAllCheckListsAsync();
            foreach (var cl in checklists)
            {
                model.AvailableCheckLists.Add(new SelectListItem
                {
                    Text = cl.Title,
                    Value = cl.Id.ToString()
                });
            }
        }

        protected virtual async Task PrepareWorkflowListAsync(CheckListMappingModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var workflows = await _processWorkflowService.GetAllProcessWorkflowsAsync();
             
            foreach (var workflow in workflows)
            {
                model.AvailableProcessWorkflows.Add(new SelectListItem
                {
                    Text = workflow.Name,
                    Value = workflow.Id.ToString()
                });
            }
        }

        #endregion

        #region Methods

        public virtual async Task<CheckListMappingSearchModel> PrepareCheckListMappingSearchModelAsync(CheckListMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<CheckListMappingListModel> PrepareCheckListMappingListModelAsync(CheckListMappingSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var mappings = await _checkListMappingService.GetAllCheckListMappingsAsync(
                taskCategoryId: searchModel.TaskCategoryId,
                statusId: searchModel.StatusId,
                checkListId: searchModel.CheckListId,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = await new CheckListMappingListModel().PrepareToGridAsync(searchModel, mappings, () =>
            {
                return mappings.SelectAwait(async m =>
                {
                    var mappingModel = m.ToModel<CheckListMappingModel>();

                    var category = await _taskCategoryService.GetTaskCategoryByIdAsync(m.TaskCategoryId);
                    if (category != null)
                        mappingModel.TaskCategoryName = category.CategoryName;

                    var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(m.StatusId);
                    if (status != null)
                        mappingModel.StatusName = status.StatusName;

                    var checklist = await _checkListService.GetCheckListByIdAsync(m.CheckListId);
                    if (checklist != null)
                        mappingModel.CheckListName = checklist.Title;

                    return mappingModel;
                });
            });

            return model;
        }

        public virtual async Task<CheckListMappingModel> PrepareCheckListMappingModelAsync(CheckListMappingModel model,
            CheckListMapping checkListMapping,
            bool excludeProperties = false)
        {
            if (checkListMapping != null)
            {
                if (model == null)
                    model = checkListMapping.ToModel<CheckListMappingModel>();

                var category = await _taskCategoryService.GetTaskCategoryByIdAsync(model.TaskCategoryId);
                if (category != null)
                    model.TaskCategoryName = category.CategoryName;

                var status = await _workflowStatusService.GetWorkflowStatusByIdAsync(model.StatusId);
                if (status != null)
                    model.StatusName = status.StatusName;

                var checklist = await _checkListService.GetCheckListByIdAsync(model.CheckListId);
                if (checklist != null)
                    model.CheckListName = checklist.Title;
            }

            await PrepareTaskCategoryListAsync(model);
            await PrepareCheckListAsync(model);
            await PrepareWorkflowListAsync(model);

            return model;
        }

        #endregion
    }
}
