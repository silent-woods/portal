using FluentValidation;
using App.Core.Domain.Common;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Common;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Common
{
    public partial class AddressAttributeValidator : BaseNopValidator<AddressAttributeModel>
    {
        public AddressAttributeValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.AddressAttributes.Fields.Name.Required"));

            SetDatabaseValidationRules<AddressAttribute>(mappingEntityAccessor);
        }
    }
}