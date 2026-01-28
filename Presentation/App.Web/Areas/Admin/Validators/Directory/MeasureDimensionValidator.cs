using FluentValidation;
using App.Core.Domain.Directory;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Directory;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Directory
{
    public partial class MeasureDimensionValidator : BaseNopValidator<MeasureDimensionModel>
    {
        public MeasureDimensionValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Dimensions.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Dimensions.Fields.SystemKeyword.Required"));

            SetDatabaseValidationRules<MeasureDimension>(mappingEntityAccessor);
        }
    }
}