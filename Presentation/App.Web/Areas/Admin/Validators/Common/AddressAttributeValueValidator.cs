using FluentValidation;
using App.Core.Domain.Common;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Common;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Common
{
    public partial class AddressAttributeValueValidator : BaseNopValidator<AddressAttributeValueModel>
    {
        public AddressAttributeValueValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.AddressAttributes.Values.Fields.Name.Required"));

            SetDatabaseValidationRules<AddressAttributeValue>(mappingEntityAccessor);
        }
    }
}