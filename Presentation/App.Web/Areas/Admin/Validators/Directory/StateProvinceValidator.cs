using FluentValidation;
using App.Core.Domain.Directory;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Directory;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Directory
{
    public partial class StateProvinceValidator : BaseNopValidator<StateProvinceModel>
    {
        public StateProvinceValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Countries.States.Fields.Name.Required"));

            SetDatabaseValidationRules<StateProvince>(mappingEntityAccessor);
        }
    }
}