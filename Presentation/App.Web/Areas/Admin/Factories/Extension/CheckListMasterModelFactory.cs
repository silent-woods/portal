using App.Data.Extensions;
using App.Services;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Extension.CheckLists;
using App.Web.Framework.Models.Extensions;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the CheckListMaster model factory implementation
    /// </summary>
    public partial class CheckListMasterModelFactory : ICheckListMasterModelFactory
    {
        #region Fields

        private readonly ICheckListMasterService _checkListMasterService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public CheckListMasterModelFactory(
            ICheckListMasterService checkListMasterService,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _checkListMasterService = checkListMasterService;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Prepare CheckListMaster search model
        /// </summary>
        public virtual async Task<CheckListMasterSearchModel> PrepareCheckListMasterSearchModelAsync(CheckListMasterSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // set grid page size, sort, filters etc.
            searchModel.SetGridPageSize();

            return await Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare CheckListMaster list model
        /// </summary>
        public virtual async Task<CheckListMasterListModel> PrepareCheckListMasterListModelAsync(CheckListMasterSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            // get entities from service
            var checkLists = await _checkListMasterService.GetAllCheckListsAsync(
                title: searchModel.SearchTitle,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            // prepare list model
            var model = await new CheckListMasterListModel().PrepareToGridAsync(searchModel, checkLists, () =>
            {
                // fill in model values from the entity
                return checkLists.SelectAwait(async checkList =>
                {
                    var checkListModel = checkList.ToModel<CheckListMasterModel>();
                    checkListModel.CreatedOn = checkList.CreatedOn;
                    return checkListModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare CheckListMaster model for create/edit
        /// </summary>
        public virtual async Task<CheckListMasterModel> PrepareCheckListMasterModelAsync(CheckListMasterModel model, CheckListMaster entity, bool excludeProperties = false)
        {
            if (entity != null)
            {
                if (model == null)
                    model = entity.ToModel<CheckListMasterModel>();
            }

            return await Task.FromResult(model);
        }

        #endregion
    }
}
