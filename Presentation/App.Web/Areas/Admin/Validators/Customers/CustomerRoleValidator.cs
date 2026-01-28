using FluentValidation;
using App.Core.Domain.Customers;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Customers
{
    public partial class CustomerRoleValidator : BaseNopValidator<CustomerRoleModel>
    {
        public CustomerRoleValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Customers.CustomerRoles.Fields.Name.Required"));

            SetDatabaseValidationRules<CustomerRole>(mappingEntityAccessor);
        }
    }
}