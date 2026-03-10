using App.Data.Extensions;
using App.Services.Helpers;
using App.Web.Areas.Admin.Models.Extension.Technologys;
using App.Web.Framework.Models.Extensions;
using Satyanam.Nop.Core.Domains;
using Satyanam.Nop.Core.Services;
using System;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the Technology model factory implementation
    /// </summary>
    public partial class TechnologyModelFactory : ITechnologyModelFactory
    {
        #region Fields

        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ITechnologyService _technologyService;
        #endregion

        #region Ctor

        public TechnologyModelFactory(IDateTimeHelper dateTimeHelper,
            ITechnologyService technologyService)
        {
            _dateTimeHelper = dateTimeHelper;
            _technologyService = technologyService;
        }

        #endregion

        #region Methods
        public Task<TechnologySearchModel> PrepareTechnologySearchModelAsync(TechnologySearchModel searchModel)
        {
            searchModel ??= new TechnologySearchModel();
            searchModel.SetGridPageSize();
            return Task.FromResult(searchModel);
        }

        public async Task<TechnologyListModel> PrepareTechnologyListModelAsync(TechnologySearchModel searchModel)
        {
            var technologies = await _technologyService.GetAllTechnologyAsync(
                searchModel.SearchName,
                null,
                searchModel.Page - 1,
                searchModel.PageSize);

            var model = await ModelExtensions.PrepareToGridAsync<TechnologyListModel, TechnologyModel, Technology>(
                new TechnologyListModel(),
                searchModel,
                technologies,
                () =>
                {
                    return technologies.SelectAwait(async entity =>
                    {
                        return new TechnologyModel
                        {
                            Id = entity.Id,
                            Name = entity.Name,
                            DisplayOrder = entity.DisplayOrder,
                            Published = entity.Published,
                            CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(entity.CreatedOnUtc, DateTimeKind.Utc),
                            UpdatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(entity.UpdatedOnUtc, DateTimeKind.Utc)
                        };
                    });
                });

            return model;
        }

        public Task<TechnologyModel> PrepareTechnologyModelAsync(TechnologyModel model, Technology entity, bool excludeProperties = false)
        {
            if (entity != null)
            {
                model ??= new TechnologyModel();
                model.Id = entity.Id;
                model.Name = entity.Name;
                model.DisplayOrder = entity.DisplayOrder;
                model.Published = entity.Published;
                model.CreatedOnUtc = entity.CreatedOnUtc;
                model.UpdatedOnUtc = entity.UpdatedOnUtc;
            }
            return Task.FromResult(model);
        }

        #endregion
    }
}