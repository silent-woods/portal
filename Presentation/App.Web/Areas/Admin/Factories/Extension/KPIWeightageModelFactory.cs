using App.Core.Domain.Designations;
using App.Core.Domain.PerformanceMeasurements;
using App.Data.Extensions;
using App.Services.Designations;
using App.Services.Helpers;
using App.Services.PerformanceMeasurements;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.PerformanceMeasurements;
using App.Web.Framework.Models.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace App.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the kpiweightage model factory implementation
    /// </summary>
    public partial class KPIWeightageModelFactory : IKPIWeightageModelFactory
    {
        #region Fields

        private readonly IKPIWeightageService _kPIWeightageService;
        private readonly IKPIMasterService _kPIMasterService;
        private readonly IDesignationService _designationService;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public KPIWeightageModelFactory(IKPIWeightageService kPIWeightageService,
            IKPIMasterService kPIMasterService,
            IDesignationService designationService,
            IDateTimeHelper dateTimeHelper
            )
        {
            _kPIWeightageService = kPIWeightageService;
            _dateTimeHelper = dateTimeHelper;
            _kPIMasterService = kPIMasterService;
            _designationService = designationService;
        }

        #endregion

        #region Utilities
        public virtual async Task PrepareKPIMasterListAsync(KPIWeightageModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.KPIMaster.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var kpimasterName = "";
            var kPIMaster = await _kPIMasterService.GetAllKPIMasterAsync(kpimasterName);
            foreach (var p in kPIMaster)
            {
                model.KPIMaster.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        public virtual async Task PrepareDesignationListAsync(KPIWeightageModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            model.Designations.Add(new SelectListItem
            {
                Text = "Select",
                Value = null
            });
            var designationsName = "";
            var designation = await _designationService.GetAllDesignationAsync(designationsName);
            foreach (var p in designation)
            {
                model.Designations.Add(new SelectListItem
                {
                    Text = p.Name,
                    Value = p.Id.ToString()
                });
            }
        }
        #endregion
        #region Methods

        public virtual async Task<KPIWeightageSearchModel> PrepareKPIWeightageSearchModelAsync(KPIWeightageSearchModel searchModel)
        {
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public virtual async Task<KPIWeightageListModel> PrepareKPIWeightageListModelAsync(KPIWeightageSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get kpiweightage
            var kPIWeightage = await _kPIWeightageService.GetAllKPIWeightageAsync(kPIWeightageName: searchModel.KPIName,
                showHidden: true,

                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);
            //prepare grid model
            var model = await new KPIWeightageListModel().PrepareToGridAsync(searchModel, kPIWeightage, () =>
            {
                return kPIWeightage.SelectAwait(async kpiWeightage =>
                {
                    //fill in model values from the entity
                    var kPIWeightageModel = kpiWeightage.ToModel<KPIWeightageModel>();
                    //kPIWeightageModel.Percentages=kpiWeightage.Percentage+" %";
                    kPIWeightageModel.Percentages= $"{kpiWeightage.Percentage:F2} {"%"}";
                    kPIWeightageModel.CreateOn = await _dateTimeHelper.ConvertToUserTimeAsync(kpiWeightage.CreateOnUtc, DateTimeKind.Utc);
                    kPIWeightageModel.UpdateOn = await _dateTimeHelper.ConvertToUserTimeAsync(kpiWeightage.UpdateOnUtc, DateTimeKind.Utc);

                    KPIMaster kPIMaster = new KPIMaster();
                    kPIMaster = await _kPIMasterService.GetKPIMasterByIdAsync(kPIWeightageModel.KPIMasterId);
                    if(kPIMaster !=null)
                    kPIWeightageModel.KPIName = kPIMaster.Name;

                    Designation designation = new Designation();
                    designation = await _designationService.GetDesignationByIdAsync(kPIWeightageModel.DesignationId);
                    if(designation !=null)
                    kPIWeightageModel.DesignationName = designation.Name;

                    return kPIWeightageModel;
                });
            });

            //prepare grid model
            return model;
        }
        public virtual async Task<KPIWeightageModel> PrepareKPIWeightageModelAsync(KPIWeightageModel model, KPIWeightage kPIWeightage, bool excludeProperties = false)
        {
            if (kPIWeightage != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = kPIWeightage.ToModel<KPIWeightageModel>();
                }
            }
            await PrepareKPIMasterListAsync(model);
            await PrepareDesignationListAsync(model);
            return model;
        }
        #endregion
    }
}
