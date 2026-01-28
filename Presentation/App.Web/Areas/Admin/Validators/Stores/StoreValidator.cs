using FluentValidation;
using App.Core.Domain.Stores;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Stores;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Stores
{
    public partial class StoreValidator : BaseNopValidator<StoreModel>
    {
        public StoreValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Stores.Fields.Name.Required"));
            RuleFor(x => x.Url).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Stores.Fields.Url.Required"));

            SetDatabaseValidationRules<Store>(mappingEntityAccessor);
        }
    }
}