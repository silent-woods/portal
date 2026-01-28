using FluentValidation;
using App.Core.Domain.Customers;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Customers
{
    public partial class CustomerAttributeValidator : BaseNopValidator<CustomerAttributeModel>
    {
        public CustomerAttributeValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Customers.CustomerAttributes.Fields.Name.Required"));

            SetDatabaseValidationRules<CustomerAttribute>(mappingEntityAccessor);
        }
    }
}