using App.Core.Domain.Designations;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Helpers;
using App.Web.Areas.Admin.Models.Designation;
using App.Web.Framework.Models.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the customer model factory implementation
    /// </summary>
    public partial class DesignationModelFactory : IDesignationModelFactory
    {
        #region Fields

        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public DesignationModelFactory(IDesignationService designationService,
            IDateTimeHelper dateTimeHelper
            )
        {
            _designationService = designationService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<DesignationSearchModel> PrepareDesignationSearchModelAsync(DesignationSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<DesignationListModel> PrepareDesignationListModelAsync(DesignationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get designation
            var designation = await _designationService.GetAllDesignationAsync(designationName: searchModel.SearchDesignationName,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new DesignationListModel().PrepareToGridAsync(searchModel, designation, () =>
            {
                return designation.SelectAwait(async designation =>
                {
                    //fill in model values from the entity
                    var designationModel = new DesignationModel();
                    designationModel.Id = designation.Id;
                    designationModel.Name = designation.Name;
                    designationModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(designation.CreateOnUtc, DateTimeKind.Utc);
                    designationModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(designation.UpdateOnUtc, DateTimeKind.Utc);
                    designationModel.CanGiveRatings = designation.CanGiveRatings;

                    return designationModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<DesignationModel> PrepareDesignationModelAsync(DesignationModel model, Designation designation, bool excludeProperties = false)
        {
            if (designation != null)
            {
                //fill in model values from the entity
                model ??= new DesignationModel();
                model = new DesignationModel();
                model.Id = designation.Id;
                model.Name = designation.Name;
                model.CreateOn = designation.CreateOnUtc;
                model.UpdateOn = designation.UpdateOnUtc;
                model.CanGiveRatings = designation.CanGiveRatings;
            }
            return model;
        }
        #endregion
    }
}
