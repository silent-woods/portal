using App.Data.Mapping;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.ProjectIntegrations;
using App.Web.Framework.Validators;
using FluentValidation;

namespace App.Web.Areas.Admin.Validators.ProjectIntegrations;

public partial class ProjectIntegrationValidator : BaseNopValidator<ProjectIntegrationModel>
{
    public ProjectIntegrationValidator(ILocalizationService localizationService, IMappingEntityAccessor mappingEntityAccessor)
    {
        RuleFor(x => x.IntegrationName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.System.ProjectIntegration.Fields.IntegrationName.Required"));

        RuleFor(x => x.SystemName)
            .NotEmpty()
            .WithMessageAwait(localizationService.GetResourceAsync("Admin.System.ProjectIntegration.Fields.SystemName.Required"));
    }
}
