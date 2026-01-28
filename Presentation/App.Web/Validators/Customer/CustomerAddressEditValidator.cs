using FluentValidation;
using App.Core.Domain.Common;
using App.Core.Domain.Customers;
using App.Services.Directory;
using App.Services.Localization;
using App.Web.Models.Customer;
using App.Web.Validators.Common;

namespace App.Web.Validators.Customer
{
    public partial class CustomerAddressEditValidator : AbstractValidator<CustomerAddressEditModel>
    {
        public CustomerAddressEditValidator(ILocalizationService localizationService,
            IStateProvinceService stateProvinceService,
            AddressSettings addressSettings,
            CustomerSettings customerSettings)
        {
            RuleFor(model => model.Address).SetValidator(new AddressValidator(localizationService, stateProvinceService, addressSettings, customerSettings));
        }
    }
}
