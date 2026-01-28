using FluentValidation;
using App.Core.Domain.Common;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Customers;
using App.Web.Areas.Admin.Validators.Common;

namespace App.Web.Areas.Admin.Validators.Customers
{
    public partial class CustomerAddressValidator : AbstractValidator<CustomerAddressModel>
    {
        public CustomerAddressValidator(ILocalizationService localizationService,
            IMappingEntityAccessor mappingEntityAccessor,
            AddressSettings addressSettings)
        {
            RuleFor(model => model.Address).SetValidator(new AddressValidator(addressSettings, localizationService, mappingEntityAccessor));
        }
    }
}
