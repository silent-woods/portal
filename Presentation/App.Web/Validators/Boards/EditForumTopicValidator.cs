using FluentValidation;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Boards;

namespace App.Web.Validators.Boards
{
    public partial class EditForumTopicValidator : BaseNopValidator<EditForumTopicModel>
    {
        public EditForumTopicValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Subject).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Forum.TopicSubjectCannotBeEmpty"));
            RuleFor(x => x.Text).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Forum.TextCannotBeEmpty"));
        }
    }
}