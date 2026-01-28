using FluentValidation;
using App.Services.Localization;
using App.Web.Framework.Validators;
using App.Web.Models.Boards;

namespace App.Web.Validators.Boards
{
    public partial class EditForumPostValidator : BaseNopValidator<EditForumPostModel>
    {
        public EditForumPostValidator(ILocalizationService localizationService)
        {            
            RuleFor(x => x.Text).NotEmpty().WithMessageAwait(localizationService.GetResourceAsync("Forum.TextCannotBeEmpty"));
        }
    }
}