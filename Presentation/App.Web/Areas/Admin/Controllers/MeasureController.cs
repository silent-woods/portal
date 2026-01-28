using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using App.Core.Domain.Directory;
using App.Services.Configuration;
using App.Services.Directory;
using App.Services.Localization;
using App.Services.Logging;
using App.Services.Security;
using App.Web.Areas.Admin.Factories;
using App.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using App.Web.Areas.Admin.Models.Directory;
using App.Web.Framework.Mvc;
using App.Web.Framework.Mvc.ModelBinding;

namespace App.Web.Areas.Admin.Controllers
{
    public partial class MeasureController : BaseAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IMeasureModelFactory _measureModelFactory;
        private readonly IMeasureService _measureService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly MeasureSettings _measureSettings;

        #endregion

        #region Ctor

        public MeasureController(ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            IMeasureModelFactory measureModelFactory,
            IMeasureService measureService,
            IPermissionService permissionService,
            ISettingService settingService,
            MeasureSettings measureSettings)
        {
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _measureModelFactory = measureModelFactory;
            _measureService = measureService;
            _permissionService = permissionService;
            _settingService = settingService;
            _measureSettings = measureSettings;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> List()
        {
            //prepare model
            var model = await _measureModelFactory.PrepareMeasureSearchModelAsync(new MeasureSearchModel());

            return View(model);
        }

        #region Weights

        [HttpPost]
        public virtual async Task<IActionResult> Weights(MeasureWeightSearchModel searchModel)
        {
          
            //prepare model
            var model = await _measureModelFactory.PrepareMeasureWeightListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> WeightUpdate(MeasureWeightModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var weight = await _measureService.GetMeasureWeightByIdAsync(model.Id);
            weight = model.ToEntity(weight);
            await _measureService.UpdateMeasureWeightAsync(weight);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditMeasureWeight",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditMeasureWeight"), weight.Id), weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> WeightAdd(MeasureWeightModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var weight = new MeasureWeight();
            weight = model.ToEntity(weight);
            await _measureService.InsertMeasureWeightAsync(weight);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewMeasureWeight",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewMeasureWeight"), weight.Id), weight);

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> WeightDelete(int id)
        {
            //try to get a weight with the specified id
            var weight = await _measureService.GetMeasureWeightByIdAsync(id)
                ?? throw new ArgumentException("No weight found with the specified id", nameof(id));

            if (weight.Id == _measureSettings.BaseWeightId)
            {
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Weights.CantDeletePrimary"));
            }

            await _measureService.DeleteMeasureWeightAsync(weight);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteMeasureWeight",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteMeasureWeight"), weight.Id), weight);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> MarkAsPrimaryWeight(int id)
        {
            //try to get a weight with the specified id
            var weight = await _measureService.GetMeasureWeightByIdAsync(id)
                ?? throw new ArgumentException("No weight found with the specified id", nameof(id));

            _measureSettings.BaseWeightId = weight.Id;
            await _settingService.SaveSettingAsync(_measureSettings);

            return Json(new { result = true });
        }

        #endregion

        #region Dimensions

        [HttpPost]
        public virtual async Task<IActionResult> Dimensions(MeasureDimensionSearchModel searchModel)
        {
          
            //prepare model
            var model = await _measureModelFactory.PrepareMeasureDimensionListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DimensionUpdate(MeasureDimensionModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var dimension = await _measureService.GetMeasureDimensionByIdAsync(model.Id);
            dimension = model.ToEntity(dimension);
            await _measureService.UpdateMeasureDimensionAsync(dimension);

            //activity log
            await _customerActivityService.InsertActivityAsync("EditMeasureDimension",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditMeasureDimension"), dimension.Id), dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DimensionAdd(MeasureDimensionModel model)
        {
            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            var dimension = new MeasureDimension();
            dimension = model.ToEntity(dimension);
            await _measureService.InsertMeasureDimensionAsync(dimension);

            //activity log
            await _customerActivityService.InsertActivityAsync("AddNewMeasureDimension",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewMeasureDimension"), dimension.Id), dimension);

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> DimensionDelete(int id)
        {
            //try to get a dimension with the specified id
            var dimension = await _measureService.GetMeasureDimensionByIdAsync(id)
                ?? throw new ArgumentException("No dimension found with the specified id", nameof(id));

            if (dimension.Id == _measureSettings.BaseDimensionId)
            {
                return ErrorJson(await _localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Dimensions.CantDeletePrimary"));
            }

            await _measureService.DeleteMeasureDimensionAsync(dimension);

            //activity log
            await _customerActivityService.InsertActivityAsync("DeleteMeasureDimension",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteMeasureDimension"), dimension.Id), dimension);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> MarkAsPrimaryDimension(int id)
        {
            //try to get a dimension with the specified id
            var dimension = await _measureService.GetMeasureDimensionByIdAsync(id)
                ?? throw new ArgumentException("No dimension found with the specified id", nameof(id));

            _measureSettings.BaseDimensionId = dimension.Id;
            await _settingService.SaveSettingAsync(_measureSettings);

            return Json(new { result = true });
        }

        #endregion

        #endregion
    }
}