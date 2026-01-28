using FluentValidation;
using App.Services.Localization;
using App.Web.Areas.Admin.Models.Plugins;
using App.Web.Framework.Validators;

namespace App.Web.Areas.Admin.Validators.Plugins
{
    public partial class PluginValidator : BaseNopValidator<PluginModel>
    {
        public PluginValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.FriendlyName).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Admin.Configuration.Plugins.Fields.FriendlyName.Required"));
        }
    }
}