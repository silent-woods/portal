using App.Core.Domain.Designations;
using App.Core.Domain.PerformanceMeasurements;
using App.Core.Domain.TimeSheets;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Helpers;
using App.Services.PerformanceMeasurements;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Designation;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Areas.Admin.Models.TimeSheets;
using App.Web.Framework.Models.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the kpimaster model factory implementation
    /// </summary>
    public partial class KPIMasterModelFactory : IKPIMasterModelFactory
    {
        #region Fields

        private readonly IKPIMasterService _kPIMasterService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public KPIMasterModelFactory(IKPIMasterService kPIMasterService,
            IDateTimeHelper dateTimeHelper
            )
        {
            _kPIMasterService = kPIMasterService;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Methods

        public virtual async Task<KPIMasterSearchModel> PrepareKPIMasterSearchModelAsync(KPIMasterSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<KPIMasterListModel> PrepareKPIMasterListModelAsync(KPIMasterSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get kpimaster
            var kPIMaster = await _kPIMasterService.GetAllKPIMasterAsync(kpiName: searchModel.kpiName,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new KPIMasterListModel().PrepareToGridAsync(searchModel, kPIMaster, () =>
            {
                return kPIMaster.SelectAwait(async kpiMaster =>
                {
                    //fill in model values from the entity
                    var kPIMasterModel = kpiMaster.ToModel<KPIMasterModel>();
                    kPIMasterModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(kpiMaster.CreateOnUtc, DateTimeKind.Utc);
                    kPIMasterModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(kpiMaster.UpdateOnUtc, DateTimeKind.Utc);

                    return kPIMasterModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<KPIMasterModel> PrepareKPIMasterModelAsync(KPIMasterModel model, KPIMaster kPIMaster, bool excludeProperties = false)
        {
            if (kPIMaster != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = kPIMaster.ToModel<KPIMasterModel>();
                }
            }
            return model;
        }
        #endregion
    }
}
