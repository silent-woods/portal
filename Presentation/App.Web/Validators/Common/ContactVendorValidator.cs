using FluentValidation;
using App.Core.Domain.Common;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Common;

namespace App.Web.Validators.Common
{
    public partial class ContactVendorValidator : BaseNopValidator<ContactVendorModel>
    {
        public ContactVendorValidator(ILocalizationService localizationService, CommonSettings commonSettings)
        {
            RuleFor(x => x.Email).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ContactVendor.Email.Required"));
            RuleFor(x => x.Email).EmailAddress().WithMessageAwait(localizationService.GetResourceAsync("Common.WrongEmail"));
            RuleFor(x => x.FullName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ContactVendor.FullName.Required"));
            if (commonSettings.SubjectFieldOnContactUsForm)
            {
                RuleFor(x => x.Subject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ContactVendor.Subject.Required"));
            }
            RuleFor(x => x.Enquiry).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("ContactVendor.Enquiry.Required"));
        }
    }
}