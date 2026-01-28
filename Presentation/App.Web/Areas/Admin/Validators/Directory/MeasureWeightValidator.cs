using FluentValidation;
using App.Core.Domain.Directory;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Directory;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Directory
{
    public partial class MeasureWeightValidator : BaseNopValidator<MeasureWeightModel>
    {
        public MeasureWeightValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Weights.Fields.Name.Required"));
            RuleFor(x => x.SystemKeyword).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Shipping.Measures.Weights.Fields.SystemKeyword.Required"));

            SetDatabaseValidationRules<MeasureWeight>(mappingEntityAccessor);
        }
    }
}