using FluentValidation;
using App.Core.Domain.Messages;
using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Messages;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Messages
{
    public partial class CampaignValidator : BaseNopValidator<CampaignModel>
    {
        public CampaignValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotions.Campaigns.Fields.Name.Required"));

            RuleFor(x => x.Subject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotions.Campaigns.Fields.Subject.Required"));

            RuleFor(x => x.Body).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Promotions.Campaigns.Fields.Body.Required"));

            SetDatabaseValidationRules<Campaign>(mappingEntityAccessor);
        }
    }
}